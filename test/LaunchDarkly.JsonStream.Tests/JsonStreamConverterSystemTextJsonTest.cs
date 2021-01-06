#if USE_SYSTEM_TEXT_JSON

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class JsonStreamConverterSystemTextJsonTest
    {
        public static readonly MyTestClass ExpectedInstance = new MyTestClass
        {
            // This object has a lot of properties because we want to make sure that all of the various
            // typed JReader methods are being passed to the appropriate delegate methods.
            StringProp = "hi",
            BoolProp = true,
            IntProp = 1,
            LongProp = 2,
            DoubleProp = 2.5,
            ArrayOfInts = new List<int> { 1, 2 },
            ObjectOfInts = new Dictionary<string, int> { { "a", 1 } },
            NullableString1 = null,
            NullableBool1 = null,
            NullableInt1 = null,
            NullableLong1 = null,
            NullableDouble1 = null,
            NullableString2 = "bye",
            NullableBool2 = true,
            NullableInt2 = 3,
            NullableLong2 = 4,
            NullableDouble2 = 4.5,
        };

        public const string ExpectedJson = @"{
            ""stringProp"": ""hi"",
            ""boolProp"": true,
            ""intProp"": 1,
            ""longProp"": 2,
            ""doubleProp"": 2.5,
            ""arrayOfInts"": [ 1, 2 ],
            ""objectOfInts"": { ""a"": 1 },
            ""nullableString1"": null,
            ""nullableBool1"": null,
            ""nullableInt1"": null,
            ""nullableLong1"": null,
            ""nullableDouble1"": null,
            ""nullableString2"": ""bye"",
            ""nullableBool2"": true,
            ""nullableInt2"": 3,
            ""nullableLong2"": 4,
            ""nullableDouble2"": 4.5
        }";

        [Fact]
        public void SerializeObject()
        {
            TestUtil.AssertJsonEqual(ExpectedJson, JsonSerializer.Serialize(ExpectedInstance));
        }

        [Fact]
        public void SerializeObjectToUTF8Bytes()
        {
            TestUtil.AssertJsonEqual(ExpectedJson,
                Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(ExpectedInstance)));
        }

        [Fact]
        public void DeserializeObject()
        {
            var instance = JsonSerializer.Deserialize<MyTestClass>(ExpectedJson);
            Assert.Equal(ExpectedInstance, instance);
        }

        [JsonStreamConverter(typeof(MyTestConverter))]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        public struct MyTestClass
