using System;
using Newtonsoft.Json.Linq;

namespace LaunchDarkly.JsonStream
{
    public static class TestUtil
    {
        public static void AssertJsonEqual(string expected, string actual)
        {
            // Newtonsoft.Json is useful here because it can do a deep-equality comparison of parsed data
            var parsedExpected = JToken.Parse(expected);
            var parsedActual = JToken.Parse(actual);
            if (!JToken.DeepEquals(parsedActual, parsedExpected))
            {
                throw new Exception(string.Format("did not get expected JSON\nexpected: {0}\nactual: {1}",
                    expected, actual));
            }
        }
    }
}
