using System.Collections.Generic;
using System.Linq;

#if USE_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Built-in implementations of <see cref="IReaderAdapter"/>.
    /// </summary>
    public static class ReaderAdapters
    {
        /// <summary>
        /// Creates an adapter that treats basic .NET data types such as <c>bool</c> and <c>List</c>
        /// as if they were the corresponding JSON types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This adapter uses the same rules as <see cref="JsonStreamConvert.ConvertSimpleTypes"/>.
        /// </para>
        /// <para>
        /// Optionally, it can also perform basic type coercion between strings, booleans, and numbers,
        /// depending on what data type is being requested by the deserialization logic; this may be
        /// useful when reading data that was parsed by a YAML parser, where the intended type of a
        /// value may be ambiguous. In this mode, the strings "true" and "false" (or "on" and "off",
        /// since those can be used as synonyms in YAML) can be converted to booleans, and boolean or
        /// number values can be converted to strings.
        /// </para>
        /// </remarks>
        /// <param name="value">a value that corresponds to a JSON value</param>
        /// <param name="allowTypeCoercion">true if basic types can be interconverted</param>
        /// <returns>an adapter for use with <see cref="JReader.FromAdapter(IReaderAdapter)"/></returns>
        public static IReaderAdapter FromSimpleTypes(object value, bool allowTypeCoercion = false) =>
            new SimpleTypesAdapter(value, allowTypeCoercion);

#if USE_SYSTEM_TEXT_JSON
        /// <summary>
        /// Creates an adapter that reads from a <c>System.Text.Json.JsonElement</c>.
        /// </summary>
        /// <remarks>
        /// This method is only available on platforms that support the <c>System.Text.Json</c> API.
        /// </remarks>
        /// <param name="value">a parsed element</param>
        /// <returns>an adapter for use with <see cref="JReader.FromAdapter(IReaderAdapter)"/></returns>
        public static IReaderAdapter FromJsonElement(JsonElement value) =>
            new JsonElementReaderAdapter(value);
#endif

        private sealed class SimpleTypesAdapter : ReaderAdapterBase<object, SimpleTypesAdapter.ParentContext>
        {
            private bool _allowTypeCoercion;

            public struct ParentContext
            {
                internal IEnumerator<object> ArrayEnumerator;
                internal IEnumerator<KeyValuePair<string, object>> ObjectEnumerator;
            }

            internal SimpleTypesAdapter(object value, bool allowTypeCoercion) : base(value)
            {
                _allowTypeCoercion = allowTypeCoercion;
            }

            protected override AnyValue ConsumeNext(ValueType? typeHint, ref object current)
            {
                var element = current;
                if (!_allowTypeCoercion)
                {
                    typeHint = null;
                }
                switch (element)
                {
                    case null:
                        return AnyValue.Null();
                    case bool value:
                        return FromBool(value, typeHint);
                    case int value:
                        return FromNumber(value, typeHint);
                    case long value:
                        return FromNumber(value, typeHint);
                    case float value:
                        return FromNumber(value, typeHint);
                    case double value:
                        return FromNumber(value, typeHint);
                    case string value:
                        return FromString(value, typeHint);
                    case IEnumerable<KeyValuePair<string, object>> dict:
                        PushContext(new ParentContext { ObjectEnumerator = dict.GetEnumerator() });
                        return AnyValue.Object(new ObjectReader());
                    case IEnumerable<KeyValuePair<object, object>> dict:
                        var transformedDict = dict.Select(kv => new KeyValuePair<string, object>(kv.Key.ToString(), kv.Value));
                        PushContext(new ParentContext { ObjectEnumerator = transformedDict.GetEnumerator() });
                        return AnyValue.Object(new ObjectReader());
                    case IEnumerable<object> enumerable:
                        PushContext(new ParentContext { ArrayEnumerator = enumerable.GetEnumerator() });
                        return AnyValue.Array(new ArrayReader());
                    default:
                        throw new SyntaxException(string.Format("ReaderAdapters.FromSimpleTypes does not support type {0}",
                            element.GetType()), null);
                }
            }

            private AnyValue FromBool(bool value, ValueType? typeHint)
            {
                switch (typeHint)
                {
                    case ValueType.String:
                        return AnyValue.String(value ? "true" : "false");
                }
                return AnyValue.Bool(value);
            }

            private AnyValue FromNumber(double value, ValueType? typeHint)
            {
                switch (typeHint)
                {
                    case ValueType.String:
                        return AnyValue.String(value.ToString());
                }
                return AnyValue.Number(value);
            }

            private AnyValue FromString(string value, ValueType? typeHint)
            {
                switch (typeHint)
                {
                    case ValueType.Bool:
                        if (value == "true" || value == "on")
                        {
                            return AnyValue.Bool(true);
                        }
                        if (value == "false" || value == "off")
                        {
                            return AnyValue.Bool(false);
                        }
                        break;
                    case ValueType.Number:
                        if (double.TryParse(value, out var n))
                        {
                            return AnyValue.Number(n);
                        }
                        break;
                }
                return AnyValue.String(value);
            }

            protected override bool TryArrayNext(ref ParentContext context, out object value)
            {
                if (context.ArrayEnumerator.MoveNext())
                {
                    value = context.ArrayEnumerator.Current;
                    return true;
                }
                value = null;
                return false;
            }

            protected override bool TryObjectNext(ref ParentContext context, out string name, out object value)
            {
                if (context.ObjectEnumerator.MoveNext())
                {
                    var property = context.ObjectEnumerator.Current;
                    name = property.Key;
                    value = property.Value;
                    return true;
                }
                name = null;
                value = null;
                return false;
            }
        }

#if USE_SYSTEM_TEXT_JSON
        private sealed class JsonElementReaderAdapter : ReaderAdapterBase<JsonElement, JsonElementReaderAdapter.ParentContext>
        {
            public struct ParentContext
            {
                internal JsonElement.ArrayEnumerator ArrayEnumerator;
                internal JsonElement.ObjectEnumerator ObjectEnumerator;
            }

            internal JsonElementReaderAdapter(JsonElement rootElement) : base(rootElement) { }

            protected override AnyValue ConsumeNext(ValueType? typeHint, ref JsonElement current)
            {
                var element = current;
                switch (element.ValueKind)
                {
                    case JsonValueKind.True:
                        return AnyValue.Bool(true);
                    case JsonValueKind.False:
                        return AnyValue.Bool(false);
                    case JsonValueKind.Number:
                        return AnyValue.Number(element.GetDouble());
                    case JsonValueKind.String:
                        return AnyValue.String(element.GetString());
                    case JsonValueKind.Array:
                        PushContext(new ParentContext { ArrayEnumerator = element.EnumerateArray() });
                        return AnyValue.Array(new ArrayReader(true));
                    case JsonValueKind.Object:
                        PushContext(new ParentContext { ObjectEnumerator = element.EnumerateObject() });
                        return AnyValue.Object(new ObjectReader(true, null));
                    default:
                        return AnyValue.Null();
                }
            }

            protected override bool TryArrayNext(ref ParentContext context, out JsonElement value)
            {
                if (context.ArrayEnumerator.MoveNext())
                {
                    value = context.ArrayEnumerator.Current;
                    return true;
                }
                value = new JsonElement();
                return false;
            }

            protected override bool TryObjectNext(ref ParentContext context, out string name, out JsonElement value)
            {
                if (context.ObjectEnumerator.MoveNext())
                {
                    var property = context.ObjectEnumerator.Current;
                    name = property.Name;
                    value = property.Value;
                    return true;
                }
                name = null;
                value = new JsonElement();
                return false;
            }
        }
#endif

        private abstract class ReaderAdapterBase<TNode, TContext> : IReaderAdapter
        {
            private TNode _current;
            private bool _eof;
            private TContext _parentContext;
            private Stack<TContext> _parentContextStack;

            public bool EOF => _eof;

            protected ReaderAdapterBase(TNode initial)
            {
                _current = initial;
            }

            public AnyValue NextValue(ValueType? desiredType, bool allowNull)
            {
                if (_eof)
                {
                    throw new SyntaxException("unexpected end of input", null);
                }
                var value = ConsumeNext(desiredType, ref _current);
                _eof = _parentContextStack is null || _parentContextStack.Count == 0;
                if (desiredType.HasValue)
                {
                    if (value.Type != desiredType.Value && !(allowNull && value.Type == ValueType.Null))
                    {
                        throw new TypeException(desiredType.Value, value.Type, null);
                    }
                }
                return value;
            }

            public bool ArrayNext(bool first)
            {
                if (TryArrayNext(ref _parentContext, out var value))
                {
                    _current = value;
                    _eof = false;
                    return true;
                }
                _parentContext = _parentContextStack.Pop();
                _eof = _parentContextStack.Count == 0;
                return false;
            }

            public PropertyNameToken ObjectNext(bool first)
            {
                if (TryObjectNext(ref _parentContext, out var name, out var value))
                {
                    _current = value;
                    _eof = false;
                    return new PropertyNameToken(name);
                }
                _parentContext = _parentContextStack.Pop();
                _eof = _parentContextStack.Count == 0;
                return PropertyNameToken.None;
            }

            protected void PushContext(TContext context)
            {
                if (_parentContextStack is null)
                {
                    _parentContextStack = new Stack<TContext>();
                }
                _parentContextStack.Push(_parentContext);
                _parentContext = context;
            }

            protected abstract AnyValue ConsumeNext(ValueType? typeHint, ref TNode current);

            protected abstract bool TryArrayNext(ref TContext context, out TNode value);

            protected abstract bool TryObjectNext(ref TContext context, out string name, out TNode value);
        }
    }
}