#pragma warning restore CS0659
        {
            public string StringProp { get; set; }
            public bool BoolProp { get; set; }
            public int IntProp { get; set; }
            public long LongProp { get; set; }
            public double DoubleProp { get; set; }
            public List<int> ArrayOfInts { get; set; }
            public Dictionary<string, int> ObjectOfInts { get; set; }
            public string NullableString1 { get; set; }
            public bool? NullableBool1 { get; set; }
            public int? NullableInt1 { get; set; }
            public long? NullableLong1 { get; set; }
            public double? NullableDouble1 { get; set; }
            public string NullableString2 { get; set; }
            public bool? NullableBool2 { get; set; }
            public int? NullableInt2 { get; set; }
            public long? NullableLong2 { get; set; }
            public double? NullableDouble2 { get; set; }

            public override bool Equals(object other) =>
                other is MyTestClass o &&
                StringProp == o.StringProp &&
                BoolProp == o.BoolProp &&
                IntProp == o.IntProp &&
                LongProp == o.LongProp &&
                DoubleProp == o.DoubleProp &&
                ArrayOfInts.SequenceEqual(o.ArrayOfInts) &&
                ObjectOfInts.SequenceEqual(o.ObjectOfInts) &&
                NullableString1 == o.NullableString1 &&
                NullableBool1 == o.NullableBool1 &&
                NullableInt1 == o.NullableInt1 &&
                NullableLong1 == o.NullableLong1 &&
                NullableDouble1 == o.NullableDouble1 &&
                NullableString2 == o.NullableString2 &&
                NullableBool2 == o.NullableBool2 &&
                NullableInt2 == o.NullableInt2 &&
                NullableLong2 == o.NullableLong2 &&
                NullableDouble2 == o.NullableDouble2;
        }

        public class MyTestConverter : IJsonStreamConverter<MyTestClass>
        {
            public MyTestClass ReadJson(ref JReader reader)
            {
                var ret = new MyTestClass();
                for (var obj = reader.Object(); obj.Next(ref reader);)
                {
                    switch (obj.Name.ToString())
                    {
                        case "stringProp":
                            ret.StringProp = reader.String();
                            break;
                        case "boolProp":
                            ret.BoolProp = reader.Bool();
                            break;
                        case "intProp":
                            ret.IntProp = reader.Int();
                            break;
                        case "longProp":
                            ret.LongProp = reader.Long();
                            break;
                        case "doubleProp":
                            ret.DoubleProp = reader.Double();
                            break;
                        case "arrayOfInts":
                            ret.ArrayOfInts = new List<int>();
                            for (var arr = reader.Array(); arr.Next(ref reader);)
                            {
                                ret.ArrayOfInts.Add(reader.Int());
                            }
                            break;
                        case "objectOfInts":
                            ret.ObjectOfInts = new Dictionary<string, int>();
                            for (var subObj = reader.Object(); subObj.Next(ref reader);)
                            {
                                ret.ObjectOfInts[subObj.Name.ToString()] = reader.Int();
                            }
                            break;
                        case "nullableString1":
                            ret.NullableString1 = reader.StringOrNull();
                            break;
                        case "nullableBool1":
                            ret.NullableBool1 = reader.BoolOrNull();
                            break;
                        case "nullableInt1":
                            ret.NullableInt1 = reader.IntOrNull();
                            break;
                        case "nullableLong1":
                            ret.NullableLong1 = reader.LongOrNull();
                            break;
                        case "nullableDouble1":
                            ret.NullableDouble1 = reader.DoubleOrNull();
                            break;
                        case "nullableString2":
                            ret.NullableString2 = reader.StringOrNull();
                            break;
                        case "nullableBool2":
                            ret.NullableBool2 = reader.BoolOrNull();
                            break;
                        case "nullableInt2":
                            ret.NullableInt2 = reader.IntOrNull();
                            break;
                        case "nullableLong2":
                            ret.NullableLong2 = reader.LongOrNull();
                            break;
                        case "nullableDouble2":
                            ret.NullableDouble2 = reader.DoubleOrNull();
                            break;
                    }
                }
                return ret;
            }

            public void WriteJson(MyTestClass instance, IValueWriter writer)
            {
                var obj = writer.Object();
                obj.Property("stringProp").String(instance.StringProp);
                obj.Property("boolProp").Bool(instance.BoolProp);
                obj.Property("intProp").Int(instance.IntProp);
                obj.Property("longProp").Long(instance.LongProp);
                obj.Property("doubleProp").Double(instance.DoubleProp);
                if (instance.ArrayOfInts != null)
                {
                    using (var arr = obj.Property("arrayOfInts").Array())
                    {
                        foreach (var n in instance.ArrayOfInts)
                        {
                            arr.Int(n);
                        }
                    }
                }
                if (instance.ObjectOfInts != null)
                {
                    using (var subObj = obj.Property("objectOfInts").Object())
                    {
                        foreach (var kv in instance.ObjectOfInts)
                        {
                            subObj.Property(kv.Key).Int(kv.Value);
                        }
                    }
                }
                obj.Property("nullableString1").String(instance.NullableString1);
                obj.Property("nullableBool1").BoolOrNull(instance.NullableBool1);
                obj.Property("nullableInt1").IntOrNull(instance.NullableInt1);
                obj.Property("nullableLong1").LongOrNull(instance.NullableLong1);
                obj.Property("nullableDouble1").DoubleOrNull(instance.NullableDouble1);
                obj.Property("nullableString2").String(instance.NullableString2);
                obj.Property("nullableBool2").BoolOrNull(instance.NullableBool2);
                obj.Property("nullableInt2").IntOrNull(instance.NullableInt2);
                obj.Property("nullableLong2").LongOrNull(instance.NullableLong2);
                obj.Property("nullableDouble2").DoubleOrNull(instance.NullableDouble2);
                obj.End();
            }
        }
    }
}

#endif
