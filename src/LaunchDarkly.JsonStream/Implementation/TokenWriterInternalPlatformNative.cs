#if NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0

// This implementation of TokenWriter is used on platforms that have the System.Text.Json API.

using System.Buffers;
using System.Text;
using System.Text.Json;

namespace LaunchDarkly.JsonStream.Implementation
{
    internal partial class TokenWriter
    {
        private readonly ArrayBufferWriter<byte> _buffer;
        private readonly Utf8JsonWriter _nativeWriter;
        private string _propertyName;

        public TokenWriter(int initialCapacity)
        {
            _buffer = new ArrayBufferWriter<byte>(initialCapacity);
            _nativeWriter = new Utf8JsonWriter(_buffer);
        }

        public string GetString()
        {
            _nativeWriter.Flush();
            return Encoding.UTF8.GetString(_buffer.WrittenSpan);
        }

        public byte[] GetUTF8Bytes()
        {
            _nativeWriter.Flush();
            return _buffer.WrittenSpan.ToArray();
        }

        public void Null()
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteNullValue();
            }
            else
            {
                _nativeWriter.WriteNull(_propertyName);
            }
        }

        public void Bool(bool value)
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteBooleanValue(value);
            }
            else
            {
                _nativeWriter.WriteBoolean(_propertyName, value);
            }
        }

        public void Long(long value)
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteNumberValue(value);
            }
            else
            {
                _nativeWriter.WriteNumber(_propertyName, value);
            }
        }

        public void Double(double value)
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteNumberValue(value);
            }
            else
            {
                _nativeWriter.WriteNumber(_propertyName, value);
            }
        }

        public void String(string value)
        {
            if (value == null)
            {
                Null();
            }
            else
            {
                if (_propertyName is null)
                {
                    _nativeWriter.WriteStringValue(value);
                }
                else
                {
                    _nativeWriter.WriteString(_propertyName, value);
                }
            }
        }

        public void StartArray()
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteStartArray();
            }
            else
            {
                _nativeWriter.WriteStartArray(_propertyName);
            }
        }

        public void NextArrayItem()
        {
            // Utf8JsonWriter does this automatically when we're in an array
        }

        public void EndArray()
        {
            _nativeWriter.WriteEndArray();
        }

        public void StartObject()
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteStartObject();
            }
            else
            {
                _nativeWriter.WriteStartObject(_propertyName);
            }
        }

        public void NextObjectItem(string name, bool first)
        {
            _propertyName = name;
        }

        public void EndObject()
        {
            _nativeWriter.WriteEndObject();
        }
    }
}

#endif
