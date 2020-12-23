#if USE_SYSTEM_TEXT_JSON

// This implementation of TokenWriter is used on platforms that have the System.Text.Json API.

using System;
using System.IO;
using System.Text;
using System.Text.Json;

// On .NET Core 3+ and .NET 5+, we can use ArrayBufferWriter, which is more efficient than
// MemoryStream when it comes to converting the output to a string. But on other platforms where
// we're getting System.Text.Json from a NuGet package, the version of ArrayBufferWriter that's
// in that package isn't public.
#if NETCOREAPP3_1 || NET5_0
using System.Buffers;
#endif

namespace LaunchDarkly.JsonStream.Implementation
{
    internal partial class TokenWriter
    {
#if NETCOREAPP3_1 || NET5_0
        private readonly ArrayBufferWriter<byte> _buffer;
#else
        private readonly MemoryStream _buffer;
#endif
        private readonly Utf8JsonWriter _nativeWriter;
        private string _propertyName;

        public TokenWriter(int initialCapacity)
        {
#if NETCOREAPP3_1 || NET5_0
            _buffer = new ArrayBufferWriter<byte>(initialCapacity);
#else
            _buffer = new MemoryStream(initialCapacity);
#endif
            _nativeWriter = new Utf8JsonWriter(_buffer);
        }

        public TokenWriter(Utf8JsonWriter nativeWriter)
        {
            _buffer = null;
            _nativeWriter = nativeWriter;
        }

        public string GetString()
        {
            if (_buffer is null)
            {
                return null;
            }
            _nativeWriter.Flush();
#if NETCOREAPP3_1 || NET5_0
            return Encoding.UTF8.GetString(_buffer.WrittenSpan); // more efficient because it doesn't create an array
#else
            return Encoding.UTF8.GetString(_buffer.ToArray());
#endif
        }

        public byte[] GetUtf8Bytes()
        {
            if (_buffer is null)
            {
                return null;
            }
            _nativeWriter.Flush();
#if NETCOREAPP3_1 || NET5_0
            return _buffer.WrittenSpan.ToArray();
#else
            return _buffer.ToArray();
#endif
        }

        public Stream GetUtf8Stream()
        {
            if (_buffer is null)
            {
                return null;
            }
            _nativeWriter.Flush();
#if NETCOREAPP3_1 || NET5_0
            return new MemoryStream(_buffer.WrittenSpan.ToArray());
#else
            _buffer.Position = 0;
            return _buffer;
#endif
        }

#if NETCOREAPP3_1 || NET5_0
        public ReadOnlyMemory<byte> GetUtf8ReadOnlyMemory()
        {
            if (_buffer is null)
            {
                return null;
            }
            _nativeWriter.Flush();
            return _buffer.WrittenMemory;
        }
#endif

        public void Null()
        {
            if (_propertyName is null)
            {
                _nativeWriter.WriteNullValue();
            }
            else
            {
                _nativeWriter.WriteNull(_propertyName);
                _propertyName = null;
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
                _propertyName = null;
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
                _propertyName = null;
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
                _propertyName = null;
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
                    _propertyName = null;
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
                _propertyName = null;
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
                _propertyName = null;
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
