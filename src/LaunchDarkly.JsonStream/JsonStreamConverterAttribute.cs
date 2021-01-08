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
    /// The specified type must implement <see cref="IJsonStreamConverter{T}"/>. This allows
    /// the <see cref="JsonStreamConvert"/> methods to work with the target type:
    /// </para>
    /// <example>
    /// <code>
    ///     [JsonStreamConverter(typeof(MyConverter))]
    ///     public class MyType { ... }
    ///
    ///     public class MyConverter : IJsonStreamConverter&lt;MyType&gt;
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
        // The type of this object is really some implementation of IJsonStreamConverter<T> where T is
        // the type specified in the attribute, but that's not available as a generic type parameter in
        // this context so we'll have to cast it to that later.
        internal object UntypedConverter { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="converterType">a type that implements <see cref="IJsonStreamConverter{T}"/></param>
        public JsonStreamConverterAttribute(Type converterType)
#if USE_SYSTEM_TEXT_JSON
            : base(typeof(JsonStreamConverterSystemTextJson))
            // Calling the constructor for the base class here means that the class has an implied
            // attribute of [JsonConverter(typeof(JsonStreamConverter))], so System.Text.Json will
            // know to delegate to our logic.
#endif
        {
            var ctor = converterType.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new ArgumentException("type for JsonStreamConverterAttribute must have a no-argument public constructor");
            }
            UntypedConverter = ctor.Invoke(null);
        }

        internal static JsonStreamConverterAttribute ForTargetType(Type targetType)
        {
            var attr = targetType.GetCustomAttribute(typeof(JsonStreamConverterAttribute)) as JsonStreamConverterAttribute;
            if (attr is null)
            {
                throw new InvalidOperationException(string.Format("{0} does not have JsonStreamConverterAttribute", targetType.FullName));
            }
            var desiredInterface = typeof(IJsonStreamConverter<>).MakeGenericType(targetType);
            if (!desiredInterface.IsAssignableFrom(attr.UntypedConverter.GetType()))
            {
                throw new InvalidOperationException(string.Format(
                    "{0} was specified in JsonStreamConverterAttribute for {1} but does not implement IJsonStreamConverter<{1}>",
                    attr.UntypedConverter.GetType().Name, targetType.Name));
            }
            return attr;
        }

        internal static IJsonStreamConverter<T> GetConverter<T>()
        {
            var attr = ForTargetType(typeof(T));
            return attr.UntypedConverter as IJsonStreamConverter<T>;
        }
    }
}
