using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TextureUtilities
{
    public class SpecularGlossinessUtilities
    {
        /// <summary>
        /// Merge specular glossiness infos into one sole texture using RGB as specular values and Alpha channel as glossiness.
        /// Processing using Bitmap.GetPixel and Bitmap.SetPixel
        /// </summary>
        /// <param name="specularColor">an RGB value, used if specular map is undefined. Values are defined in the range of [0,1].</param>
        /// <param name="specularMap">the specular map where we get RGB component from</param>
        /// <param name="glossiness">a value to specify glossiness if the glossiness map is undefined</param>
         /// <returns></returns>
        public static Bitmap Merge(float [] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
        {
            if(specularMap == null && glossinessMap == null)
            {
                throw new ArgumentException("Either specular or glossiness texture MUST be present.");
            }

            var reference = specularMap ?? glossinessMap;
            int width = reference.Width;
            int height = reference.Height;

            Bitmap newBitmap = new Bitmap(width, height);
            // prepare optional color and factor
            // this can be optimized but have near to zero impact on performance
            Color rgbColor = Color.FromArgb(1, (int)(specularColor[0]*255), (int)(specularColor[1] * 255), (int)(specularColor[2] * 255));
            int glossinessFactor = (int)(glossiness * 255);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int alpha = glossinessMap?.GetPixel(x, y).R ?? glossinessFactor;
                    Color rgb = specularMap?.GetPixel(x, y) ?? rgbColor;
                    Color color = Color.FromArgb(alpha, rgb);
                    newBitmap.SetPixel(x, y, color);
                }
            }
            return newBitmap;
        }

        /// <summary>
        /// Merge specular glossiness infos into one sole texture using RGB as specular values and Alpha channel as glossiness.
        /// Processing using Bitmap.Lockbits and Marshal.Copy
        /// </summary>
        /// <param name="specularColor">an RGB value, used if specular map is undefined. Values are defined in the range of [0,1].</param>
        /// <param name="specularMap">the specular map where we get RGB component from</param>
        /// <param name="glossiness">a value to specify glossiness if the glossiness map is undefined</param>
        /// <returns></returns>
        public static Bitmap FastMerge(float[] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
        {
            if (specularMap != null)
            {
                return glossinessMap != null ? FastMerge(specularMap, glossinessMap) // both map
                                             : FastMerge(specularMap, glossiness   );// specular only with glossiness factor
            }
            if (glossinessMap != null)
            {
                // glossiness only + specular color
                return FastMerge(specularColor, glossinessMap);
            }

            //at least one of the map must be set
            return null;
        }

        private static Bitmap FastMerge(Bitmap specularMap, Bitmap glossinessMap)
        {
            unsafe
            {
                Rectangle rect = new Rectangle(0, 0, specularMap.Width, specularMap.Height);

                // glossiness
                // --------
                BitmapData glossinessMapData = glossinessMap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, specularMap.PixelFormat);
                // Get the address of the first line.
                byte * glossinessPtr = (byte*) glossinessMapData.Scan0;
                // different operation depending on pixel format
                int glossinessPixelSize = Image.GetPixelFormatSize(glossinessMapData.PixelFormat) >> 3;

                // we choose the byte offset to get the glossiness value
                int glossinessOffset = 0;
                switch (glossinessMapData.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Canonical:
                    case PixelFormat.Format32bppRgb:
                        {
                            break;
                        }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        {
                            glossinessOffset++;
                            break;
                        }
                    default:
                        throw new NotSupportedException($"Pixel format not supported :{glossinessMapData.PixelFormat}");
                }

                var target = new uint[rect.Height* rect.Width] ;

                // specular
                // --------
                BitmapData specularMapData = specularMap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, specularMap.PixelFormat);
                // Get the address of the first line.
                byte* specularPtr = (byte*)specularMapData.Scan0;
                // different operation depending on pixel format
                int specularPixelSize = Image.GetPixelFormatSize(specularMapData.PixelFormat) >> 3;
                
                switch (specularMapData.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        {
                            Parallel.For(0, specularMapData.Height, row =>
                            {
                                byte* gPtr = glossinessPtr + (row * glossinessMapData.Stride + glossinessOffset);
                                var offset = row * rect.Width;
                                byte* rgbPtr = specularPtr + (row * specularMapData.Stride);
                                for (int i = offset; i != offset + rect.Width; i++)
                                {
                                    var a = *gPtr;
                                    var r = *(rgbPtr++);
                                    var g = *(rgbPtr++);
                                    var b = *(rgbPtr++);
                                    target[i] =  (uint)(a<<24) + (uint)(r <<16) + (uint)(g <<8) + (uint)b;
                                    gPtr += glossinessPixelSize;
                                }
                            });
                            break;
                        }
                    case PixelFormat.Canonical:
                    case PixelFormat.Format32bppRgb:
                        {
                            Parallel.For(0, specularMapData.Height, row =>
                            {
                                byte* gPtr = glossinessPtr + (row * glossinessMapData.Stride + glossinessOffset);
                                var offset = row * rect.Width;
                                uint* rgbPtr = ((uint*)specularPtr) + offset;
                                for (int i = offset; i != offset + rect.Width; i++)
                                {
                                    target[i] = ((*rgbPtr) >> 8) | (uint)((*gPtr) << 24);
                                    rgbPtr++;
                                    gPtr += glossinessPixelSize;
                                }
                            });
                            break;
                        }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        {
                            Parallel.For(0, specularMapData.Height, row =>
                            {
                                byte* gPtr = glossinessPtr + (row * glossinessMapData.Stride + glossinessOffset);
                                var offset = row * rect.Width;
                                uint* rgbPtr = ((uint*)specularPtr) + offset ;
                                for (int i = offset; i != offset + rect.Width; i++)
                                {
                                    target[i] = ((*rgbPtr) & 0x00FFFFFF) | (uint)((*gPtr) << 24);
                                    rgbPtr++;
                                    gPtr += glossinessPixelSize;
                                }
                            });
                            break;
                        }
                    default:
                        throw new NotSupportedException($"Pixel format not supported :{specularMapData.PixelFormat}");
                }

                // unlock
                specularMap.UnlockBits(specularMapData);
                glossinessMap.UnlockBits(glossinessMapData);

                // build new bitmap
                var targetPixelSize = 4;
                var targetStride = rect.Width * targetPixelSize;
                var targetPtr = Marshal.UnsafeAddrOfPinnedArrayElement(target, 0);

                return new Bitmap(rect.Width, rect.Height, targetStride, PixelFormat.Format32bppArgb, targetPtr);
            } // unsafe
        }

        private static Bitmap FastMerge(Bitmap specularMap, float glossiness)
        {
            return default;
        }
        private static Bitmap FastMerge(float[] specularColor, Bitmap glossinessMap)
        {
            return default;
        }
    }
}
