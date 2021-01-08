using System.Collections.Generic;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ReaderAdaptersTest
    {
        [Fact]
        public void ReadFromSimpleTypes()
        {
            var value = new List<object>
            {
                null,
                true,
                1,
                (long)2,
                3,
                (long)4,
                (float)5.5,
                (double)6.5,
                "x",
                new Dictionary<string, object> { { "a", 1 } },
                new Dictionary<object, object> { { "b", 2 } }
            };
            var r = JReader.FromAdapter(ReaderAdapters.FromSimpleTypes(value));

            var arr = r.Array();

            Assert.True(arr.Next(ref r));
            r.Null();

            Assert.True(arr.Next(ref r));
            Assert.True(r.Bool());

            Assert.True(arr.Next(ref r));
            Assert.Equal(1, r.Int());

            Assert.True(arr.Next(ref r));
            Assert.Equal(2, r.Int());

            Assert.True(arr.Next(ref r));
            Assert.Equal(3, r.Long());

            Assert.True(arr.Next(ref r));
            Assert.Equal(4, r.Long());

            Assert.True(arr.Next(ref r));
            Assert.Equal(5.5, r.Double());

            Assert.True(arr.Next(ref r));
            Assert.Equal(6.5, r.Double());

            Assert.True(arr.Next(ref r));
            Assert.Equal("x", r.String());

            Assert.True(arr.Next(ref r));
            var obj1 = r.Object();
            Assert.True(obj1.Next(ref r));
            Assert.Equal("a", obj1.Name.ToString());
            Assert.Equal(1, r.Int());
            Assert.False(obj1.Next(ref r));

            Assert.True(arr.Next(ref r));
            var obj2 = r.Object();
            Assert.True(obj2.Next(ref r));
            Assert.Equal("b", obj2.Name.ToString());
            Assert.Equal(2, r.Int());
            Assert.False(obj2.Next(ref r));

            Assert.False(arr.Next(ref r));
            Assert.True(r.EOF);
        }

        [Fact]
        public void ReadFromSimpleTypesWithStrictTypeChecking()
        {
            Assert.True(ParseBool(ReaderAdapters.FromSimpleTypes(true)));
            Assert.False(ParseBool(ReaderAdapters.FromSimpleTypes(false)));
            Assert.Throws<TypeException>(() => ParseBool(ReaderAdapters.FromSimpleTypes("true")));
            Assert.Throws<TypeException>(() => ParseBool(ReaderAdapters.FromSimpleTypes("true")));

            Assert.Equal(100, ParseDouble(ReaderAdapters.FromSimpleTypes(100)));
            Assert.Throws<TypeException>(() => ParseDouble(ReaderAdapters.FromSimpleTypes("100")));

            Assert.Equal("true", ParseString(ReaderAdapters.FromSimpleTypes("true")));
            Assert.Equal("100", ParseString(ReaderAdapters.FromSimpleTypes("100")));
            Assert.Throws<TypeException>(() => ParseString(ReaderAdapters.FromSimpleTypes(true)));
            Assert.Throws<TypeException>(() => ParseString(ReaderAdapters.FromSimpleTypes(100)));
        }

        [Fact]
        public void ReadFromSimpleTypesWithTypeCoercion()
        {
            Assert.True(ParseBool(ReaderAdapters.FromSimpleTypes(true, allowTypeCoercion: true)));
            Assert.False(ParseBool(ReaderAdapters.FromSimpleTypes(false, allowTypeCoercion: true)));
            Assert.True(ParseBool(ReaderAdapters.FromSimpleTypes("true", allowTypeCoercion: true)));
            Assert.False(ParseBool(ReaderAdapters.FromSimpleTypes("false", allowTypeCoercion: true)));
            Assert.True(ParseBool(ReaderAdapters.FromSimpleTypes("on", allowTypeCoercion: true)));
            Assert.False(ParseBool(ReaderAdapters.FromSimpleTypes("off", allowTypeCoercion: true)));
            Assert.Throws<TypeException>(() => ParseBool(ReaderAdapters.FromSimpleTypes("x", allowTypeCoercion: true)));

            Assert.Equal(100, ParseDouble(ReaderAdapters.FromSimpleTypes(100, allowTypeCoercion: true)));
            Assert.Equal(100, ParseDouble(ReaderAdapters.FromSimpleTypes("100", allowTypeCoercion: true)));
            Assert.Throws<TypeException>(() => ParseDouble(ReaderAdapters.FromSimpleTypes("x", allowTypeCoercion: true)));

            Assert.Equal("true", ParseString(ReaderAdapters.FromSimpleTypes("true", allowTypeCoercion: true)));
            Assert.Equal("100", ParseString(ReaderAdapters.FromSimpleTypes("100", allowTypeCoercion: true)));
            Assert.Equal("true", ParseString(ReaderAdapters.FromSimpleTypes(true, allowTypeCoercion: true)));
            Assert.Equal("100", ParseString(ReaderAdapters.FromSimpleTypes(100, allowTypeCoercion: true)));
        }

        private bool ParseBool(IReaderAdapter adapter)
        {
            var r = JReader.FromAdapter(adapter);
            return r.Bool();
        }

        private double ParseDouble(IReaderAdapter adapter)
        {
            var r = JReader.FromAdapter(adapter);
            return r.Double();
        }

        private string ParseString(IReaderAdapter adapter)
        {
            var r = JReader.FromAdapter(adapter);
            return r.String();
        }

        [Fact]
        public void ReadSimpleTypesFromYamlParser()
        {
            // The .NET SDK allows flag data to be read from a YAML file by first running it through a YAML
            // parser, which produces types like Dictionary, and then feeding that into JReader. So we'll
            // verify that a basic use case like that works, using YamlDotNet.
            var yamlContent = @"
---
flags:
  flag1:
    key: flag1
    ""on"": true
    version: 1
    fallthrough:
      variation: 2
    variations:
      - fall
      - ""off""
      - ""on""
flagValues:
  flag2: value2
segments:
  seg1:
    key: seg1
    version: 1
    include: [""user1""]
";
            var yaml = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            var dataIn = yaml.Deserialize<object>(yamlContent);

            var r = JReader.FromAdapter(ReaderAdapters.FromSimpleTypes(dataIn));

            var dataOut = JsonStreamConvert.ConvertSimpleTypes.ReadJson(ref r);

            var jsonOut = JsonStreamConvert.SerializeObject(dataOut, JsonStreamConvert.ConvertSimpleTypes);
            var expectedJson = @"{
""flags"": {
  ""flag1"": {
    ""key"": ""flag1"",
    ""on"": ""true"",
    ""version"": ""1"",
    ""fallthrough"": { ""variation"": ""2"" },
    ""variations"": [ ""fall"", ""off"", ""on"" ]
  }
},
""flagValues"": { ""flag2"": ""value2"" },
""segments"": {
  ""seg1"": { ""key"": ""seg1"", ""version"": ""1"", ""include"": [""user1""] }
}
}";
            // Note that YamlDotNet parses all the booleans and numbers as strings; that's why we provide a
            // type coercion option.
            TestUtil.AssertJsonEqual(expectedJson, jsonOut);
        }
    }
}
