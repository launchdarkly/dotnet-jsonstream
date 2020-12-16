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

        protected JsonReadException(int? offset)
        {
            Offset = offset;
        }

//        public static Exception FromException(Exception e)
//        {
//            if (e is JsonReadException)
//            {
//                return e;
//            }
//#if NETCORE3_0 || NET5_0
//            if (e is System.Text.Json.JsonException je)
//            {
//                return new SyntaxException(je.Message, )
//            }
//#endif
//        }
    }
}
