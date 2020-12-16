
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Thrown by <see cref="JReader"/> when the input data contains a value of an unexpected
    /// type, even though it is well-formed JSON.
    /// </summary>
    /// <remarks>
    /// For instance, this is thrown if you call <see cref="JReader.Bool"/> but the next value
    /// in the input data is a number.
    /// </remarks>
    public sealed class TypeException : JsonReadException
    {
        /// <summary>
        /// The type that was requested by the caller.
        /// </summary>
        public ValueType ExpectedType { get; }

        /// <summary>
        /// The type that was found in the input data.
        /// </summary>
        public ValueType ActualType { get; }

        public override string Message =>
            string.Format("expected {0} but got {1} at position {2}",
                ExpectedType, ActualType, Offset);

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="expectedType">the type that was requested by the caller</param>
        /// <param name="actualType">the type that was found in the input data</param>
        /// <param name="offset">the approximate character offset within the input data where the error occurred</param>
        public TypeException(ValueType expectedType, ValueType actualType,
            int? offset) : base(offset)
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }
    }
}
