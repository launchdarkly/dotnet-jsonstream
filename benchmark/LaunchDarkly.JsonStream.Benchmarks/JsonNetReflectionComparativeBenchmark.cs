using System.Collections.Generic;
using System.Text;
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
            var bools = JsonConvert.DeserializeObject<List<bool>>(ListOfBoolsJson);
        }

        [Benchmark]
        public void ReadInts()
        {
            var ints = JsonConvert.DeserializeObject<List<int>>(ListOfIntsJson);
        }

        [Benchmark]
        public void ReadStructs()
        {
            var ts = JsonConvert.DeserializeObject<List<TestStruct>>(ListOfStructsJson);
        }

        [Benchmark]
        public void ReadBoolsUtf8()
        {
            var bools = JsonConvert.DeserializeObject<List<bool>>(Encoding.UTF8.GetString(ListOfBoolsJsonUtf8));
        }

        [Benchmark]
        public void ReadIntsUtf8()
        {
            var ints = JsonConvert.DeserializeObject<List<int>>(Encoding.UTF8.GetString(ListOfIntsJsonUtf8));
        }

        [Benchmark]
        public void ReadStructsUtf8()
        {
            var ts = JsonConvert.DeserializeObject<List<TestStruct>>(Encoding.UTF8.GetString(ListOfStructsJsonUtf8));
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

        [Benchmark]
        public void WriteBoolsUtf8()
        {
            var s = JsonConvert.SerializeObject(ListOfBools);
            var b = Encoding.UTF8.GetBytes(s);
        }

        [Benchmark]
        public void WriteIntsUtf8()
        {
            var s = JsonConvert.SerializeObject(ListOfInts);
            var b = Encoding.UTF8.GetBytes(s);
        }

        [Benchmark]
        public void WriteStructsUtf8()
        {
            var s = JsonConvert.SerializeObject(ListOfStructs);
            var b = Encoding.UTF8.GetBytes(s);
        }
    }
}
