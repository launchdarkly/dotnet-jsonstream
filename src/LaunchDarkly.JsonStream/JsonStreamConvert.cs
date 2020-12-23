using System;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Helper methods for serializing and deserializing types that are annotated with
    /// the <c>[JsonStreamConverter]</c> attribute.
    /// </summary>
    public static class JsonStreamConvert
    {
        /// <summary>
        /// Creates a JSON representation of a value using whatever converter is
        /// specified in that type's attributes.
        /// </summary>
        /// <typeparam name="T">a type that has a JSON conversion defined with a
        /// <c>[JsonStreamConverter]</c> attribute</typeparam>
        /// <param name="instance">an instance of type T</param>
        /// <returns>the serialized JSON data as a string</returns>
        /// <exception cref="ArgumentException">if type T does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <seealso cref="JsonStreamConverterAttribute"/>
        /// <seealso cref="SerializeObjectToUtf8Bytes{T}(T)"/>
        public static string SerializeObject<T>(T instance)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var writer = JWriter.New();
            converter.WriteJson(instance, writer);
            return writer.GetString();
        }

        /// <summary>
        /// Same as <see cref="SerializeObject{T}(T)"/>, but returns the JSON output
        /// as a byte array using UTF8 encoding.
        /// </summary>
        /// <typeparam name="T">a type that has a JSON conversion defined with a
        /// <c>[JsonStreamConverter]</c> attribute</typeparam>
        /// <param name="instance">an instance of type T</param>
        /// <returns>the serialized JSON data as a byte array</returns>
        /// <exception cref="ArgumentException">if type T does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <seealso cref="JsonStreamConverterAttribute"/>
        /// <seealso cref="SerializeObject{T}(T)"/>
        public static byte[] SerializeObjectToUtf8Bytes<T>(T instance)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var writer = JWriter.New();
            converter.WriteJson(instance, writer);
            return writer.GetUtf8Bytes();
        }

        /// <summary>
        /// Decodes a value from a JSON representation using whatever converter is
        /// specified in its type's attributes.
        /// </summary>
        /// <typeparam name="T">a type that has a JSON conversion defined with a
        /// <c>[JsonStreamConverter]</c> attribute</typeparam>
        /// <param name="json">the JSON representation as a string</param>
        /// <returns>an instance of type T</returns>
        /// <exception cref="ArgumentException">if type T does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <exception cref="JsonReadException">if an error occurred in parsing
        /// the JSON or translating it to the desired type; see subclasses of
        /// <see cref="JsonReadException"/> for more specific errors</exception>
        public static T DeserializeObject<T>(string json)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var reader = JReader.FromString(json);
            try
            {
                return converter.ReadJson(ref reader);
            }
            catch (Exception ex)
            {
                throw reader.TranslateException(ex);
            }
        }
    }
}
