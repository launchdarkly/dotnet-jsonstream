#if !USE_SYSTEM_TEXT_JSON

// This implementation of TokenWriter is used on platforms that do not have the
// System.Text.Json API.

using System.IO;
using System.Text;

namespace LaunchDarkly.JsonStream.Implementation
{
    internal partial class TokenWriter
    {
        private readonly TextWriter _w;
        private readonly char[] _tempChars = new char[30];

        public TokenWriter(int initialCapacity)
        {
            _w = new StringWriter(new StringBuilder(initialCapacity));
        }

        public string GetString() =>
            _w.ToString();

        public byte[] GetUTF8Bytes() =>
            Encoding.UTF8.GetBytes(_w.ToString());

        public Stream GetUTF8Stream() =>
            new MemoryStream(GetUTF8Bytes());
        
        public void Null()
        {
            _w.Write("null");
        }

        public void Bool(bool value)
        {
            _w.Write(value ? "true" : "false");
        }

        public void Long(long value)
        {
            if (value >= 0 && value < 10)
            {
                _w.Write((char)('0' + value));
                return;
            }
            // Avoid allocating a string, as the built-in int formatter would do
            var minus = value < 0;
            if (minus)
            {
                value = -value;
            }
            var pos = _tempChars.Length;
            while (value != 0)
            {
                var digit = value % 10;
                value /= 10;
                _tempChars[--pos] = (char)(digit + '0');
            }
            if (minus)
            {
                _tempChars[--pos] = '-';
            }
            _w.Write(_tempChars, pos, _tempChars.Length - pos);
        }

        public void Double(double value)
        {
            if (value == 0)
            {
                _w.Write('0');
                return;
            }
            _w.Write(value);
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
                _w.Write("\"\"");
                return;
            }
            _w.Write('"');
            var startPos = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch < 32 || ch == '"' || ch == '\\')
                {
                    if (i > startPos)
                    {
                        _w.Write(value.Substring(startPos, i - startPos)); // TODO
                    }
                    WriteEscapedChar(ch);
                    startPos = i + 1;
                }
            }
            if (startPos < value.Length)
            {
                _w.Write(value.Substring(startPos, value.Length - startPos)); // TODO
            }
            _w.Write('"');
        }

        private void WriteEscapedChar(char ch)
        {
            switch (ch)
            {
                case '\b':
                    _w.Write("\\b");
                    break;
                case '\f':
                    _w.Write("\\f");
                    break;
                case '\n':
                    _w.Write("\\r");
                    break;
                case '\r':
                    _w.Write("\\r");
                    break;
                case '\t':
                    _w.Write("\\t");
                    break;
                case '"':
                    _w.Write("\\\"");
                    break;
                case '\\':
                    _w.Write("\\\\");
                    break;
                default:
                    _w.Write("\\u00");
                    _w.Write(HexChar(ch >> 4));
                    _w.Write(HexChar(ch & 0xf));
                    break;
            }
        }

        private char HexChar(int n)
        {
            return n > 9 ? (char)('A' - 10 + n) : (char)('0' + n);
        }

        public void StartArray()
        {
            _w.Write('[');
        }

        public void NextArrayItem()
        {
            _w.Write(',');
        }

        public void EndArray()
        {
            _w.Write(']');
        }

        public void StartObject()
        {
            _w.Write('{');
        }

        public void NextObjectItem(string name, bool first)
        {
            if (!first)
            {
                _w.Write(',');
            }
            String(name);
            _w.Write(':');
        }

        public void EndObject()
        {
            _w.Write('}');
        }
    }
}

#endif
