using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

using static LaunchDarkly.JsonStream.Benchmarks.BenchmarkData;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    [MemoryDiagnoser]
    public class JsonNetStreamingComparativeBenchmark
    {
        [Benchmark]
        public void ReadBools()
        {
            var jr = new JsonTextReader(new StringReader(ListOfBoolsJson));
            var bools = ReadBools(jr);
        }

        [Benchmark]
        public void ReadInts()
        {
            var jr = new JsonTextReader(new StringReader(ListOfIntsJson));
            var ints = ReadInts(jr);
        }

        [Benchmark]
        public void ReadStruct()
        {
            var jr = new JsonTextReader(new StringReader(StructJson));
            var ts = ReadTestStruct(jr);
        }

        [Benchmark]
        public void WriteBools()
        {
            var sw = new StringWriter();
            var jw = new JsonTextWriter(sw);
            WriteBools(jw, ListOfBools);
            var s = sw.ToString();
        }

        [Benchmark]
        public void WriteInts()
        {
            var sw = new StringWriter();
            var jw = new JsonTextWriter(sw);
            WriteInts(jw, ListOfInts);
            var s = sw.ToString();
        }

        [Benchmark]
        public void WriteStruct()
        {
            var sw = new StringWriter();
            var jw = new JsonTextWriter(sw);
            WriteTestStruct(jw, Struct);
            var s = sw.ToString();
        }

        private List<bool> ReadBools(JsonReader jr)
        {
            var ret = new List<bool>();
            jr.Read();
            if (jr.TokenType != JsonToken.StartArray)
            {
                throw new Exception();
            }
            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonToken.EndArray)
                {
                    jr.Skip();
                    break;
                }
                ret.Add(jr.ReadAsBoolean().Value);
            }
            return ret;
        }

        private List<int> ReadInts(JsonReader jr)
        {
            var ret = new List<int>();
            jr.Read();
            if (jr.TokenType != JsonToken.StartArray)
            {
                throw new Exception();
            }
            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonToken.EndArray)
                {
                    jr.Skip();
                    break;
                }
                ret.Add(jr.ReadAsInt32().Value);
            }
            return ret;
        }

        private void WriteBools(JsonWriter jw, List<bool> bools)
        {
            jw.WriteStartArray();
            for (var i = 0; i < bools.Count; i++)
            {
                jw.WriteValue(bools[i]);
            }
            jw.WriteEndArray();
        }

        private void WriteInts(JsonWriter jw, List<int> ints)
        {
            jw.WriteStartArray();
            for (var i = 0; i < ints.Count; i++)
            {
                jw.WriteValue(ints[i]);
            }
            jw.WriteEndArray();
        }

        private TestStruct ReadTestStruct(JsonReader jr)
        {
            jr.Read();
            if (jr.TokenType == JsonToken.Null)
            {
                jr.Skip();
                return null;
            }
            if (jr.TokenType != JsonToken.StartObject)
            {
                throw new Exception();
            }

            var ret = new TestStruct();

            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonToken.EndObject)
                {
                    jr.Skip();
                    break;
                }
                if (jr.TokenType != JsonToken.PropertyName)
                {
                    throw new Exception();
                }
                switch (jr.Value.ToString())
                {
                    case "str":
                        ret.Str = jr.ReadAsString();
                        break;
                    case "nums":
                        ret.Nums = ReadInts(jr);
                        break;
                    case "bool":
                        ret.Bool = jr.ReadAsBoolean().Value;
                        break;
                    case "nested":
                        ret.Nested = ReadTestStruct(jr);
                        break;
                    default:
                        jr.Skip();
                        break;
                }
            }

            return ret;
        }

        private void WriteTestStruct(JsonWriter jw, TestStruct ts)
        {
            jw.WriteStartObject();

            jw.WritePropertyName("Str");
            jw.WriteValue(ts.Str);

            jw.WritePropertyName("Nums");
            WriteInts(jw, ts.Nums);

            jw.WritePropertyName("Bool");
            jw.WriteValue(ts.Bool);

            if (ts.Nested != null)
            {
                jw.WritePropertyName("Nested");
                WriteTestStruct(jw, ts.Nested);
            }

            jw.WriteEndObject();
        }
    }
}
