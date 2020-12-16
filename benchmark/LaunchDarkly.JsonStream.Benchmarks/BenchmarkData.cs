using System.Collections.Generic;
using Newtonsoft.Json;

namespace LaunchDarkly.JsonStream.Benchmarks
{
    public static class BenchmarkData
    {
        public static List<bool> ListOfBools = MakeListOfBools();
        public static List<int> ListOfInts = MakeListOfInts();
        public static TestStruct Struct = new TestStruct
        {
            Str = "abc",
            Nums = MakeListOfInts(),
            Bool = true,
            Nested = new TestStruct
            {
                Str = "def",
                Nums = MakeListOfInts(),
                Bool = false
            }
        };

        public static string ListOfBoolsJson = JsonConvert.SerializeObject(ListOfBools);
        public static string ListOfIntsJson = JsonConvert.SerializeObject(ListOfInts);
        public static string StructJson = JsonConvert.SerializeObject(Struct);

        private static List<bool> MakeListOfBools()
        {
            var ret = new List<bool>(100);
            for (int i = 0; i < 100; i++)
            {
                ret.Add(i % 2 == 1);
            }
            return ret;
        }

        private static List<int> MakeListOfInts()
        {
            var ret = new List<int>(100);
            for (int i = 0; i < 100; i++)
            {
                ret.Add(i * 10);
            }
            return ret;
        }
    }

    public class TestStruct
    {
        [JsonProperty(PropertyName = "str")]
        public string Str;

        [JsonProperty(PropertyName = "nums")]
        public List<int> Nums;

        [JsonProperty(PropertyName = "bool")]
        public bool Bool;

        [JsonProperty(PropertyName = "nested")]
        public TestStruct Nested;
    }
}
