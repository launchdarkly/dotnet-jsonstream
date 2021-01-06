using System;
using LaunchDarkly.JsonStream.Implementation;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// A high-level API for reading JSON data sequentially.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On platforms that provide the <c>System.Text.Json</c> API (.NET Core 3.1+, .NET 5.0+), this
    /// works as a wrapper for <c>System.Text.Json.Utf8JsonReader</c>, providing a more convenient API
    /// for common JSON parsing operations. On platforms that do not have <c>System.Text.Json</c>, it
    /// falls back to a portable implementation that is not as fast as <c>Utf8JsonReader</c> but still
    /// highly efficient and has no external dependencies. Portable JSON unmarshaling logic can
    /// therefore be written against this API without needing to know the target platform.
    /// </para>
    /// <para>
    /// The general usage pattern is as follows:
    /// </para>
    /// <list type="bullet">
    /// <item>Values are parsed in the order that they appear.</item>
    /// <item>In general, the caller should know what data type is expected. Since it is common for
    /// properties to be nullable, the methods for reading scalar types have variants for allowing
    /// a null instead of the specified type. If the type is completely unknown, use
    /// <see cref="Any"/>.</item>
    /// <item>For reading array or object structures, the <see cref="Array"/> and <see cref="Object"/>
    /// methods return a struct that keeps track of additional reader state while that structure is
    /// being parsed.</item>
    /// <item>If any method encounters an error (due to either malformed JSON, or well-formed JSON that
    /// did not match the caller's data type expectations), an exception derived from
    /// <see cref="JsonReadException"/> is thrown. The caller should not attempt to use the
    /// <c>JReader</c> any further after that point.</item>
    /// </list>
    /// <para>
    /// <c>JReader</c> is defined as a <c>ref struct</c>, which places certain limitations on how it
    /// can be used in .NET code. This is done for two reasons. First, this is necessary in order for
    /// it to be able to take advantage of <c>System.Text.Json</c> on compatible platforms. Second, it
    /// allows the compiler to make significant optimizations regardless of the platform, since it
    /// guarantees that this object will never be allocated on the heap. An indirect result of this
    /// design is that methods like <see cref="ArrayReader.Next(ref JReader)"/> and
    /// <see cref="ObjectReader.Next(ref JReader)"/> require the original <c>JReader</c> to be passed
    /// as a <c>ref</c> parameter, because they are not allowed to retain a reference to it in a field
    /// of their own.
    /// </para>
    /// <para>
    /// Another design decision based on <c>System.Text.Json</c> interoperability and efficiency is
    /// that <c>JReader</c> methods can throw either their own exception types (derived from
    /// <see cref="JsonReadException"/>), or, on some platforms, a <c>System.Text.Json.JsonException</c>.
    /// <c>JReader</c> does not automatically catch the latter and translate them to its own types, in
    /// order to avoid the overhead of <c>try</c> blocks within high-traffic code paths. If you need to
    /// be able to distinguish between different kinds of JSON errors, you can call
    /// <see cref="JReader.TranslateException(Exception)"/> after catching an exception.
    /// </para>
    /// </remarks>
    public ref struct JReader
    {
        private readonly IReaderAdapter _delegate;
        private TokenReader _tr;
        private bool _awaitingReadValue;

        /// <summary>
        /// Creates a <see cref="JReader"/> that processes JSON data from a stirng.
        /// </summary>
        /// <param name="input">the input string</param>
        /// <returns>a new <see cref="JReader"/></returns>
        public static JReader FromString(string input) =>
            new JReader(new TokenReader(input));

        /// <summary>
        /// Creates a <see cref="JReader"/> that processes JSON data from a UTF-8 byte array.
        /// </summary>
        /// <param name="input">the input data</param>
        /// <returns>a new <see cref="JReader"/></returns>
        public static JReader FromUtf8Bytes(byte[] input) =>
            new JReader(new TokenReader(input));

        /// <summary>
        /// Creates a <see cref="JReader"/> that processes JSON-equivalent data from a custom
        /// source.
        /// </summary>
        /// <remarks>
        /// This allows deserialization logic based on <see cref="JReader"/> to be used on a custom input
        /// stream without involving the actual JSON parser. For instance, you could define an
        /// implementation of <see cref="IReaderDelegate"/> that consumes YAML data.
        /// </remarks>
        /// <param name="dataAdapter">an implementation of <see cref="IReaderAdapter"/></param>
        /// <returns>a new <see cref="JReader"/></returns>
        /// <seealso cref="IReaderDelegate"/>
        public static JReader FromAdapter(IReaderAdapter dataAdapter) =>
            new JReader(dataAdapter);

        private JReader(TokenReader tokenReader)
        {
            _tr = tokenReader;
            _awaitingReadValue = false;
            _delegate = null;
        }

        private JReader(IReaderAdapter d)
        {
            _tr = new TokenReader();
            _awaitingReadValue = false;
            _delegate = d;
        }

        internal int LastPos => _tr.LastPos;

        /// <summary>
        /// True if all of the input has been consumed (not counting whitespace).
        /// </summary>
        public bool EOF => _delegate == null ? _tr.EOF : _delegate.EOF;

        /// <summary>
        /// Attempts to read a null value.
        /// </summary>
        /// <exception cref="TypeException">if the next token is not a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public void Null()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                _delegate.NextValue(ValueType.Null, true);
                return;
            }
            if (!_tr.Null())
            {
                var pos = _tr.LastPos;
                var nextVal = _tr.Any();
                throw new TypeException(ValueType.Null, nextVal.Type, pos);
            }
        }

        /// <summary>
        /// Attempts to read a boolean value.
        /// </summary>
        /// <returns>the boolean value</returns>
        /// <exception cref="TypeException">if the next token is not a boolean</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public bool Bool()
        {
            _awaitingReadValue = false;
            return _delegate == null ? _tr.Bool() : _delegate.NextValue(ValueType.Bool, false).BoolValue;
        }

        /// <summary>
        /// Attempts to read either a boolean value or a null.
        /// </summary>
        /// <returns>the boolean value or null</returns>
        /// <exception cref="TypeException">if the next token is neither a boolean nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public bool? BoolOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var val = _delegate.NextValue(ValueType.Bool, true);
                return val.Type == ValueType.Null ? null : (bool?)val.BoolValue;
            }
            return _tr.Null() ? null : (bool?)_tr.Bool();
        }

        /// <summary>
        /// Attempts to read a numeric value as an <see langword="int"/>.
        /// </summary>
        /// <returns>the numeric value, truncated to an integer</returns>
        /// <exception cref="TypeException">if the next token is not a number</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public int Int()
        {
            _awaitingReadValue = false;
            return _delegate == null ? (int)_tr.Number() : (int)_delegate.NextValue(ValueType.Number, false).NumberValue;
        }

        /// <summary>
        /// Attempts to read either an integer value or a null.
        /// </summary>
        /// <returns>the numeric value, truncated to an integer, or null</returns>
        /// <exception cref="TypeException">if the next token is neither a number nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public int? IntOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var val = _delegate.NextValue(ValueType.Number, true);
                return val.Type == ValueType.Null ? null : (int?)val.NumberValue;
            }
            return _tr.Null() ? null : (int?)_tr.Number();
        }

        /// <summary>
        /// Attempts to read a numeric value as an <see langword="long"/>.
        /// </summary>
        /// <returns>the numeric value, truncated to an integer</returns>
        /// <exception cref="TypeException">if the next token is not a number</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public long Long()
        {
            _awaitingReadValue = false;
            return _delegate == null ? (long)_tr.Number() : (long)_delegate.NextValue(ValueType.Number, false).NumberValue;
        }

        /// <summary>
        /// Attempts to read either a long integer value or a null.
        /// </summary>
        /// <returns>the numeric value, truncated to an integer, or null</returns>
        /// <exception cref="TypeException">if the next token is neither a number nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public long? LongOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var val = _delegate.NextValue(ValueType.Number, true);
                return val.Type == ValueType.Null ? null : (long?)val.NumberValue;
            }
            return _tr.Null() ? null : (long?)_tr.Number();
        }

        /// <summary>
        /// Attempts to read a numeric value as a <see langword="double"/>.
        /// </summary>
        /// <returns>the numeric value</returns>
        /// <exception cref="TypeException">if the next token is not a number</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public double Double()
        {
            _awaitingReadValue = false;
            return _delegate == null ? _tr.Number() : _delegate.NextValue(ValueType.Number, false).NumberValue;
        }

        /// <summary>
        /// Attempts to read either a numeric value or a null.
        /// </summary>
        /// <returns>the numeric value or null</returns>
        /// <exception cref="TypeException">if the next token is neither a number nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public double? DoubleOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var val = _delegate.NextValue(ValueType.Number, true);
                return val.Type == ValueType.Null ? null : (double?)val.NumberValue;
            }
            return _tr.Null() ? null : (double?)_tr.Number();
        }

        /// <summary>
        /// Attempts to read a non-null string value.
        /// </summary>
        /// <returns>the string value</returns>
        /// <exception cref="TypeException">if the next token is not a string</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public string String()
        {
            _awaitingReadValue = false;
            return _delegate == null ? _tr.String() : _delegate.NextValue(ValueType.String, false).StringValue;
        }

        /// <summary>
        /// Attempts to read either a string value or a null.
        /// </summary>
        /// <returns>a <c>string</c> which may be null</returns>
        /// <exception cref="TypeException">if the next token is neither a string nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public string StringOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                return _delegate.NextValue(ValueType.String, true).StringValue;
            }
            return _tr.Null() ? null : _tr.String();
        }

        /// <summary>
        /// Attempts to begin reading a JSON array value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If successful, the return value will be an <see cref="ArrayReader"/> containing the
        /// necessary state for iterating through the array elements.
        /// </para>
        /// <para>
        /// The <see cref="ArrayReader"/>is used only for the iteration state; to read the value of
        /// each array element, you will still use the <c>JReader</c>'s methods.
        /// </para>
        /// </remarks>
        /// <returns>an <see cref="ArrayReader"/></returns>
        /// <exception cref="TypeException">if the next token is not the beginning of an array</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public ArrayReader Array()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                _delegate.NextValue(ValueType.Array, false);
                return new ArrayReader(true);
            }
            _tr.StartArray();
            return new ArrayReader(true);
        }

        /// <summary>
        /// Attempts to either begin reading a JSON array value or read a null.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the case of an array, the return value will be an <see cref="ArrayReader"/> containing the
        /// necessary state for iterating through the array elements. In the case of a null, the returned
        /// <see cref="ArrayReader"/> will be a stub that always returns <see langword="false"/> for both
        /// <see cref="ArrayReader.Next"/> and <see cref="ArrayReader.IsDefined"/>.
        /// </para>
        /// <para>
        /// The <see cref="ArrayReader"/>is used only for the iteration state; to read the value of
        /// each array element, you will still use the <c>JReader</c>'s methods.
        /// </para>
        /// </remarks>
        /// <returns>an <see cref="ArrayReader"/></returns>
        /// <exception cref="TypeException">if the next token is neither the beginning of an array nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public ArrayReader ArrayOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var value = _delegate.NextValue(ValueType.Array, true);
                return new ArrayReader(value.Type != ValueType.Null);
            }
            if (_tr.Null())
            {
                return new ArrayReader(false);
            }
            _tr.StartArray();
            return new ArrayReader(true);
        }
        
        /// <summary>
        /// Attempts to begin reading a JSON object value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If successful, the return value will be an <see cref="ObjectReader"/> containing the
        /// necessary state for iterating through the object properties.
        /// </para>
        /// <para>
        /// The <see cref="ObjectReader"/>is used only for the iteration state; to read the value of
        /// each property, you will still use the <c>JReader</c>'s methods.
        /// </para>
        /// </remarks>
        /// <returns>an <see cref="ObjectReader"/></returns>
        /// <exception cref="TypeException">if the next token is not the beginning of an object</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public ObjectReader Object()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                _delegate.NextValue(ValueType.Object, false);
                return new ObjectReader(true, null);
            }
            _tr.StartObject();
            return new ObjectReader(true, null);
        }

        /// <summary>
        /// Attempts to either begin reading a JSON object value or read a null.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the case of an object, the return value will be an <see cref="ObjectReader"/> containing the
        /// necessary state for iterating through the object properties. In the case of a null, the returned
        /// <see cref="ObjectReader"/> will be a stub that always returns <see langword="false"/> for both
        /// <see cref="ObjectReader.Next"/> and <see cref="ArrayReader.IsDefined"/>.
        /// </para>
        /// <para>
        /// The <see cref="ObjectReader"/>is used only for the iteration state; to read the value of
        /// each property, you will still use the <c>JReader</c>'s methods.
        /// </para>
        /// </remarks>
        /// <returns>an <see cref="ObjectReader"/></returns>
        /// <exception cref="TypeException">if the next token is neither the beginning of an object nor a null</exception>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public ObjectReader ObjectOrNull()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                var value = _delegate.NextValue(ValueType.Object, true);
                return new ObjectReader(value.Type != ValueType.Null, null);
            }
            if (_tr.Null())
            {
                return new ObjectReader(false, null);
            }
            _tr.StartObject();
            return new ObjectReader(true, null);
        }

        /// <summary>
        /// Reads a single value of any type, if it is a scalar value or a null, or prepares to read
        /// the value if it is an array or object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned <see cref="AnyValue"/>'s <see cref="AnyValue.Type"/> property indicates the
        /// value type. If it is <see cref="ValueType.Bool"/>, <see cref="ValueType.Number"/>, or
        /// <see cref="ValueType.String"/>, check the corresponding <see cref="AnyValue.BoolValue"/>,
        /// <see cref="AnyValue.NumberValue"/>, or <see cref="AnyValue.StringValue"/> property. If it is
        /// <see cref="ValueType.Array"/> or <see cref="ValueType.Object"/>, the <c>AnyValue</c>'s
        /// <see cref="AnyValue.ArrayValue"/> or <see cref="AnyValue.ObjectValue"/> property has been
        /// initialized with an <see cref="ArrayReader"/> or <see cref="ObjectReader"/> just as if you
        /// had called the <c>JReader</c>'s <see cref="Array"/> or <see cref="Object"/> method.
        /// </para>
        /// </remarks>
        /// <returns>a scalar value or a marker for the beginning of a complex value</returns>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public AnyValue Any()
        {
            _awaitingReadValue = false;
            if (_delegate != null)
            {
                return _delegate.NextValue(null, true);
            }
            var token = _tr.Any();
            switch (token.Type)
            {
                case ValueType.Bool:
                    return AnyValue.Bool(token.BoolValue);
                case ValueType.Number:
                    return AnyValue.Number(token.NumberValue);
                case ValueType.String:
                    return AnyValue.String(token.StringValue);
                case ValueType.Array:
                    return AnyValue.Array(new ArrayReader(true));
                case ValueType.Object:
                    return AnyValue.Object(new ObjectReader(true, null));
                default:
                    return AnyValue.Null();
            }
        }

        /// <summary>
        /// Consumes and discards the next JSON value of any type.
        /// </summary>
        /// <remarks>
        /// For an array or object value, it recurses to also consume and discard all array elements or
        /// object properties.
        /// </remarks>
        /// <exception cref="SyntaxException">if there is a JSON parsing error</exception>
        public void SkipValue()
        {
            _awaitingReadValue = false;
            var av = Any();
            switch (av.Type)
            {
                case ValueType.Array:
                    var arr = av.ArrayValue;
                    while (arr.Next(ref this)) { }
                    break;
                case ValueType.Object:
                    var obj = av.ObjectValue;
                    while (obj.Next(ref this)) { }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Normalizes exceptions that may have been thrown from low-level code.
        /// </summary>
        /// <remarks>
        /// When using a target framework that has <c>System.Text.Json</c>, some low-level JSON parsing
        /// logic may throw a <c>System.Text.Json</c> exception type rather than the <see cref="JsonReadException"/>
        /// types used by <c>JReader</c>. To avoid the overhead of using <c>try</c>/<c>catch</c> in
        /// high-traffic code paths, <c>JReader</c> does not translate these exceptions on a per-method-call
        /// basis. Instead, if you want to be able to distinguish between different kinds of errors in a
        /// consistent way, you should catch <c>Exception</c> in general and then pass the exception to
        /// this method.
        /// </remarks>
        /// <param name="e">the exception that was thrown</param>
        /// <returns>a <see cref="SyntaxException"/> or <see cref="TypeException"/> if the original
        /// exception was a different type that can be translated to those types; otherwise, the
        /// original exception</returns>
        public Exception TranslateException(Exception e)
        {
            return _tr.TranslateException(e);
        }

        internal bool ArrayNext(bool first)
        {
            if (!first && _awaitingReadValue)
            {
                SkipValue();
            }
            if (_delegate == null ? _tr.ArrayNext(first) : _delegate.ArrayNext(first))
            {
                _awaitingReadValue = true;
                return true;
            }
            return false;
        }

        internal PropertyNameToken ObjectNext(bool first)
        {
            if (!first && _awaitingReadValue)
            {
                SkipValue();
            }
            var ret = _delegate == null ? _tr.ObjectNext(first) : _delegate.ObjectNext(first);
            _awaitingReadValue = !ret.Empty;
            return ret;
        }
    }
}
