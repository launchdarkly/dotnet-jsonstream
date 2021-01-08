using System;

namespace LaunchDarkly.JsonStream.Implementation
{
    /// <summary>
    /// Used internally by the parser to hold a string value in situations where it may or may
    /// not be desirable to allocate a <c>string</c> instance.
    /// </summary>
    internal ref struct StringToken
    {
        private readonly string _str;
        private readonly char[] _array;
        private readonly int _offset, _length;

        /// <summary>
        /// Creates a <c>StringToken</c> that wraps an existing string.
        /// </summary>
        /// <param name="s">a string</param>
        /// <returns>a <c>StringToken</c> referencing this string</returns>
        public static StringToken FromString(string s) => new StringToken(s, null, 0, s.Length);

        /// <summary>
        /// Creates a <c>StringToken</c> that points to existing characters in an array.
        /// </summary>
        /// <param name="array">a character array</param>
        /// <param name="offset">index of the starting character</param>
        /// <param name="length">number of characters to use</param>
        /// <returns>a <c>StringToken</c> referencing this range</returns>
        public static StringToken FromChars(char[] array, int offset, int length) =>
            new StringToken(null, array, offset, length);

        private StringToken(string str, char[] array, int offset, int length)
        {
            _str = str;
            _array = array;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// The length of the string.
        /// </summary>
        public int Length => _length;

#if USE_SYSTEM_TEXT_JSON
        /// <summary>
        /// Converts the <c>StringToken</c> to a <c>ReadOnlySpan</c>.
        /// </summary>
        /// <returns>an equivalent <c>ReadOnlySpan</c></returns>
        public ReadOnlySpan<char> AsSpan() =>
            _str is null ? new ReadOnlySpan<char>(_array, _offset, _length) :
            _str.AsSpan();
#endif

        /// <summary>
        /// Compares this value to another <c>StringToken</c>.
        /// </summary>
        /// <remarks>
        /// The values are equal if their contents are equal, regardless of whether the tokens were
        /// created with <see cref="FromString"/> or <see cref="FromChars(char[], int, int)"/>.
        /// </remarks>
        /// <param name="other">the value to compare</param>
        /// <returns>true if the values are logically equal</returns>
        public bool Equals(StringToken other)
        {
            var len = _length;
            if (len != other._length)
            {
                return false;
            }
            if (_str == null)
            {
                if (other._str is null)
                {
                    var offset = _offset;
                    var otherCh = other._array;
                    var otherOffset = other._offset;
                    for (int i = 0; i < len; i++)
                    {
                        if (otherCh[otherOffset + i] != _array[offset + i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return Equals(other._str);
            }
            else
            {
                return other._str is null ? other.Equals(_str) : other._str.Equals(_str);
            }    
        }

        /// <summary>
        /// Compares this value to a <c>string</c>.
        /// </summary>
        /// <remarks>
        /// The values are equal if their contents are equal, regardless of whether the token was
        /// created with <see cref="FromString"/> or <see cref="FromChars(char[], int, int)"/>.
        /// </remarks>
        /// <param name="value">the value to compare</param>
        /// <returns>true if the values are logically equal</returns>
        public bool Equals(string value)
        {
            if (!(_str is null))
            {
                return _str.Equals(value);
            }
            var len = _length;
            if (value == null || len != value.Length)
            {
                return false;
            }
            var offset = _offset;
            for (int i = 0; i < len; i++)
            {
                if (value[i] != _array[offset + i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converts this value to a string.
        /// </summary>
        /// <remarks>
        /// If the <c>StringToken</c> was created with <see cref="FromChars(char[], int, int)"/>,
        /// this causes a new <c>string</c> to be allocated; if it was created with
        /// <see cref="FromString"/>, it returns the original string.
        /// </remarks>
        /// <returns>the string value</returns>
        public override string ToString()
        {
            if (_str != null)
            {
                return _str;
            }
            if (_length == 0)
            {
                return "";
            }
            return new String(_array, _offset, _length);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (_str != null)
            {
                return _str.GetHashCode();
            }
            var ret = 0;
            for (var i = 0; i < _length; i++)
            {
                ret = ret * 23 + _array[i];
            }
            return ret;
        }

        /// <summary>
        /// Attempts to parse the string token as an integer.
        /// </summary>
        /// <returns>the parsed numeric value</returns>
        /// <exception cref="FormatException">if it is not a valid integer</exception>
        public long ParseLong()
        {
            if (_str != null)
            {
                return long.Parse(_str);
            }
#if NET5_0 || NETCOREAPP3_1
            return long.Parse(AsSpan());
#else
            var minus = false;
            long n = 0;
            var pos = _offset;
            var end = _offset + _length;
            if (pos < end && _array[pos] == '-')
            {
                minus = true;
                pos++;
            }
            if (pos == end)
            {
                throw new FormatException();
            }
            while (pos < end)
            {
                var ch = _array[pos++];
                if (ch < '0' || ch > '9')
                {
                    throw new FormatException();
                }
                n = (n * 10) + (ch - '0');
            }
            return minus ? -n : n;
#endif
        }

        /// <summary>
        /// Attempts to parse the string token as a number.
        /// </summary>
        /// <remarks>
        /// In .NET Core 3.1+ and .NET 5.0+, this is implemented efficiently with
        /// <c>double.Parse(ReadOnlySpan&lt;char&gt;)</c>. Otherwise it requires converting
        /// the <c>StringToken</c> to a <c>string</c> internally.
        /// </remarks>
        /// <returns>the parsed numeric value</returns>
        /// <exception cref="FormatException">if it is not a valid integer</exception>
        public double ParseDouble()
        {
            if (_str != null)
            {
                return double.Parse(_str);
            }
#if NET5_0 || NETCOREAPP3_1
            return double.Parse(AsSpan());
#else
            return double.Parse(ToString());
#endif
        }
    }
}
