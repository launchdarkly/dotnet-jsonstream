using System;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ImplementationTest
    {
        [Fact]
        public void CheckExpectedImplementation()
        {
            // The environment variable SHOULD_USE_SYSTEM_TEXT_JSON is set by our CI tests
            // when we are running in a target framework that should be using System.Text.Json.
            var value = Environment.GetEnvironmentVariable("SHOULD_USE_SYSTEM_TEXT_JSON");
            if (value != null)
            {
                if (value == "true")
                {
                    Assert.True(Implementation.Properties.IsPlatformNativeImplementation);
                }
                else if (value == "false")
                {
                    Assert.False(Implementation.Properties.IsPlatformNativeImplementation);
                }
                else
                {
                    Assert.True(false, "test variable SHOULD_USE_SYSTEM_TEXT_JSON must be set to \"true\" or \"false\"");
                }
            }
        }
    }
}
