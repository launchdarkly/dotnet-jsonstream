using System;

#if USE_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

using LaunchDarkly.JsonStream.Implementation;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Used by <see cref="ObjectReader"/> to return an object property name in situations where it
    /// may not be desirable to allocate a <c>string</c> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type is used for the <see cref="ObjectReader.Name"/> property of <see cref="ObjectReader"/>,
    /// to allow for a slight efficiency gain in some circumstances by not always having to create
    /// a <c>string</c> instance. The rationale is that JSON property names are often used only
    /// briefly for comparisons during parsing, and if they do not need to be retained in the parsed
    /// data, it is preferable to avoid creating many <c>string</c> instances that will increase
    /// garbage collection overhead.
    /// </para>
    /// <para>
    /// Whenever possible, the parser will set the <see cref="PropertyNameToken"/> to work as a direct
    /// view onto the underlying character sequence in the JSON input, in which case using
    /// <see cref="Equals(string)"/> or <c>==</c> will not cause any allocations. In some cases
    /// (such as if the property name contains string escape sequences) that is not possible, and the
    /// parser will have to allocate a <c>string</c>, but will wrap it in a <c>PropertyNameToken</c>
    /// so your code can treat it the same either way.
    /// </para>
    /// <para>
    /// The down side is that it is not possible to use a <c>PropertyNameToken</c> as the value in an
    /// ordinary <c>switch</c> statement; if you want to use <c>switch</c>, you must either call
    /// <see cref="ToString"/> to get the name as a string, or use <c>when</c> clauses in the <c>switch</c>
    /// as shown in the example. But even though <c>switch</c> can be slightly faster than a series of
    /// equality tests, if the number of comparisons is small that advantage may not matter.
    /// </para>
    /// <example>
    /// <code>
    ///     string prop1Value, prop2Value;
    ///     for (var obj = reader.Object(); obj.Next();)
    ///     {
    ///         switch (obj.Name)
    ///         {
    ///             case var n when n == "prop1":
    ///                 prop1Value = obj.String();
    ///                 break;
    ///             case var n when n == "prop2":
    ///                 prop2Value = obj.String();
    ///                 break;
    ///         }
    ///     }
    /// </code>
    /// </example>
    /// </remarks>
    public ref struct PropertyNameToken
    {
        private readonly bool _defined;

        /// <summary>
        /// An empty value that indicates there are no more properties.
        /// </summary>
        public static PropertyNameToken None => new PropertyNameToken();

        /// <summary>
        /// True if this is <see cref="PropertyNameToken.None"/>.
        /// </summary>
        public bool Empty => !_defined;

#if USE_SYSTEM_TEXT_JSON
        private readonly string _string;
        private readonly ReadOnlySpan<byte> _asciiBytes;
        private readonly JsonProperty? _jsonProperty;

        internal PropertyNameToken(string fromString)
        {
            _string = fromString;
            _jsonProperty = null;
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

        // This constructor is used when we're scanning already-parsed JSON nodes - see
        // JsonStreamConverterSystemTextJson.cs
        internal PropertyNameToken(JsonProperty fromJsonProperty)
        {
            _jsonProperty = fromJsonProperty;
            _string = null;
            _asciiBytes = null;
            _defined = true;
        }

        /// <summary>
        /// Converts the <c>PropertyNameToken</c> to a <c>string</c>.
        /// </summary>
        /// <para>
        /// If the parser already allocated a <c>string</c> for this name internally, it returns the
        /// same <c>string</c>. Otherwise it allocates a new one.
        /// </para>
        /// <returns>the name as a string</returns>
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

        /// <summary>
        /// Compares the <c>PropertyNameToken</c> to another <c>PropertyNameToken</c>.
        /// </summary>
        /// <param name="other">another name</param>
        /// <returns>true if the names are equal</returns>
        public bool Equals(PropertyNameToken other)
        {
            var otherString = other._string;
            if (_string != null)
            {
                return otherString == null ? other.Equals(_string) : _string.Equals(otherString);
            }
            var otherBytes = other._asciiBytes;
            var otherProp = other._jsonProperty;
            if (_jsonProperty.HasValue)
            {
                var jp = _jsonProperty.Value;
                if (otherString != null)
                {
                    return jp.NameEquals(otherString);
                }
                if (otherProp.HasValue)
                {
                    return jp.NameEquals(other._jsonProperty.Value.Name); // inefficient, requires string allocation
                }
                return jp.NameEquals(otherBytes);

            }

            // We're using this._asciiBytes
            if (otherString != null)
            {
                return Equals(otherString);
            }
            if (otherProp.HasValue)
            {
                return otherProp.Value.NameEquals(_asciiBytes);
            }
            var len = _asciiBytes.Length;
            if (otherBytes.Length != len)
            {
                return false;
            }
            for (var i = 0; i < len; i++)
            {
                if (otherBytes[i] != _asciiBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares the <c>PropertyNameToken</c> to a string value.
        /// </summary>
        /// <param name="value">a string</param>
        /// <returns>true if the string is an exact match</returns>
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
            return _jsonProperty.HasValue ? _jsonProperty.Value.NameEquals(value) : value.Equals(_string);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => ToString().GetHashCode();
#else
        private readonly StringToken _stringToken;

        internal PropertyNameToken(string fromString) : this(StringToken.FromString(fromString)) { }

        internal PropertyNameToken(StringToken fromStringToken)
        {
            _stringToken = fromStringToken;
            _defined = true;
        }

        /// <summary>
        /// Converts the <c>PropertyNameToken</c> to a <c>string</c>.
        /// </summary>
        /// <para>
        /// If the parser already allocated a <c>string</c> for this name internally, it returns the
        /// same <c>string</c>. Otherwise it allocates a new one.
        /// </para>
        /// <returns>the name as a string</returns>
        public override string ToString() => _stringToken.ToString();

        /// <summary>
        /// Compares the <c>PropertyNameToken</c> to another <c>PropertyNameToken</c>.
        /// </summary>
        /// <param name="other">another name</param>
        /// <returns>true if the names are equal</returns>
        public bool Equals(PropertyNameToken other) => _stringToken.Equals(other._stringToken);

        /// <summary>
        /// Compares the <c>PropertyNameToken</c> to a string value.
        /// </summary>
        /// <param name="value">a string</param>
        /// <returns>true if the string is an exact match</returns>
        public bool Equals(string value) => _stringToken.Equals(value);

        /// <inheritdoc/>
        public override int GetHashCode() =>_stringToken.GetHashCode();
#endif

        /// <inheritdoc/>
        public override bool Equals(object o) => o is string s && Equals(s);

#pragma warning disable CS1591 // don't need XML comments for these standard operators
        public static bool operator ==(PropertyNameToken p1, PropertyNameToken p2) => p1.Equals(p2);

        public static bool operator !=(PropertyNameToken p1, PropertyNameToken p2) => p1.Equals(p2);

        public static bool operator ==(PropertyNameToken p, string value) => p.Equals(value);

        public static bool operator !=(PropertyNameToken p, string value) => !p.Equals(value);
#pragma warning restore CS1591
    }
}
