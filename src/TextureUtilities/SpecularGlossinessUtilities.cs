using System;
using System.Drawing;
using System.Linq;

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
        public static Bitmap MergeSetGetPixel(float [] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
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
        public static Bitmap MergeLockbitsCopy(float[] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
        {
            return default;
        }

        /// <summary>
        /// Merge specular glossiness infos into one sole texture using RGB as specular values and Alpha channel as glossiness.
        /// Processing using Bitmap.LockBits and direct memory manipulation in unsafe context
        /// </summary>
        /// <param name="specularColor">an RGB value, used if specular map is undefined. Values are defined in the range of [0,1].</param>
        /// <param name="specularMap">the specular map where we get RGB component from</param>
        /// <param name="glossiness">a value to specify glossiness if the glossiness map is undefined</param>
        /// <returns></returns>
        public unsafe static Bitmap MergeLockbitsUnsafe(float[] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
        {
            return default;
        }

        /// <summary>
        /// Merge specular glossiness infos into one sole texture using RGB as specular values and Alpha channel as glossiness.
        /// Processing using Bitmap.LockBits and direct memory manipulation in unsafe context with Task Parallel Library
        /// </summary>
        /// <param name="specularColor">an RGB value, used if specular map is undefined. Values are defined in the range of [0,1].</param>
        /// <param name="specularMap">the specular map where we get RGB component from</param>
        /// <param name="glossiness">a value to specify glossiness if the glossiness map is undefined</param>
        /// <returns></returns>
        public unsafe static Bitmap MergeLockbitsUnsafeTpl(float[] specularColor, Bitmap specularMap, float glossiness, Bitmap glossinessMap)
        {
            return default;
        }
    }
}
