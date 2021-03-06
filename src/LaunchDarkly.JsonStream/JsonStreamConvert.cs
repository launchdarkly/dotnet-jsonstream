﻿using System;
using System.Collections.Generic;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Helper methods for serializing and deserializing types that have <see cref="IJsonStreamConverter"/>
    /// implementations.
    /// </summary>
    public static class JsonStreamConvert
    {
        /// <summary>
        /// Creates a JSON representation of a value using the converter specified in
        /// the type's attributes.
        /// </summary>
        /// <param name="instance">an instance of some type</param>
        /// <returns>the serialized JSON data as a string</returns>
        /// <exception cref="ArgumentException">if the type does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <seealso cref="JsonStreamConverterAttribute"/>
        /// <seealso cref="SerializeObjectToUtf8Bytes(object)"/>
        public static string SerializeObject(object instance)
        {
            if (instance is null)
            {
                return "null";
            }
            return SerializeObject(instance,
                JsonStreamConverterAttribute.ForTargetType(instance.GetType()).Converter);
        }

        /// <summary>
        /// Uses the specified converter to create a JSON representation as a string.
        /// </summary>
        /// <param name="instance">an instance of some type</param>
        /// <param name="converter">a converter for that type</param>
        /// <returns>the serialized JSON data as a string</returns>
        public static string SerializeObject(object instance, IJsonStreamConverter converter)
        {
            var writer = JWriter.New();
            SerializeObjectToJWriter(instance, writer, converter);
            return writer.GetString();
        }

        /// <summary>
        /// Same as <see cref="SerializeObject(object)"/>, but returns the JSON output
        /// as a byte array using UTF8 encoding.
        /// </summary>
        /// <param name="instance">an instance of some type</param>
        /// <returns>the serialized JSON data as a byte array</returns>
        /// <exception cref="ArgumentException">if the type does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <seealso cref="JsonStreamConverterAttribute"/>
        /// <seealso cref="SerializeObject(object)"/>
        public static byte[] SerializeObjectToUtf8Bytes(object instance)
        {
            var writer = JWriter.New();
            if (instance is null)
            {
                writer.Null();
            }
            else
            {
                SerializeObjectToJWriter(instance, writer,
                    JsonStreamConverterAttribute.ForTargetType(instance.GetType()).Converter);
            }
            return writer.GetUtf8Bytes();
        }

        private static void SerializeObjectToJWriter(object instance, IValueWriter writer, IJsonStreamConverter converter)
        {
            if (converter is null)
            {
                throw new NullReferenceException(nameof(converter));
            }
            if (instance is null)
            {
                writer.Null();
            }
            else
            {
                converter.WriteJson(instance, writer);
            }
        }

        /// <summary>
        /// Decodes a value from a JSON representation using the converter specified
        /// in the type's attributes.
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
        public static T DeserializeObject<T>(string json) => (T)DeserializeObject(json, typeof(T));

        /// <summary>
        /// Decodes a value from a JSON representation using the converter specified
        /// in the type's attributes.
        /// </summary>
        /// <remarks>
        /// This is the same as <see cref="DeserializeObject{T}(string)"/>, but for cases
        /// where the type is not known at compile time.
        /// </remarks>
        /// <param name="json">the JSON representation as a string</param>
        /// <param name="type">the desired type</param>
        /// <returns>an instance of that type</returns>
        /// <exception cref="ArgumentException">if the type does not have a
        /// <c>[JsonStreamConverter]</c> attribute</exception>
        /// <exception cref="JsonReadException">if an error occurred in parsing
        /// the JSON or translating it to the desired type; see subclasses of
        /// <see cref="JsonReadException"/> for more specific errors</exception>
        public static object DeserializeObject(string json, Type type) =>
            DeserializeObject(json, JsonStreamConverterAttribute.ForTargetType(type).Converter);

        /// <summary>
        /// Decodes a value from a JSON representation using the specified converter.
        /// </summary>
        /// <param name="json">the JSON representation as a string</param>
        /// <param name="converter">a converter for the desired type</param>
        /// <returns>an instance of that type</returns>
        /// <exception cref="JsonReadException">if an error occurred in parsing
        /// the JSON or translating it to the desired type; see subclasses of
        /// <see cref="JsonReadException"/> for more specific errors</exception>
        public static object DeserializeObject(string json, IJsonStreamConverter converter)
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
        /// Returns an <see cref="IJsonStreamConverter"/> that converts between JSON and
        /// a simple set of .NET data types.
        /// </summary>
        /// <remarks>
        /// The supported scalar types are <c>bool</c>, <c>int</c>, <c>long</c>, <c>float</c>,
        /// <c>double</c>, and <c>string</c> (when reading, numbers are always returned as
        /// <c>double</c>). Nulls are <c>null</c>. Arrays are read as <c>List&lt;object&gt;</c>
        /// and can be written from any <c>IEnumerable&lt;object&gt;</c>. JSON objects are
        /// read as <c>Dictionary&lt;string, object&gt;</c> and can be written from any
        /// <c>IReadOnlyDictionary&lt;string, object&gt;</c> or
        /// <c>IReadOnlyDictionary&lt;object, object&gt;</c>.
        /// </remarks>
        public static IJsonStreamConverter ConvertSimpleTypes =>
            new SimpleTypesConverter();

        private sealed class SimpleTypesConverter : IJsonStreamConverter
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
                            WriteJson(kv.Value, obj.Name(kv.Key));
                        }
                        obj.End();
                        break;
                    case IReadOnlyDictionary<object, object> dict:
                        var obj1 = writer.Object();
                        foreach (var kv in dict)
                        {
                            WriteJson(kv.Value, obj1.Name(kv.Key.ToString()));
                        }
                        obj1.End();
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
