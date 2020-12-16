using System;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Used by <see cref="JReader"/> to return a string value without allocating a string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>StringToken</c> represents a JSON string that was parsed from the input data by
    /// <see cref="JReader"/>. Under most circumstances, the <c>StringToken</c> will simply be a
    /// view into the original data, rather than a copy; it can be compared to other strings
    /// without causing any allocations. In other cases, <c>JReader</c> will need to allocate a
    /// string, but it will wrap the string in a <c>StringToken</c> so it looks the same to your
    /// code.
    /// </para>
    /// <para>
    /// This is basically equivalent to the <c>System.ReadOnlySpan</c> type that was introduced
    /// in .NET Core 2.1 and .NET 5.0, but can be used by LaunchDarkly libraries that support
    /// earler target frameworks.
    /// </para>
    /// <para>
    /// Although there are implicit operators that make <c>StringToken</c> transparently
    /// interoperable with <c>string</c> in most cases, there is one limitation: you cannot do a
    /// <c>switch</c> statement on a <c>StringToken</c>. Instead, you must either switch on its
    /// <c>ToString()</c> value, or else use a series of simple comparisons:
    /// </para>
    /// <code>
    ///     if (myStringToken == "a")
    ///     {
    ///         ...
    ///     }
    ///     else if (myStringToken == "b")
    ///     {
    ///         ...
    ///     }
    /// </code>
    /// <para>
    /// In most cases, unless the list of possible values is long, the latter will still be more
    /// efficient than allocating a stirng value.
    /// </para>
    /// </remarks>
    public struct StringToken : IEquatable<StringToken>, IEquatable<string>
    {
        private readonly string _str;
        private readonly char[] _array;
        private readonly int _offset, _length;

        /// <summary>
        /// A <c>StringToken</c> representing an empty string.
        /// </summary>
        public static readonly StringToken Empty = new StringToken(null, null, 0, 0);

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

#if NET5_0 || NETSTANDARD2_1
        /// <summary>
        /// Converts the <c>StringToken</c> to a <c>ReadOnlySpan</c>.
        /// </summary>
        /// <returns>an equivalent <c>ReadOnlySpan</c></returns>
        public ReadOnlySpan<char> AsSpan() =>
            _str is null ? new ReadOnlySpan<char>(_array, _offset, _length) :
            _str.AsSpan();
#endif

#pragma warning disable CS1591  // don't need XML comments for these standard methods
        public static bool operator ==(StringToken st, string s) => st.Equals(s);

        public static bool operator ==(StringToken st, StringToken st1) => st.Equals(st1);

        public static bool operator !=(StringToken st, string s) => !st.Equals(s);

        public static bool operator !=(StringToken st, StringToken st1) => !st.Equals(st1);

        public static implicit operator StringToken(string s) => StringToken.FromString(s);
#pragma warning restore CS1591

        /// <summary>
        /// Compares this value to an object that can be either a <c>string</c> or a <c>StringToken</c>.
        /// </summary>
        /// <param name="obj">the value to compare</param>
        /// <returns>true if the values are logically equal</returns>
        public override bool Equals(object obj) =>
            (obj is string s && Equals(s)) ||
            (obj is StringToken st && Equals(st));
        
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
        /// <param name="other">the value to compare</param>
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
#if NET5_0 || NETSTANDARD2_1
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
        /// In .NET Core 2.1+ and .NET 5.0+, this is implemented efficiently with
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
#if NET5_0 || NETSTANDARD2_1
            return double.Parse(AsSpan());
#else
            return double.Parse(ToString());
#endif
        }
    }
}
