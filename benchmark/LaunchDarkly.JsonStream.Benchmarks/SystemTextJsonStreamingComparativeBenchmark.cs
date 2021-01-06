#if !NET452

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using BenchmarkDotNet.Attributes;

using static LaunchDarkly.JsonStream.Benchmarks.BenchmarkData;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    [MemoryDiagnoser]
    public class SystemTextJsonStreamingComparativeBenchmark
    {
        [Benchmark]
        public void ReadBools()
        {
            var jr = new Utf8JsonReader(Encoding.UTF8.GetBytes(ListOfBoolsJson));
            var bools = ReadBools(ref jr);
        }

        [Benchmark]
        public void ReadInts()
        {
            var jr = new Utf8JsonReader(Encoding.UTF8.GetBytes(ListOfIntsJson));
            var ints = ReadInts(ref jr);
        }

        [Benchmark]
        public void ReadStructs()
        {
            var jr = new Utf8JsonReader(Encoding.UTF8.GetBytes(ListOfStructsJson));
            var ts = ReadStructs(ref jr);
        }

        [Benchmark]
        public void WriteBools()
        {
            var buf = new MemoryStream();
            var jw = new Utf8JsonWriter(buf);
            WriteBools(jw, ListOfBools);
            var s = new StreamReader(buf).ReadToEnd();
        }

        [Benchmark]
        public void WriteInts()
        {
            var buf = new MemoryStream();
            var jw = new Utf8JsonWriter(buf);
            WriteInts(jw, ListOfInts);
            var s = new StreamReader(buf).ReadToEnd();
        }

        [Benchmark]
        public void WriteStructs()
        {
            var buf = new MemoryStream();
            var jw = new Utf8JsonWriter(buf);
            WriteStructs(jw, ListOfStructs);
            var s = new StreamReader(buf).ReadToEnd();
        }

        private List<bool> ReadBools(ref Utf8JsonReader jr)
        {
            var ret = new List<bool>();
            jr.Read();
            if (jr.TokenType != JsonTokenType.StartArray)
            {
                throw new Exception();
            }
            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonTokenType.EndArray)
                {
                    jr.Skip();
                    break;
                }
                ret.Add(jr.GetBoolean());
            }
            return ret;
        }

        private List<int> ReadInts(ref Utf8JsonReader jr)
        {
            var ret = new List<int>();
            jr.Read();
            if (jr.TokenType != JsonTokenType.StartArray)
            {
                throw new Exception();
            }
            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonTokenType.EndArray)
                {
                    jr.Skip();
                    break;
                }
                ret.Add((int)jr.GetDouble());
            }
            return ret;
        }

        private List<TestStruct> ReadStructs(ref Utf8JsonReader jr)
        {
            var ret = new List<TestStruct>();
            jr.Read();
            if (jr.TokenType != JsonTokenType.StartArray)
            {
                throw new Exception();
            }
            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonTokenType.EndArray)
                {
                    jr.Skip();
                    break;
                }
                ret.Add(ReadTestStruct(ref jr));
            }
            return ret;
        }

        private void WriteBools(Utf8JsonWriter jw, List<bool> bools)
        {
            jw.WriteStartArray();
            for (var i = 0; i < bools.Count; i++)
            {
                jw.WriteBooleanValue(bools[i]);
            }
            jw.WriteEndArray();
        }

        private void WriteInts(Utf8JsonWriter jw, List<int> ints)
        {
            jw.WriteStartArray();
            for (var i = 0; i < ints.Count; i++)
            {
                jw.WriteNumberValue(ints[i]);
            }
            jw.WriteEndArray();
        }

        private void WriteStructs(Utf8JsonWriter jw, List<TestStruct> structs)
        {
            jw.WriteStartArray();
            for (var i = 0; i < structs.Count; i++)
            {
                WriteTestStruct(jw, structs[i]);
            }
            jw.WriteEndArray();
        }

        private TestStruct ReadTestStruct(ref Utf8JsonReader jr)
        {
            if (jr.TokenType == JsonTokenType.Null)
            {
                jr.Skip();
                return null;
            }
            if (jr.TokenType != JsonTokenType.StartObject)
            {
                throw new Exception("unexpected token: " + jr.TokenType);
            }

            var ret = new TestStruct();

            while (true)
            {
                jr.Read();
                if (jr.TokenType == JsonTokenType.EndObject)
                {
                    jr.Skip();
                    break;
                }
                if (jr.TokenType != JsonTokenType.PropertyName)
                {
                    throw new Exception("unexpected token: " + jr.TokenType);
                }
                switch (jr.GetString())
                {
                    case "str":
                        ret.Str = jr.GetString();
                        break;
                    case "nums":
                        ret.Nums = ReadInts(ref jr);
                        break;
                    case "bool":
                        ret.Bool = jr.GetBoolean();
                        break;
                    case "nested":
                        jr.Read();
                        ret.Nested = ReadTestStruct(ref jr);
                        break;
                    default:
                        jr.Skip();
                        break;
                }
            }

            return ret;
        }

        private void WriteTestStruct(Utf8JsonWriter jw, TestStruct ts)
        {
            jw.WriteStartObject();

            jw.WriteString("str", ts.Str);

            jw.WritePropertyName("nums");
            WriteInts(jw, ts.Nums);

            jw.WriteBoolean("bool", ts.Bool);

            if (ts.Nested != null)
            {
                jw.WritePropertyName("nested");
                WriteTestStruct(jw, ts.Nested);
            }

            jw.WriteEndObject();
        }
    }
}

#endif
