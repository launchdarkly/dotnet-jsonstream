
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// An interface for providing JSON-equivalent data from some other source that can be
    /// read by <see cref="JReader"/>.
    /// </summary>
    /// <remarks>
    /// This allows deserialization logic based on <see cref="JReader"/> to be used on a custom input stream
    /// without involving the actual JSON parser. The adapter is responsible for maintaining its
    /// own state information regarding the position of the next value to be read.
    /// </remarks>
    /// <seealso cref="JReader.FromAdapter(IReaderAdapter)"/>
    /// <seealso cref="ReaderAdapters"/>
    public interface IReaderAdapter
    {
        /// <summary>
        /// Returns true if there is no more input data.
        /// </summary>
        /// <exception cref="SyntaxException">if the input data is malformed</exception>
        bool EOF { get; }

        /// <summary>
        /// Consumes the next value of a scalar type, or prepares to read the contents
        /// of an array or object.
        /// </summary>
        /// <remarks>
        /// The adapter should throw an exception if there is a type mismatch.
        /// </remarks>
        /// <param name="desiredType">the desired data type, or null if any type is allowable</param>
        /// <param name="allowNull">true if a null value is allowed instead of the desired type</param>
        /// <returns>an <see cref="AnyValue"/> that contains the actual value if it is a scalar,
        /// or just indicates the type if it is an array or object</returns>
        /// <exception cref="TypeException">if the actual type of the next value does not match
        /// the desired type</exception>
        /// <exception cref="SyntaxException">if the input data is malformed</exception>
        AnyValue NextValue(ValueType? desiredType, bool allowNull);

        /// <summary>
        /// Prepares to read the next element of an array.
        /// </summary>
        /// <remarks>
        /// This method will only be called if <see cref="NextValue(ValueType?, bool)"/>
        /// previously returned <see cref="ValueType.Array"/>. It should update the adapter's state
        /// so that the next call to <see cref="NextValue(ValueType?, bool)"/> will return the
        /// next element of the array. If there are no more array elements, it should return false
        /// and update the adapter's state so that the current position is after the end of the array.
        /// </remarks>
        /// <param name="first">true if this is the first array element being read</param>
        /// <returns>true if there is a next array element, false if there are no more elements</returns>
        /// <exception cref="SyntaxException">if the input data is malformed</exception>
        bool ArrayNext(bool first);

        /// <summary>
        /// Prepares to read the next property of an object.
        /// </summary>
        /// <remarks>
        /// This method will only be called if <see cref="NextValue(ValueType?, bool)"/>
        /// previously returned <see cref="ValueType.Object"/>. It should update the adapter's state
        /// so that the next call to <see cref="NextValue(ValueType?, bool)"/> will return the value
        /// of the next property. If there are no more properties, it should return <see cref="PropertyNameToken.None"/>
        /// and update the adapter's state so that the current position is after the end of the object.
        /// </remarks>
        /// <param name="first">true if this is the first object element being read</param>
        /// <returns>a <see cref="PropertyNameToken"/> for the next property, or
        /// <see cref="PropertyNameToken.None"/> if there are no more properties</returns>
        /// <exception cref="SyntaxException">if the input data is malformed</exception>
        PropertyNameToken ObjectNext(bool first);
    }
}
