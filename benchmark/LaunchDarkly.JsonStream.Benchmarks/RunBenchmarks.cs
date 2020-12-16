using BenchmarkDotNet.Running;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    public class RunBenchmarks
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JReaderWriterBenchmark>();
            BenchmarkRunner.Run<JsonNetReflectionComparativeBenchmark>();
            BenchmarkRunner.Run<JsonNetStreamingComparativeBenchmark>();
        }
    }
}
