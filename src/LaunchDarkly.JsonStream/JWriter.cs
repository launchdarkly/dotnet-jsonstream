using LaunchDarkly.JsonStream.Implementation;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// A high-level API for writing JSON data sequentially.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On platforms that provide the <c>System.Text.Json</c> API (.NET Core 3.0+, .NET 5.0+), this
    /// works as a wrapper for <c>System.Text.Json.Utf8JsonWriter</c>, providing a more convenient API
    /// for common JSON writing operations. On platforms that do not have <c>System.Text.Json</c>, it
    /// falls back to a portable implementation that is not as fast as <c>Utf8JsonWriter</c> but still
    /// highly efficient and has no external dependencies. Portable JSON marshaling logic can therefore
    /// be written against this API without needing to know the target platform.
    /// </para>
    /// <para>
    /// The general usage pattern is as follows:
    /// </para>
    /// <list type="bullet">
    /// <item>There is one method for each JSON data type.</item>
    /// <item>For writing array or object structures, the <see cref="Array"/> and <see cref="Object"/>
    /// methods return a struct that keeps track of additional writer state while that structure is
    /// being written.</item>
    /// <item>If any method encounters an error, an exception is thrown and the caller should not
    /// attempt to use the <c>JWriter</c> any further after that point.</item>
    /// </list>
    /// </remarks>
    public sealed class JWriter : IValueWriter
    {
        internal readonly TokenWriter _tw;
        internal WriterState _state;

        /// <summary>
        /// Constructs a new instance with a default initial buffer size.
        /// </summary>
        /// <returns>a new <see cref="JWriter"/></returns>
        public static JWriter New() => NewWithInitialCapacity(100);

        /// <summary>
        /// Constructs a new instance with the specified initial buffer size.
        /// </summary>
        /// <param name="initialCapacity">the number of characters to preallocate</param>
        /// <returns>a new <see cref="JWriter"/></returns>
        public static JWriter NewWithInitialCapacity(int initialCapacity) =>
            new JWriter(initialCapacity);

        private JWriter(int initialCapacity)
        {
            _tw = new TokenWriter(initialCapacity);
        }

        /// <summary>
        /// Returns the output JSON data as a string.
        /// </summary>
        /// <remarks>
        /// If this is called after an error, or before all output has been written, the result is
        /// undefined.
        /// </remarks>
        /// <returns>a JSON string</returns>
        public string GetString() =>
            _tw.GetString();

        /// <summary>
        /// Returns the output JSON data as a UTF8-encoded byte array.
        /// </summary>
        /// <remarks>
        /// This is defined separately from <see cref="GetString"/> because on some platforms it is
        /// possible to implement this method more efficiently than <c>Encoding.UTF8.GetBytes(GetString())</c>.
        /// </remarks>
        /// <returns>a UTF8-encoded byte array</returns>
        public byte[] GetUTF8Bytes() =>
            _tw.GetUTF8Bytes();

        /// <inheritdoc/>
        public void Null()
        {
            BeforeValue();
            _tw.Null();
        }

        /// <inheritdoc/>
        public void Bool(bool value)
        {
            BeforeValue();
            _tw.Bool(value);
        }

        /// <inheritdoc/>
        public void BoolOrNull(bool? value)
        {
            if (value.HasValue)
            {
                Bool(value.Value);
            }
            else
            {
                Null();
            }
        }

        /// <inheritdoc/>
        public void Int(int value)
        {
            BeforeValue();
            _tw.Long(value);
        }

        /// <inheritdoc/>
        public void IntOrNull(int? value)
        {
            if (value.HasValue)
            {
                Int(value.Value);
            }
            else
            {
                Null();
            }
        }

        /// <inheritdoc/>
        public void Long(long value)
        {
            BeforeValue();
            _tw.Long(value);
        }

        /// <inheritdoc/>
        public void LongOrNull(long? value)
        {
            if (value.HasValue)
            {
                Long(value.Value);
            }
            else
            {
                Null();
            }
        }

        /// <inheritdoc/>
        public void Double(double value)
        {
            BeforeValue();
            _tw.Double(value);
        }

        /// <inheritdoc/>
        public void DoubleOrNull(double? value)
        {
            if (value.HasValue)
            {
                Double(value.Value);
            }
            else
            {
                Null();
            }
        }

        /// <inheritdoc/>
        public void String(string value)
        {
            BeforeValue();
            _tw.String(value);
        }

        /// <inheritdoc/>
        public ArrayWriter Array()
        {
            BeforeValue();
            _tw.StartArray();
            var previousState = _state;
            _state = new WriterState { InArray = true };
            return new ArrayWriter(this, previousState);
        }

        /// <inheritdoc/>
        public ObjectWriter Object()
        {
            BeforeValue();
            _tw.StartObject();
            var previousState = _state;
            _state = new WriterState { InArray = false };
            return new ObjectWriter(this, previousState);
        }

        private void BeforeValue()
        {
            if (_state.InArray)
            {
                if (_state.ArrayHasItems)
                {
                    _tw.NextArrayItem();
                }
                else
                {
                    _state.ArrayHasItems = true;
                }
            }
        }
    }
}
