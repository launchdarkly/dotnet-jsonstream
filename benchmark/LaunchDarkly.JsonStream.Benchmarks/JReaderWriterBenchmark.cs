using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

using static LaunchDarkly.JsonStream.Benchmarks.BenchmarkData;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    [MemoryDiagnoser]
    public class JReaderWriterBenchmark
    {
        [Benchmark]
        public void ReadBools()
        {
            var r = JReader.FromString(ListOfBoolsJson);
            var bools = ReadBools(ref r);
        }

        [Benchmark]
        public void ReadInts()
        {
            var r = JReader.FromString(ListOfIntsJson);
            var ints = ReadInts(ref r);
        }

        [Benchmark]
        public void ReadStruct()
        {
            var r = JReader.FromString(StructJson);
            var ts = ReadStruct(ref r);
        }

        [Benchmark]
        public void WriteBools()
        {
            var w = JWriter.New();
            WriteBools(w, ListOfBools);
            var s = w.GetString();
        }

        [Benchmark]
        public void WriteInts()
        {
            var w = JWriter.New();
            WriteInts(w, ListOfInts);
            var s = w.GetString();
        }

        [Benchmark]
        public void WriteStruct()
        {
            var w = JWriter.New();
            WriteTestStruct(w, Struct);
            var s = w.GetString();
        }

        private List<bool> ReadBools(ref JReader r)
        {
            var ret = new List<bool>();
            for (var arr = r.Array(); arr.Next(ref r);)
            {
                ret.Add(r.Bool());
            }
            return ret;
        }

        private List<int> ReadInts(ref JReader r)
        {
            var ret = new List<int>();
            for (var arr = r.Array(); arr.Next(ref r);)
            {
                ret.Add(r.Int());
            }
            return ret;
        }

        private TestStruct ReadStruct(ref JReader r)
        {
            var obj = r.ObjectOrNull();
            if (!obj.IsDefined)
            {
                return null;
            }
            var ret = new TestStruct();
            while (obj.Next(ref r))
            {
                var name = obj.Name;
                if (name == "str")
                {
                    ret.Str = r.String();
                }
                else if (name == "nums")
                {
                    ret.Nums = ReadInts(ref r);
                }
                else if (name == "bool")
                {
                    ret.Bool = r.Bool();
                }
                else if (name == "nested")
                {
                    ret.Nested = ReadStruct(ref r);
                }
            }
            return ret;
        }

        private void WriteBools(IValueWriter w, List<bool> bools)
        {
            using (var arr = w.Array())
            {
                for (var i = 0; i < bools.Count; i++)
                {
                    w.Bool(bools[i]);
                }
            }
        }

        private void WriteInts(IValueWriter w, List<int> ints)
        {
            using (var arr = w.Array())
            {
                for (var i = 0; i < ints.Count; i++)
                {
                    w.Int(ints[i]);
                }
            }
        }

        private void WriteTestStruct(IValueWriter w, TestStruct ts)
        {
            using (var obj = w.Object())
            {
                obj.Property("str").String(ts.Str);
                WriteInts(obj.Property("nums"), ts.Nums);
                obj.Property("bool").Bool(ts.Bool);
                if (ts.Nested != null)
                {
                    WriteTestStruct(obj.Property("nested"), ts.Nested);
                }
            }
        }
    }
}
