using System;
using System.Collections.Generic;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Helper methods for serializing and deserializing types that have <see cref="IJsonStreamConverter{T}"/>
    /// implementations.
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
        public static string SerializeObject<T>(T instance) =>
            SerializeObject(instance, JsonStreamConverterAttribute.GetConverter<T>());

        /// <summary>
        /// Uses the specified converter to create a JSON representation as a string.
        /// </summary>
        /// <typeparam name="T">an arbitrary type</typeparam>
        /// <param name="instance">an instance of type T</param>
        /// <param name="converter">a converter for type T</param>
        /// <returns>the serialized JSON data as a string</returns>
        public static string SerializeObject<T>(T instance, IJsonStreamConverter<T> converter)
        {
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
        public static T DeserializeObject<T>(string json) =>
            DeserializeObject(json, JsonStreamConverterAttribute.GetConverter<T>());

        /// <summary>
        /// Decodes a value from a JSON representation using the specified converter.
        /// </summary>
        /// <typeparam name="T">an arbitrary type</typeparam>
        /// <param name="json">the JSON representation as a string</param>
        /// <param name="converter">a converter for type T</param>
        /// <returns>an instance of type T</returns>
        /// <exception cref="JsonReadException">if an error occurred in parsing
        /// the JSON or translating it to the desired type; see subclasses of
        /// <see cref="JsonReadException"/> for more specific errors</exception>
        public static T DeserializeObject<T>(string json, IJsonStreamConverter<T> converter)
        {
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

        /// <summary>
        /// Returns an <see cref="IJsonStreamConverter{T}"/> that converts between JSON and
        /// a simple set of .NET data types.
        /// </summary>
        /// <remarks>
        /// The supported scalar types are <c>bool</c>, <c>int</c>, <c>long</c>, <c>float</c>,
        /// <c>double</c>, and <c>string</c> (when reading, numbers are always returned as
        /// <c>double</c>). Nulls are <c>null</c>. Arrays are read as <c>List&lt;object&gt;</c>
        /// and can be written from any <c>IEnumerable&lt;object&gt;</c>. JSON objects are
        /// read as <c>Dictionary&lt;string, object&gt;</c> and can be written from any
        /// <c>IReadOnlyDictionary&lt;string, object&gt;</c>.
        /// </remarks>
        public static IJsonStreamConverter<object> ConvertSimpleTypes =>
            new SimpleTypesConverter();

        private sealed class SimpleTypesConverter : IJsonStreamConverter<object>
        {
            public object ReadJson(ref JReader reader)
            {
                var value = reader.Any();
                switch (value.Type)
                {
                    case ValueType.Bool:
                        return value.BoolValue;
                    case ValueType.Number:
                        return value.NumberValue;
                    case ValueType.String:
                        return value.StringValue;
                    case ValueType.Array:
                        var list = new List<object>();
                        for (var arr = value.ArrayValue; arr.Next(ref reader);)
                        {
                            list.Add(ReadJson(ref reader));
                        }
                        return list;
                    case ValueType.Object:
                        var dict = new Dictionary<string, object>();
                        for (var obj = value.ObjectValue; obj.Next(ref reader);)
                        {
                            dict[obj.Name.ToString()] = ReadJson(ref reader);
                        }
                        return dict;
                    default:
                        return null;
                }
            }

            public void WriteJson(object instance, IValueWriter writer)
            {
                switch (instance)
                {
                    case null:
                        writer.Null();
                        break;
                    case bool value:
                        writer.Bool(value);
                        break;
                    case int value:
                        writer.Int(value);
                        break;
                    case long value:
                        writer.Long(value);
                        break;
                    case float value:
                        writer.Double(value);
                        break;
                    case double value:
                        writer.Double(value);
                        break;
                    case string value:
                        writer.String(value);
                        break;
                    case IReadOnlyDictionary<string, object> dict:
                        var obj = writer.Object();
                        foreach (var kv in dict)
                        {
                            WriteJson(kv.Value, obj.Property(kv.Key));
                        }
                        obj.End();
                        break;
                    case IEnumerable<object> list:
                        var arr = writer.Array();
                        foreach (var o in list)
                        {
                            WriteJson(o, arr);
                        }
                        arr.End();
                        break;
                    default:
                        throw new ArgumentException(string.Format("ConvertSimpleTypes does not support type {0}",
                            instance.GetType()));
                }
            }
        }
    }
}
