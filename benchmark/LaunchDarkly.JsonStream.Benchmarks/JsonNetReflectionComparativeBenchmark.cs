using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

using static LaunchDarkly.JsonStream.Benchmarks.BenchmarkData;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    [MemoryDiagnoser]
    public class JsonNetReflectionComparativeBenchmark
    {
        [Benchmark]
        public void ReadBools()
        {
            var bools = JsonConvert.DeserializeObject<bool[]>(ListOfBoolsJson);
        }

        [Benchmark]
        public void ReadInts()
        {
            var ints = JsonConvert.DeserializeObject<int[]>(ListOfIntsJson);
        }

        [Benchmark]
        public void ReadStructs()
        {
            var ts = JsonConvert.DeserializeObject<TestStruct>(ListOfStructsJson);
        }

        [Benchmark]
        public void WriteBools()
        {
            var s = JsonConvert.SerializeObject(ListOfBools);
        }

        [Benchmark]
        public void WriteInts()
        {
            var s = JsonConvert.SerializeObject(ListOfInts);
        }

        [Benchmark]
        public void WriteStructs()
        {
            var s = JsonConvert.SerializeObject(ListOfStructs);
        }
    }
}
