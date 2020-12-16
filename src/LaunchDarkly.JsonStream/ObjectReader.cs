
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Used in conjunction with <see cref="JReader"/> to iterate through a JSON object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="JReader.Object"/> and <see cref="JReader.ObjectOrNull"/> methods return an
    /// instance of this type. Call the <see cref="Next"/> method to iterate through each object
    /// property. Properties may appear in any order. To read the actual values, you will still
    /// use the <see cref="JReader"/>'s methods.
    /// </para>
    /// <para>
    /// This example reads an object whose values are strings; if there is a null instead of an object,
    /// it behaves the same as for an empty object.
    /// </para>
    /// <code>
    ///     var values = new Dictionary&lt;string, string&gt;()
    ///     for (var obj = r.ObjectOrNull(); obj.Next();)
    ///     {
    ///         values[obj.Name.ToString()] = r.String()
    ///     }
    /// </code>
    /// <para>
    /// The next example reads an object with two expected property names, "a" and "b". Any =
    /// unrecognized properties are ignored.
    /// </para>
    /// <code>
    ///     int aValue, bValue;
    ///     for (var obj = r.ObjectOrNull(); obj.Next();)
    ///     {
    ///         if (obj.Name == "a")
    ///         {
    ///             a = r.Int();
    ///         }
    ///         else if (obj.Name == "b")
    ///         {
    ///             b = r.Int();
    ///         }
    ///     }
    /// </code>
    /// <para>
    /// Note that it is not possible to do <c>switch (obj.Name)</c> because the type of
    /// <see cref="Name"/> is <see cref="StringToken"/>, which, like <c>ReadOnlySpan&lt;char&gt;</c>,
    /// cannot be compared to string constants in a <c>switch</c> (for further discussion, see
    /// <a href="https://github.com/dotnet/csharplang/issues/1881">this issue</a>). This is a
    /// performance optimization to avoid allocating strings; a series of simple <c>if</c>
    /// comparisons will often be more efficient for this purpose. However, you can also use
    /// <c>switch (obj.Name.ToString())</c> if you do not mind allocating a string.
    /// </para>
    /// </remarks>
    public ref struct ObjectReader
    {
        private readonly bool _defined;
        private readonly string[] _requiredProperties;
        private readonly bool[] _foundProperties;
        private bool _afterFirst;
        private StringToken _name;

        internal ObjectReader(bool defined, string[] requiredProperties)
        {
            _defined = defined;
            _requiredProperties = requiredProperties;
            _foundProperties = !defined || requiredProperties is null ?
                null : new bool[requiredProperties.Length];
            _afterFirst = false;
            _name = StringToken.Empty;
        }

        /// <summary>
        /// True if the <c>ObjectReader</c> represents an actual object, or <see langword="false"/> if
        /// it was parsed from a null value or was the result of an error.
        /// </summary>
        /// <remarks>
        /// If <c>IsDefined</c> is <see langword="false"/>, <c></c>Next will always return <see langword="false"/>.
        /// </remarks>
        public bool IsDefined => _defined;

        /// <summary>
        /// The name of the current object property.
        /// </summary>
        /// <remarks>
        /// This value is initialized by calling <see cref="Next"/>. It returns the name as a
        /// <see cref="StringToken"/> rather than a <c>string</c> for efficiency, since this can often
        /// avoid the overhead of allocating strings that applications normally will not need to retain.
        /// If there is no current property (that is, if <c>Next</c> returned false or was never called)
        /// then it returns <see cref="StringToken.Empty"/>.
        /// </remarks>
        public StringToken Name => _name;

        /// <summary>
        /// Adds a requirement that the specified JSON property name(s) must appear in the JSON object at
        /// some point before it ends.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method returns a new, modified <see cref="ObjectReader"/>. It should be called before
        /// the first time you call <see cref="Next"/>. For instance:
        /// </para>
        /// <code>
        ///     var requiredProps = new string[] { "key", "name" };
        ///     for (var obj = reader.Object().WithRequiredProperties(requiredProps); obj.Next();)
        ///     {
        //         switch (obj.Name) { ... }
        //      }
        /// </code>
        /// <para>
        /// When the end of the object is reached, if one of the required properties has not yet been
        /// seen, <see cref="Next"/> will throw a <see cref="RequiredPropertyException"/>.
        /// </para>
        /// <para>
        /// For efficiency, it is best to preallocate the list of property names globally rather than
        /// creating it inline.
        /// </para>
        /// </remarks>
        /// <param name="propertyNames">the required property names</param>
        /// <returns>an updated <see cref="ObjectReader"/></returns>
        public ObjectReader WithRequiredProperties(params string[] propertyNames) =>
            new ObjectReader(_defined, propertyNames);
        
        /// <summary>
        /// Advances to the next object property if any, and returns <see langword="true"/> if successful.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It returns <see langword="false"/> if the <c>JReader</c> has reached the end of the object,
        /// or if the object was empty or null.
        /// </para>
        /// <para>
        /// If <c>Next</c> returns <see langword="true"/>, you can then use <see cref="JReader"/> methods
        /// such as <see cref="JReader.Bool"/> to read the element value. If you do not care about the
        /// value, simply calling <c>Next</c> again without calling a <c>JReader</c> method will discard
        /// the value.
        /// </para>
        /// </remarks>
        /// <returns><see langword="true"/> if there is a next object property</returns>
        public bool Next(ref JReader reader)
        {
            if (!_defined)
            {
                return false;
            }
            if (_afterFirst && reader._awaitingReadValue)
            {
                reader.SkipValue();
            }
            var nextName = reader._tr.ObjectNext(!_afterFirst);
            _afterFirst = true;
            if (nextName.HasValue)
            {
                _name = nextName.Value;
                reader._awaitingReadValue = true;
                if (_requiredProperties != null)
                {
                    for (int i = 0; i < _requiredProperties.Length; i++)
                    {
                        if (_name == _requiredProperties[i])
                        {
                            _foundProperties[i] = true;
                            break;
                        }
                    }
                }
                return true;
            }
            _name = StringToken.Empty;
            if (_requiredProperties != null)
            {
                for (int i = 0; i < _requiredProperties.Length; i++)
                {
                    if (!_foundProperties[i])
                    {
                        throw new RequiredPropertyException(_requiredProperties[i],
                            reader._tr.LastPos);
                    }
                }
            }
            return false;
        }
    }
}
