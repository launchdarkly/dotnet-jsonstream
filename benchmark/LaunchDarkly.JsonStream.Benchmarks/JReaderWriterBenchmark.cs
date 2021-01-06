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
            var values = ReadBools(ref r);
        }

        [Benchmark]
        public void ReadInts()
        {
            var r = JReader.FromString(ListOfIntsJson);
            var values = ReadInts(ref r);
        }

        [Benchmark]
        public void ReadStructs()
        {
            var r = JReader.FromString(ListOfStructsJson);
            var values = ReadStructs(ref r);
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
        public void WriteStructs()
        {
            var w = JWriter.New();
            WriteStructs(w, ListOfStructs);
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

        private List<TestStruct> ReadStructs(ref JReader r)
        {
            var ret = new List<TestStruct>();
            for (var arr = r.Array(); arr.Next(ref r);)
            {
                ret.Add(ReadStruct(ref r));
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
                switch (obj.Name)
                {
                    case var n when n == "str":
                        ret.Str = r.String();
                        break;
                    case var n when n == "nums":
                        ret.Nums = ReadInts(ref r);
                        break;
                    case var n when n == "bool":
                        ret.Bool = r.Bool();
                        break;
                    case var n when n == "nested":
                        ret.Nested = ReadStruct(ref r);
                        break;
                }
            }
            return ret;
        }

        private void WriteBools(IValueWriter w, List<bool> bools)
        {
            var arr = w.Array();
            for (var i = 0; i < bools.Count; i++)
            {
                w.Bool(bools[i]);
            }
            arr.End();
        }

        private void WriteInts(IValueWriter w, List<int> ints)
        {
            var arr = w.Array();
            for (var i = 0; i < ints.Count; i++)
            {
                w.Int(ints[i]);
            }
            arr.End();
        }

        private void WriteStructs(IValueWriter w, List<TestStruct> structs)
        {
            var arr = w.Array();
            for (var i = 0; i < structs.Count; i++)
            {
                WriteTestStruct(w, structs[i]);
            }
            arr.End();
        }

        private void WriteTestStruct(IValueWriter w, TestStruct ts)
        {
            var obj = w.Object();
            obj.Property("str").String(ts.Str);
            WriteInts(obj.Property("nums"), ts.Nums);
            obj.Property("bool").Bool(ts.Bool);
            if (ts.Nested != null)
            {
                WriteTestStruct(obj.Property("nested"), ts.Nested);
            }
            obj.End();
        }
    }
}
