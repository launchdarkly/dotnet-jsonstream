#if !USE_SYSTEM_TEXT_JSON

// This implementation of TokenWriter is used on platforms that do not have the
// System.Text.Json API.

using System;
using System.IO;
using System.Text;

namespace LaunchDarkly.JsonStream.Implementation
{
    internal partial class TokenWriter
    {
        private static readonly byte[] _nullBytes = Encoding.UTF8.GetBytes("null");
        private static readonly byte[] _trueBytes = Encoding.UTF8.GetBytes("true");
        private static readonly byte[] _falseBytes = Encoding.UTF8.GetBytes("false");
        private static readonly byte[] _emptyStringBytes = Encoding.UTF8.GetBytes("\"\"");

        private readonly MemoryStream _buf;
        private readonly byte[] _tempBytes = new byte[40];
        private readonly char[] _tempChars = new char[2];

        public TokenWriter(int initialCapacity)
        {
            _buf = new MemoryStream(initialCapacity);
        }

        public string GetString()
        {
            _buf.Seek(0, SeekOrigin.Begin);
            return new StreamReader(_buf).ReadToEnd();
        }

        public byte[] GetUtf8Bytes() => _buf.ToArray();

        public Stream GetUtf8Stream()
        {
            _buf.Seek(0, SeekOrigin.Begin);
            return _buf;
        }
        
        public void Null()
        {
            _buf.Write(_nullBytes, 0, _nullBytes.Length);
        }

        public void Bool(bool value)
        {
            var bytes = value ? _trueBytes : _falseBytes;
            _buf.Write(bytes, 0, bytes.Length);
        }

        public void Long(long value)
        {
            if (value >= 0 && value < 10)
            {
                _buf.WriteByte((byte)('0' + value));
                return;
            }
            // Avoid allocating a string, as the built-in int formatter would do
            var minus = value < 0;
            if (minus)
            {
                value = -value;
            }
            var pos = _tempBytes.Length;
            while (value != 0)
            {
                var digit = value % 10;
                value /= 10;
                _tempBytes[--pos] = (byte)(digit + '0');
            }
            if (minus)
            {
                _tempBytes[--pos] = (byte)'-';
            }
            _buf.Write(_tempBytes, pos, _tempBytes.Length - pos);
        }

        public void Double(double value)
        {
            if (value == 0)
            {
                _buf.WriteByte((byte)'0');
                return;
            }
            long valueInt = (long)value;
            if ((double)valueInt == value)
            {
                Long(valueInt);
                return;
            }
            // Unfortunately double.ToString() will allocate an array, but there's no
            // method available in .NET Framework 4.5.x that encodes a floating-point number
            // into an existing character array.
            var valueStr = value.ToString();
            if (valueStr.Length <= _tempBytes.Length)
            {
                for (var i = 0; i < valueStr.Length; i++)
                {
                    _tempBytes[i] = (byte)valueStr[i]; // these are guaranteed to be ASCI chars
                }
                _buf.Write(_tempBytes, 0, valueStr.Length);
            }
            else
            {
                for (var i = 0; i < valueStr.Length; i++)
                {
                    _tempBytes[0] = (byte)valueStr[i];
                    _buf.Write(_tempBytes, 0, 1);
                }
            }
        }

        public void String(string value)
        {
            if (value == null)
            {
                Null();
                return;
            }
            if (value.Length == 0)
            {
                _buf.Write(_emptyStringBytes, 0, _emptyStringBytes.Length);
                return;
            }
            _buf.WriteByte((byte)'"');
            var encoding = Encoding.UTF8;
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch < 32 || ch == '"' || ch == '\\')
                {
                    WriteEscapedChar(ch);
                }
                else
                {
                    if (ch < 128)
                    {
                        _buf.WriteByte((byte)ch);
                    }
                    else
                    {
                        // C# stores strings as UTF-16. Each code point may use either 1 char value or 2.
                        _tempChars[0] = ch;
                        var numUtf16Chars = 1;
                        if (ch >= 0xd800 && ch <= 0xdbff)
                        {
                            // This range indicates that it's the first of 2 chars for a single code point
                            i++;
                            _tempChars[1] = value[i];
                            numUtf16Chars++;
                        }
                        var nBytes = encoding.GetBytes(_tempChars, 0, numUtf16Chars, _tempBytes, 0);
                        _buf.Write(_tempBytes, 0, nBytes);
                    }
                }
            }
            _buf.WriteByte((byte)'"');
        }

        private void WriteEscapedChar(char ch)
        {
            _buf.WriteByte((byte)'\\');
            switch (ch)
            {
                case '\b':
                    _buf.WriteByte((byte)'b');
                    break;
                case '\f':
                    _buf.WriteByte((byte)'f');
                    break;
                case '\n':
                    _buf.WriteByte((byte)'n');
                    break;
                case '\r':
                    _buf.WriteByte((byte)'r');
                    break;
                case '\t':
                    _buf.WriteByte((byte)'t');
                    break;
                case '"':
                    _buf.WriteByte((byte)'"');
                    break;
                case '\\':
                    _buf.WriteByte((byte)'\\');
                    break;
                default:
                    _tempBytes[0] = (byte)'u';
                    _tempBytes[1] = (byte)'0';
                    _tempBytes[2] = (byte)'0';
                    _tempBytes[3] = HexChar(ch >> 4);
                    _tempBytes[4] = HexChar(ch & 0xf);
                    _buf.Write(_tempBytes, 0, 5);
                    break;
            }
        }

        private static byte HexChar(int n)
        {
            return n > 9 ? (byte)('A' - 10 + n) : (byte)('0' + n);
        }

        public void StartArray()
        {
            _buf.WriteByte((byte)'[');
        }

        public void NextArrayItem()
        {
            _buf.WriteByte((byte)',');
        }

        public void EndArray()
        {
            _buf.WriteByte((byte)']');
        }

        public void StartObject()
        {
            _buf.WriteByte((byte)'{');
        }

        public void NextObjectItem(string name, bool first)
        {
            if (!first)
            {
                _buf.WriteByte((byte)',');
            }
            String(name);
            _buf.WriteByte((byte)':');
        }

        public void EndObject()
        {
            _buf.WriteByte((byte)'}');
        }
    }
}

#endif
