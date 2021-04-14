using BenchmarkDotNet.Attributes;
using System;
using System.Drawing;
using System.IO;
using TextureUtilities;

namespace TextureBenchmark
{
    public class TextureMergeChannelBenchmark
    {
        // remember that Benchmark .net do not provide any way to pass parameters into constructor
        // work arround is to setting up static parameters it serve all the instances.

        public const string specularColorStr = "#ff0000";
        public const string specularMapPath = @".\Textures\painted_metal_chipped_Specular.png";
        public const string glossinessStr = "1";
        public const string countStr = "10";
        public const string glossinessMapPath = @".\Textures\painted_metal_chipped_Glossiness.png";

        static readonly float[] ColorDefault = { 1f, 1f, 1f };
        const float GlossinessDefault = 1f;
        const int countDefault = 1;

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
                    Console.WriteLine($"Unable to parse color [{colorStr}].");
                }
            }
            return null;
        }

        static internal float? ParseFloat(string floatStr)
        {
            if (!string.IsNullOrEmpty(floatStr))
            {
                if (Single.TryParse(floatStr, out float v))
                {
                    return v;
                }
                Console.WriteLine($"Unable to parse [{floatStr}].");
            }
            return null;
        }

        static internal int? ParseInt(string intStr)
        {
            if (!string.IsNullOrEmpty(intStr))
            {
                if (int.TryParse(intStr, out int v))
                {
                    return v;
                }
                Console.WriteLine($"Unable to parse [{intStr}].");
            }
            return null;
        }

        public float[] _specularColor;
        public Bitmap _specularMap;
        public float _glossiness;
        public Bitmap _glossinessMap;
        public int _count;

        [GlobalSetup]
        public void Init()
        {
            // load & parse necessary data
            _specularColor = ParseColor(specularColorStr) ?? ColorDefault;
            _specularMap = LoadMap(specularMapPath);
            _glossiness = ParseFloat(glossinessStr) ?? GlossinessDefault;
            _glossinessMap = LoadMap(glossinessMapPath);
            _count = ParseInt(countStr) ?? countDefault;
        }

        [Benchmark]
        public void MergeGetSetPixel()
        {
            for (var i = 0; i != _count; i++)
            {
                var result = SpecularGlossinessUtilities.Merge(_specularColor, _specularMap, _glossiness, _glossinessMap);
            }
        }

        [Benchmark]
        public void MergeLockbits()
        {
            for (var i = 0; i != _count; i++)
            {
                var result = SpecularGlossinessUtilities.FastMerge(_specularColor, _specularMap, _glossiness, _glossinessMap);
            }
        }
    }
}
