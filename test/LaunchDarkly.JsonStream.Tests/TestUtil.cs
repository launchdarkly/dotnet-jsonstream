using Newtonsoft.Json.Linq;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public static class TestUtil
    {
        public static void AssertJsonEqual(string expected, string actual)
        {
            // Newtonsoft.Json is useful here because it can do a deep-equality comparison of parsed data
            var parsedExpected = JToken.Parse(expected);
            var parsedActual = JToken.Parse(actual);
            Assert.Equal(parsedExpected, parsedActual);
        }
    }
}
