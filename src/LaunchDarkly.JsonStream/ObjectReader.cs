using LaunchDarkly.JsonStream.Implementation;

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
    ///     for (var obj = reader.ObjectOrNull(); obj.Next(ref reader);)
    ///     {
    ///         values[obj.Name.ToString()] = reader.String()
    ///     }
    /// </code>
    /// <para>
    /// The next example reads an object with two expected property names, "a" and "b". Any =
    /// unrecognized properties are ignored.
    /// </para>
    /// <code>
    ///     int aValue, bValue;
    ///     for (var obj = reader.ObjectOrNull(); obj.Next(ref reader);)
    ///     {
    ///         switch (obj.Name)
    ///         {
    ///             case var n when n == "a":
    ///                 a = reader.Int();
    ///                 break;
    ///             case var n when n == "b":
    ///                 b = reader.Int();
    ///                 break;
    ///         }
    ///     }
    /// </code>
    /// <para>
    /// Note that this <c>switch</c> block uses <c>when</c> clauses rather than simple
    /// <c>case "a":</c>, etc., because <c>obj.Name</c> is not a <c>string</c>. For details, see
    /// <see cref="PropertyNameToken"/>. You could also do the following, which is simpler but may
    /// cause more heap allocations:
    /// </para>
    /// <code>
    ///     int aValue, bValue;
    ///     for (var obj = reader.ObjectOrNull(); obj.Next(ref reader);)
    ///     {
    ///         switch (obj.Name.ToString())
    ///         {
    ///             case "a":
    ///                 a = reader.Int();
    ///                 break;
    ///             case "b":
    ///                 b = reader.Int();
    ///                 break;
    ///         }
    ///     }
    /// </code>
    /// </remarks>
    public ref struct ObjectReader
    {
        private readonly bool _defined;
        private readonly string[] _requiredProperties;
        private readonly bool[] _foundProperties;
        private bool _afterFirst;
        private PropertyNameToken _name;

        internal ObjectReader(bool defined, string[] requiredProperties)
        {
            _defined = defined;
            _requiredProperties = requiredProperties;
            _foundProperties = !defined || requiredProperties is null ?
                null : new bool[requiredProperties.Length];
            _afterFirst = false;
            _name = new PropertyNameToken();
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
        /// <para>
        /// This value is initialized by calling <see cref="Next"/>.
        /// </para>
        /// <para>
        /// The type of this property is not <c>string</c>, but it can be compared to strings. See
        /// <see cref="PropertyNameToken"/> for details.
        /// </para>
        /// </remarks>
        public PropertyNameToken Name => _name;

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
        ///     for (var obj = reader.Object().WithRequiredProperties(requiredProps); obj.Next(ref reader);)
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
        /// If <c>Next</c> returns <see langword="true"/>, you can then use <see cref="Name"/> or
        /// <see cref="NameIs(string)"/> to check the name of the property, and use <see cref="JReader"/>
        /// methods such as <see cref="JReader.Bool"/> to read the element value. If you do not care about
        /// the value, simply calling <c>Next</c> again without calling a <c>JReader</c> method will
        /// discard the value.
        /// </para>
        /// </remarks>
        /// <returns><see langword="true"/> if there is a next object property</returns>
        public bool Next(ref JReader reader)
        {
            if (!_defined)
            {
                return false;
            }
            _name = reader.ObjectNext(!_afterFirst);
            _afterFirst = true;
            if (!_name.Empty)
            {
                if (_requiredProperties != null)
                {
                    for (int i = 0; i < _requiredProperties.Length; i++)
                    {
                        if (_name.Equals(_requiredProperties[i]))
                        {
                            _foundProperties[i] = true;
                            break;
                        }
                    }
                }
                return true;
            }
            if (_requiredProperties != null)
            {
                for (int i = 0; i < _requiredProperties.Length; i++)
                {
                    if (!_foundProperties[i])
                    {
                        throw new RequiredPropertyException(_requiredProperties[i],
                            reader.LastPos);
                    }
                }
            }
            return false;
        }
    }
}
