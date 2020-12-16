
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Thrown by <see cref="JReader"/> when the input data is not well-formed JSON.
    /// </summary>
    public sealed class SyntaxException : JsonReadException
    {
        /// <summary>
        /// The error message, not including the character offset.
        /// </summary>
        public string BaseMessage { get; }

        public override string Message =>
            string.Format("{0} at position {1}", BaseMessage, Offset);

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="baseMessage">the error message</param>
        /// <param name="offset">the approximate character offset within the input data where the error occurred</param>
        public SyntaxException(string baseMessage, int? offset) : base(offset)
        {
            BaseMessage = baseMessage;
        }
    }
}
