
namespace LaunchDarkly.JsonStream.Implementation
{
    /// <summary>
    /// Allows programmatic inspection of the library implementation.
    /// </summary>
    public static class Properties
    {
        /// <summary>
        /// True if this is a version of this library that uses <c>System.Text.Json</c>
        /// as its underlying implementation.
        /// </summary>
        /// <remarks>
        /// This should be the case if your target framework is .NET Core 3.x or .NET 5.x.
        /// The <c>System.Text.Json</c> implementation is faster than the default. The
        /// library is <i>not</i> able to use <c>System.Text.Json</c> on other platforms
        /// even if it is installed separately as a NuGet package; it has to select the
        /// implementation when the library is built.
        /// </remarks>
        public static bool IsPlatformNativeImplementation =>
            TokenReader.IsPlatformNativeImplementation;
    }
}
