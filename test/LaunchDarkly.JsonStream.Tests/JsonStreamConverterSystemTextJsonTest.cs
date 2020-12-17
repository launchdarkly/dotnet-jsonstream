#if USE_SYSTEM_TEXT_JSON

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

using static LaunchDarkly.JsonStream.JsonStreamConvertTest;

namespace LaunchDarkly.JsonStream
{
    public class JsonStreamConverterSystemTextJsonTest
    {
        [Fact]
        public void SerializeObject()
        {
            Assert.Equal(ExpectedJson, JsonSerializer.Serialize(ExpectedInstance));
        }

        [Fact]
        public void SerializeObjectToUTF8Bytes()
        {
            Assert.Equal(Encoding.UTF8.GetBytes(ExpectedJson),
                JsonSerializer.SerializeToUtf8Bytes(ExpectedInstance));
        }

        [Fact]
        public void DeserializeObject()
        {
            var instance = JsonSerializer.Deserialize<MyTestClass>(ExpectedJson);
            Assert.Equal(ExpectedInstance.Value, instance.Value);
        }

    }
}

#endif
