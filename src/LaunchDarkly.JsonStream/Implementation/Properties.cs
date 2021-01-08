
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
        /// This should be the case for all target frameworks except .NET Framework 4.5.x,
        /// where <c>System.Text.Json</c> is not available; in that case, <c>LaunchDarkly.JsonStream</c>
        /// uses a less efficient implementation of its own.
        /// </remarks>
        public static bool IsSystemTextJsonImplementation =>
            TokenReader.IsSystemTextJsonImplementation;
    }
}
