using System;
using System.Reflection;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Apply this attribute to a type that can use the <c>LaunchDarkly.JsonStream</c> API to
    /// serialize or deserialize its instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The specified type must implement <see cref="IJsonStreamConverter"/>. This allows
    /// the <see cref="JsonStreamConvert"/> methods to work with the target type:
    /// </para>
    /// <example>
    /// <code>
    ///     [JsonStreamConverter(typeof(MyConverter))]
    ///     public class MyType { ... }
    ///
    ///     public class MyConverter : IJsonStreamConverter
    ///     {
    ///         // implement IJsonStreamConverter methods to handle MyType
    ///     }
    ///
    ///     var instanceToEncode = new MyType();
    ///     var someJson = JsonStreamConvert.SerializeObject(instanceToEncode);
    ///
    ///     var decodedInstance = JsonStreamConvert.DeserializeObject&lt;MyType&gt;(someJson);
    /// </code>
    /// </example>
    /// <para>
    /// On platforms where <c>System.Text.Json</c> is available, this attribute also works as
    /// an equivalent to the <c>[JsonConverter]</c> attribute, so <c>System.Text.Json.JsonSerializer</c>
    /// methods will also work with the target type and will delegate to the specified converter.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class JsonStreamConverterAttribute :
#if USE_SYSTEM_TEXT_JSON
        System.Text.Json.Serialization.JsonConverterAttribute
#else
        System.Attribute
#endif
    {
        internal IJsonStreamConverter Converter { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="converterType">a type that implements <see cref="IJsonStreamConverter"/></param>
        public JsonStreamConverterAttribute(Type converterType)
#if USE_SYSTEM_TEXT_JSON
            : base(typeof(JsonStreamConverterSystemTextJson))
            // Calling the constructor for the base class here means that the class has an implied
            // attribute of [JsonConverter(typeof(JsonStreamConverter))], so System.Text.Json will
            // know to delegate to our logic.
#endif
        {
            if (!typeof(IJsonStreamConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException("type for JsonStreamConverterAttribute must implement IJsonStreamConverter");
            }
            var ctor = converterType.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new ArgumentException("type for JsonStreamConverterAttribute must have a no-argument public constructor");
            }
            Converter = ctor.Invoke(null) as IJsonStreamConverter;
        }

        internal static JsonStreamConverterAttribute ForTargetType(Type targetType)
        {
            var attr = targetType.GetCustomAttribute(typeof(JsonStreamConverterAttribute)) as JsonStreamConverterAttribute;
            if (attr is null)
            {
                throw new ArgumentException(string.Format("{0} does not have JsonStreamConverterAttribute", targetType.FullName));
            }
            return attr;
        }
    }
}
