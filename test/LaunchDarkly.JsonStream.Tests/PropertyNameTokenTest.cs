using System;
using System.Text;
#if USE_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif
using LaunchDarkly.JsonStream.Implementation;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class PropertyNameTokenTest
    {
        [Fact]
        public void Empty()
        {
            var p = new PropertyNameToken();
            Assert.True(p.Empty);
            Assert.False(p.Equals("x"));
            Assert.True(p.Equals(new PropertyNameToken()));
        }

        private void DoEqualityTests(PropertyNameToken p)
        {
            Assert.False(p.Empty);
            Assert.True(p.Equals("abc"));
            Assert.True(p == "abc");
            Assert.False(p != "abc");
            Assert.True(p.Equals(new PropertyNameToken("abc")));
            Assert.False(p.Equals(new PropertyNameToken("abd")));
#if USE_SYSTEM_TEXT_JSON
            Assert.True(p.Equals(new PropertyNameToken(MakeUtf8ByteSpan("xxabczz", 2, 3))));
            Assert.False(p.Equals(new PropertyNameToken(MakeUtf8ByteSpan("xxabdzz", 2, 3))));
            Assert.True(p.Equals(new PropertyNameToken(MakeJsonProperty("abc"))));
            Assert.False(p.Equals(new PropertyNameToken(MakeJsonProperty("abd"))));
#else

#endif
        }

        [Fact]
        public void FromString()
        {
            DoEqualityTests(new PropertyNameToken("abc"));
        }

#if USE_SYSTEM_TEXT_JSON
        private static ReadOnlySpan<byte> MakeUtf8ByteSpan(string s, int offset, int length) =>
            new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(s), offset, length);

        private static int nextPropertyValue = 0;

        private static JsonProperty MakeJsonProperty(string name)
        {
            var s = "{\"" + name + "\":" + (nextPropertyValue++) + "}";
            var doc = JsonDocument.Parse(s);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                return prop;
            }
            throw new InvalidOperationException();
        }

        [Fact]
        public void FromAsciiBytes()
        {
            DoEqualityTests(new PropertyNameToken(MakeUtf8ByteSpan("xabcz", 1, 3)));
        }

        [Fact]
        public void FromJsonProperty()
        {
            DoEqualityTests(new PropertyNameToken(MakeJsonProperty("abc")));
        }
#else
        [Fact]
        public void FromStringToken()
        {
            DoEqualityTests(new PropertyNameToken(StringToken.FromChars("xabcz".ToCharArray(), 1, 3)));
        }
#endif
    }
}
