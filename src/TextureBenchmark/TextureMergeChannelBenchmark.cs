using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureUtilities;

namespace TextureBenchmark
{
    public class TextureMergeChannelBenchmark
    {
        public const string specularColorStr = "#ff0000";
        public const string specularMapPath = @".\Textures\painted_metal_chipped_Specular.png";
        public const string glossinessStr = "1";
        public const string glossinessMapPath = @".\Textures\painted_metal_chipped_Glossiness.png";


        static readonly float[] ColorDefault = { 1f, 1f, 1f };
        const float GlossinessDefault = 1f;

        static internal Bitmap LoadMap(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var f = new FileInfo(path);
                if (f.Exists)
                {
                    try
                    {
                        return new Bitmap(path);
                    }
                    catch
                    {
                        Console.WriteLine($"Unable to load map [{path}].");
                    }
                }
                else
                {
                    Console.WriteLine($"Map not found [{path}].");
                }
            }
            return null;
        }

        static internal float[] ParseColor(string colorStr)
        {
            if (!string.IsNullOrEmpty(colorStr))
            {
                try
                {
                    var c = ColorTranslator.FromHtml(colorStr);
                    float r = c.R;
                    float g = c.G;
                    float b = c.B;
                    return new float[] { r / 255, g / 255, b / 255 };
                }
                catch
                {
                    Console.WriteLine($"Unable to parse specular color [{colorStr}].");
                }
            }
            return null;
        }

        static internal float? ParseGloss(string glossStr)
        {
            if (!string.IsNullOrEmpty(glossStr))
            {
                if (Single.TryParse(glossStr, out float v))
                {
                    return v;
                }
                Console.WriteLine($"Unable to parse glossiness [{glossStr}].");
            }
            return null;
        }


        public float[] _specularColor;
        public Bitmap _specularMap;
        public float _glossiness;
        public Bitmap _glossinessMap;

        [GlobalSetup]
        public void Init()
        {
            _specularColor = ParseColor(specularColorStr) ?? ColorDefault;
            _specularMap = LoadMap(specularMapPath);
            _glossiness = ParseGloss(glossinessStr) ?? GlossinessDefault;
            _glossinessMap = LoadMap(glossinessMapPath);
        }

        [Benchmark]
        public void MergeSetGetPixel()
        {
            var result = SpecularGlossinessUtilities.MergeSetGetPixel(_specularColor, _specularMap, _glossiness, _glossinessMap);
        }
    
        public void MergeLockbitsCopy()
        {
            var result = SpecularGlossinessUtilities.MergeLockbitsCopy(_specularColor, _specularMap, _glossiness, _glossinessMap);
        }

        public void MergeLockbitsUnsafe()
        {
            var result = SpecularGlossinessUtilities.MergeLockbitsUnsafe(_specularColor, _specularMap, _glossiness, _glossinessMap);
        }

        public void MergeLockbitsUnsafeTpl()
        {
            var result = SpecularGlossinessUtilities.MergeLockbitsUnsafeTpl(_specularColor, _specularMap, _glossiness, _glossinessMap);
        }
    }
}
