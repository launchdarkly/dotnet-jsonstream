#if USE_SYSTEM_TEXT_JSON

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaunchDarkly.JsonStream
{
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
                attr.Converter
                );
        }

        private sealed class ConverterImpl<T> : JsonConverter<T>
        {
            private IJsonStreamConverter _jsonStreamConverter;

            public ConverterImpl(IJsonStreamConverter jsonStreamConverter)
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
                using(JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var readerWrapper = JReader.FromAdapter(ReaderAdapters.FromJsonElement(jsonDocument.RootElement));
                    return (T)_jsonStreamConverter.ReadJson(ref readerWrapper);
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var writerWrapper = JWriter.NewWithUtf8JsonWriter(writer);
                _jsonStreamConverter.WriteJson(value, writerWrapper);
            }
        }

    }
}

#endif
