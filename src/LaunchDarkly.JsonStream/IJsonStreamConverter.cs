
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Interface for an object that knows how to convert some type to or from JSON.
    /// </summary>
    /// <remarks>
    /// This interface uses the type <c>object</c> instead of specifying the actual target
    /// object type as a generic type parameter. This is because <see cref="JsonStreamConvert"/>
    /// may need to use reflection to access this interface, and <c>ref struct</c> types like
    /// <c>JReader</c> aren't compatible with that mechanism. It is the implementator's job to
    /// ensure that the converter uses the appropriate type; otherwise a type cast error will
    /// occur.
    /// </remarks>
    /// <seealso cref="JsonStreamConvert"/>
    /// <seealso cref="JsonStreamConverterAttribute"/>
    public interface IJsonStreamConverter
    {
        /// <summary>
        /// Writes the JSON representation of an object.
        /// </summary>
        /// <param name="instance">the object</param>
        /// <param name="writer">the streaming writer</param>
        void WriteJson(object instance, IValueWriter writer);

        /// <summary>
        /// Reads an object from its JSON representation.
        /// </summary>
        /// <param name="reader">the streaming reader</param>
        /// <returns>the deserialized object</returns>
        object ReadJson(ref JReader reader);
    }
}
