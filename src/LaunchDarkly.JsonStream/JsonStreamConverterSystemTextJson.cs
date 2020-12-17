#if USE_SYSTEM_TEXT_JSON

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LaunchDarkly.JsonStream.Implementation;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class JsonStreamConverterSystemTextJson : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var attr = JsonStreamConverterAttribute.ForTargetType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(
                typeof(ConverterImpl<>).MakeGenericType(typeToConvert),
                attr.UntypedConverter
                );
        }

        private sealed class ConverterImpl<T> : JsonConverter<T>
        {
            private IJsonStreamConverter<T> _jsonStreamConverter;

            public ConverterImpl(IJsonStreamConverter<T> jsonStreamConverter)
            {
                _jsonStreamConverter = jsonStreamConverter;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Unfortunately we can't hook directly into an existing Utf8JsonReader, because JReader (like any
                // other type) isn't allowed to retain a reference to a ref struct outside of itself. So we have
                // to parse out the next JSON value or tree all at once, and then wrap it in a delegate object
                // that a JReader can read from. That's less efficient than reading directly from the original
                // reader, due to 1. preallocating the value struct(s) and 2. the overhead of JReader calling the
                // delegate, but it's still better than actually parsing the JSON twice.
                var jsonDocument = JsonDocument.ParseValue(ref reader);
                var readerWrapper = new JReader(new JsonElementReaderDelegate(jsonDocument.RootElement));
                return _jsonStreamConverter.ReadJson(ref readerWrapper);
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var writerWrapper = JWriter.NewWithUtf8JsonWriter(writer);
                _jsonStreamConverter.WriteJson(value, writerWrapper);
            }
        }

        private sealed class JsonElementReaderDelegate : IReaderDelegate
        {
            private JsonElement? _current;
            private ParentContext _parentContext;
            private Stack<ParentContext> _parentContextStack;

            private struct ParentContext
            {
                internal JsonElement.ArrayEnumerator ArrayEnumerator;
                internal JsonElement.ObjectEnumerator ObjectEnumerator;
            }

            internal JsonElementReaderDelegate(JsonElement rootElement)
            {
                _current = rootElement;
            }

            public bool EOF => !_current.HasValue;

            public void Null()
            {
                RequireValueOfType(ValueType.Null, true);
            }

            public bool Bool()
            {
                return RequireValueOfType(ValueType.Bool, false).BoolValue;
            }

            public bool? BoolOrNull()
            {
                var value = RequireValueOfType(ValueType.Bool, true);
                return value.Type == ValueType.Bool ? value.BoolValue : (bool?)null;
            }

            public double Number()
            {
                return RequireValueOfType(ValueType.Number, false).NumberValue;
            }

            public double? NumberOrNull()
            {
                var value = RequireValueOfType(ValueType.Number, true);
                return value.Type == ValueType.Number ? value.NumberValue : (double?)null;
            }

            public string String()
            {
                return RequireValueOfType(ValueType.String, false).StringValue;
            }

            public string StringOrNull()
            {
                return RequireValueOfType(ValueType.String, true).StringValue;
            }

            public ArrayReader Array()
            {
                return RequireValueOfType(ValueType.Array, false).ArrayValue;
            }

            public ArrayReader ArrayOrNull()
            {
                return RequireValueOfType(ValueType.Array, true).ArrayValue;
            }

            public bool ArrayNext(bool first)
            {
                if (_parentContext.ArrayEnumerator.MoveNext())
                {
                    _current = _parentContext.ArrayEnumerator.Current;
                    return true;
                }
                _parentContext = _parentContextStack.Pop();
                return false;
            }

            public ObjectReader Object()
            {
                return RequireValueOfType(ValueType.Object, false).ObjectValue;
            }

            public ObjectReader ObjectOrNull()
            {
                return RequireValueOfType(ValueType.Object, true).ObjectValue;
            }

            public PropertyNameToken ObjectNext(bool first)
            {
                if (_parentContext.ObjectEnumerator.MoveNext())
                {
                    var property = _parentContext.ObjectEnumerator.Current;
                    _current = property.Value;
                    return new PropertyNameToken(property);
                }
                _parentContext = _parentContextStack.Pop();
                return new PropertyNameToken();
            }

            public AnyValue Any()
            {
                if (!_current.HasValue)
                {
                    throw new SyntaxException("unexpected end of input", null);
                }
                var element = _current.Value;
                _current = null;
                switch (element.ValueKind)
                {
                    case JsonValueKind.True:
                        return AnyValue.Bool(true);
                    case JsonValueKind.False:
                        return AnyValue.Bool(false);
                    case JsonValueKind.Number:
                        return AnyValue.Number(element.GetDouble());
                    case JsonValueKind.String:
                        return AnyValue.String(element.GetString());
                    case JsonValueKind.Array:
                        if (_parentContextStack is null)
                        {
                            _parentContextStack = new Stack<ParentContext>();
                        }
                        _parentContextStack.Push(_parentContext);
                        _parentContext.ArrayEnumerator = element.EnumerateArray();
                        return AnyValue.Array(new ArrayReader(true));
                    case JsonValueKind.Object:
                        if (_parentContextStack is null)
                        {
                            _parentContextStack = new Stack<ParentContext>();
                        }
                        _parentContextStack.Push(_parentContext);
                        _parentContext.ObjectEnumerator = element.EnumerateObject();
                        return AnyValue.Object(new ObjectReader(true, null));
                    default:
                        return AnyValue.Null();
                }
            }

            public void SkipValue()
            {
                _ = Any();
            }

            private AnyValue RequireValueOfType(ValueType expectedType, bool nullable)
            {
                var value = Any();
                if (value.Type != expectedType && !(nullable && value.Type == ValueType.Null))
                {
                    throw new TypeException(expectedType, value.Type, null);
                }
                return value;
            }
        }
    }
}

#endif
