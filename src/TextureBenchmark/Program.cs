using BenchmarkDotNet.Running;
#if USEPARAMS
using Microsoft.Extensions.Configuration;
#endif

namespace TextureBenchmark
{
    public class Program
    {
#if USEPARAMS
        const string SpecularColoKey = "specularcolor";
        const string SpecularMapKey = "specularmap";
        const string GlossinessFactorKey = "glossinessfactor";
        const string GlossinessMapKey = "glossinessmap";
#endif
        public static void Main(string[] args)
        {
#if USEPARAMS
            var switchMappings = new Dictionary<string, string>()
            {
                { "-sc", SpecularColoKey },
                { "-sm", SpecularMapKey },
                { "-gf", GlossinessFactorKey },
                { "-gm", GlossinessMapKey }
            };
            var commandLineConfig = new ConfigurationBuilder().AddCommandLine(args, switchMappings).Build();
#endif
            var summary = BenchmarkRunner.Run<TextureMergeChannelBenchmark>();
        }
    }
}
