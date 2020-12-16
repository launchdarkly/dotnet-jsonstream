
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Thrown by <see cref="ObjectReader"/> if a property that you marked as required was not found.
    /// </summary>
    public sealed class RequiredPropertyException : JsonReadException
    {
        /// <summary>
        /// The name of the missing property.
        /// </summary>
        public string Name { get; }

        public override string Message =>
            string.Format("Missing required property \"{0}\" in JSON object that ended at position {1}", Name, Offset);

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="name">the name of the missing property</param>
        /// <param name="offset">the approximate character offset within the input data where the error occurred</param>
        public RequiredPropertyException(string name, int offset) : base(offset)
        {
            Name = name;
        }
    }
}
