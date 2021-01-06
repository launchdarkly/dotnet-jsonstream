using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class JsonStreamConvertTest
    {
        public static readonly MyTestClass ExpectedInstance = new MyTestClass { Value = "hi" };
        public const string ExpectedJson = @"{""value"":""hi""}";

        [Fact]
        public void SerializeObject()
        {
            Assert.Equal(ExpectedJson, JsonStreamConvert.SerializeObject(ExpectedInstance));
        }

        [Fact]
        public void SerializeObjectToUtf8Bytes()
        {
            Assert.Equal(Encoding.UTF8.GetBytes(ExpectedJson),
                JsonStreamConvert.SerializeObjectToUtf8Bytes(ExpectedInstance));
        }

        [Fact]
        public void DeserializeObject()
        {
            var instance = JsonStreamConvert.DeserializeObject<MyTestClass>(ExpectedJson);
            Assert.Equal(ExpectedInstance.Value, instance.Value);
        }

        [Fact]
        public void SerializeObjectFailsForClassWithInvalidConverter()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JsonStreamConvert.SerializeObject(new TestClassWithInvalidConverter()));
        }

        [Fact]
        public void DeserializeObjectFailsForClassWithInvalidConverter()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JsonStreamConvert.DeserializeObject<TestClassWithInvalidConverter>("{}"));
        }

        [Fact]
        public void SerializeObjectFailsForClassWithNoConverter()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JsonStreamConvert.SerializeObject(new TestClassWithNoConverter()));
        }

        [Fact]
        public void DeserializeObjectFailsForClassWithNoConverter()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JsonStreamConvert.DeserializeObject<TestClassWithNoConverter>("{}"));
        }

        [Fact]
        public void ConvertFromSimpleTypes()
        {
            var value = new List<object>
            {
                null,
                true,
                100,
                "x",
                new Dictionary<string, object> { { "a", 1 } }
            };
            var expected = @"[null, true, 100, ""x"", {""a"": 1}]";
            var actual = JsonStreamConvert.SerializeObject(value, JsonStreamConvert.ConvertSimpleTypes);
            TestUtil.AssertJsonEqual(expected, actual);
        }

        [Fact]
        public void ConvertToSimpleTypes()
        {
            var json = @"[null, true, 100, ""x"", {""a"": 1}]";
            var value = JsonStreamConvert.DeserializeObject(json, JsonStreamConvert.ConvertSimpleTypes);
            var list = Assert.IsType<List<object>>(value);
            Assert.Collection(list,
                e => Assert.Null(e),
                e => Assert.Equal(true, e),
                e => Assert.Equal((double)100, e),
                e => Assert.Equal("x", e),
                e =>
                {
                    var dict = Assert.IsType<Dictionary<string, object>>(e);
                    Assert.Equal((double)1, dict["a"]);
                }
                );
        }

        [JsonStreamConverter(typeof(MyTestConverter))]
        public class MyTestClass
        {
            public string Value { get; set; }
        }

        public class MyTestConverter : IJsonStreamConverter<MyTestClass>
        {
            public MyTestClass ReadJson(ref JReader reader)
            {
                string value = null;
                for (var obj = reader.Object().WithRequiredProperties(new string[] { "value" }); obj.Next(ref reader);)
                {
                    if (obj.Name == "value")
                    {
                        value = reader.String();
                    }
                }
                return new MyTestClass { Value = value };
            }

            public void WriteJson(MyTestClass instance, IValueWriter writer)
            {
                var obj = writer.Object();
                obj.Name("value").String(instance.Value);
                obj.End();
            }
        }

        public class TestClassWithNoConverter {}

        [JsonStreamConverter(typeof(NotReallyAConverter))]
        public class TestClassWithInvalidConverter {}

        public class NotReallyAConverter {}
    }
}
