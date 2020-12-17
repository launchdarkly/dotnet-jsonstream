using System;

#if USE_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace LaunchDarkly.JsonStream.Implementation
{
    internal ref struct PropertyNameToken
    {
        private readonly bool _defined;

        public bool IsDefined => _defined;

#if USE_SYSTEM_TEXT_JSON
        private readonly JsonProperty? _jsonProperty;
        private readonly ReadOnlySpan<byte> _asciiBytes;
        private readonly string _string;

        internal PropertyNameToken(string fromString)
        {
            _string = fromString;
            _jsonProperty = null;
            _asciiBytes = null;
            _defined = true;
        }

        internal PropertyNameToken(JsonProperty fromJsonProperty)
        {
            _jsonProperty = fromJsonProperty;
            _string = null;
            _asciiBytes = null;
            _defined = true;
        }

        internal PropertyNameToken(ReadOnlySpan<byte> fromAsciiBytes)
        {
            _asciiBytes = fromAsciiBytes;
            _jsonProperty = null;
            _string = null;
            _defined = true;
        }

        public override string ToString()
        {
            if (!_asciiBytes.IsEmpty)
            {
#if NETCOREAPP3_1 || NET5_0
                // On these platforms, we can create a string more efficiently without allocating an intermediate array
                return System.Text.Encoding.ASCII.GetString(_asciiBytes);
#else
                return System.Text.Encoding.ASCII.GetString(_asciiBytes.ToArray());
#endif
            }
            return _jsonProperty.HasValue ? _jsonProperty.Value.Name : _string;
        }

        public bool Equals(string value)
        {
            if (!_asciiBytes.IsEmpty)
            {
                var len = _asciiBytes.Length;
                if (value.Length != len)
                {
                    return false;
                }
                for (var i = 0; i < len; i++)
                {
                    if (value[i] != _asciiBytes[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return _jsonProperty.HasValue? _jsonProperty.Value.NameEquals(value) : value.Equals(_string);
        }
#else
        private readonly StringToken _stringToken;

        internal PropertyNameToken(StringToken fromStringToken)
        {
            _stringToken = fromStringToken;
            _defined = true;
        }
        
        public override string ToString() => _stringToken.ToString();

        public bool Equals(string value) => _stringToken.Equals(value);
#endif
    }
}
