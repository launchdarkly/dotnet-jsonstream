
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Interface for an object that knows how to convert some type to or from JSON.
    /// </summary>
    /// <typeparam name="T">the type to be converted</typeparam>
    /// <seealso cref="JsonStreamConvert"/>
    /// <seealso cref="JsonStreamConverterAttribute"/>
    public interface IJsonStreamConverter<T>
    {
        /// <summary>
        /// Writes the JSON representation of an object.
        /// </summary>
        /// <param name="instance">the object</param>
        /// <param name="writer">the streaming writer</param>
        void WriteJson(T instance, IValueWriter writer);

        /// <summary>
        /// Reads an object from its JSON representation.
        /// </summary>
        /// <param name="reader">the streaming reader</param>
        /// <returns>the deserialized object</returns>
        T ReadJson(ref JReader reader);
    }
}
