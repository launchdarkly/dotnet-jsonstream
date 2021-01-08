
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Used in conjunction with <see cref="JReader"/> to iterate through a JSON array.
    /// </summary>
    /// <remarks>
    /// The <see cref="JReader.Array"/> and <see cref="JReader.ArrayOrNull"/> methods return an
    /// instance of this type. Call the <see cref="Next(ref JReader)"/> method to iterate through each
    /// array element. To read the actual values, you will still use the <see cref="JReader"/>'s methods.
    /// </remarks>
    /// <example>
    /// <code>
    ///     var r = new JReader("[10, 20, 30]");
    ///     for (var arr = r.Array(); arr.Next(ref r);)
    ///     {
    ///         value = r.Int();
    ///         System.Console.WriteLine(value);
    ///     }
    /// </code>
    /// </example>
    public ref struct ArrayReader
    {
        private readonly bool _defined;
        private bool _afterFirst;

        internal ArrayReader(bool defined)
        {
            _defined = defined;
            _afterFirst = false;
        }

        /// <summary>
        /// True if the <c>ArrayReader</c> represents an actual array, or false if it was parsed
        /// from a null value or was the result of an error.
        /// </summary>
        /// <remarks>
        /// If <c>IsDefined</c> is <see langword="false"/>, <c></c>Next will always return <see langword="false"/>.
        /// </remarks>
        public bool IsDefined => _defined;

        /// <summary>
        /// Advances to the next array element if any, and returns <see langword="true"/> if successful.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It returns <see langword="false"/> if the <c>JReader</c> has reached the end of the array, or
        /// if the array was empty or null.
        /// </para>
        /// <para>
        /// If <c>Next</c> returns <see langword="true"/>, you can then use <see cref="JReader"/> methods
        /// such as <see cref="JReader.Bool"/> to read the element value. If you do not care about the
        /// value, simply calling <c>Next</c> again without calling a <c>JReader</c> method will discard
        /// the value.
        /// </para>
        /// <para>
        /// For more information about why <c>Next</c> requires a <c>ref</c> parameter, see
        /// <see cref="JReader"/>.
        /// </para>
        /// </remarks>
        /// <param name="reader">the original <see cref="JReader"/> (must be passed by reference)</param>
        /// <returns><see langword="true"/> if there is a next array element</returns>
        public bool Next(ref JReader reader)
        {
            if (!_defined)
            {
                return false;
            }
            var hasNext = reader.ArrayNext(!_afterFirst);
            _afterFirst = true;
            return hasNext;
        }
    }
}
