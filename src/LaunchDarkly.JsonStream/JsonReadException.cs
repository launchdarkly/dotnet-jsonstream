using System;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Base class for all exceptions thrown by <see cref="JReader"/>.
    /// </summary>
    public class JsonReadException : Exception
    {
        /// <summary>
        /// The approximate character offset within the input data where the error occurred,
        /// if this can be determined.
        /// </summary>
        public int? Offset { get; }

        /// <summary>
        /// Base class constructor.
        /// </summary>
        /// <param name="offset">the character offset of the error, if known</param>
        protected JsonReadException(int? offset)
        {
            Offset = offset;
        }
    }
}
