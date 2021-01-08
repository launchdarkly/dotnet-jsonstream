
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Defines the allowable value types for <see cref="AnyValue"/>.
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// Indicates that the value is a null.
        /// </summary>
        Null,

        /// <summary>
        /// Indicates that the value is a boolean.
        /// </summary>
        Bool,

        /// <summary>
        /// Indicates that the value is a number.
        /// </summary>
        Number,

        /// <summary>
        /// Indicates that the value is a string.
        /// </summary>
        String,

        /// <summary>
        /// Indicates that the value is an array.
        /// </summary>
        Array,

        /// <summary>
        /// Indicates that the value is an object.
        /// </summary>
        Object
    }

    /// <summary>
    /// Returned by <see cref="JReader.Any"/> to represent a JSON value of an arbitrary type.
    /// </summary>
    public ref struct AnyValue
    {
        /// <summary>
        /// Describes the type of the JSON value.
        /// </summary>
        public ValueType Type { get; }

        /// <summary>
        /// The value if the JSON value is a boolean, or false otherwise.
        /// </summary>
        public bool BoolValue { get; }

        /// <summary>
        /// The value if the JSON value is a number, or zero otherwise.
        /// </summary>
        public double NumberValue { get; }

        /// <summary>
        /// The value if the JSON value is a string, or null otherwise.
        /// </summary>
        public string StringValue { get; }

        /// <summary>
        /// An <see cref="ArrayReader"/> that can be used to iterate through the array elements if the JSON
	    /// value is an array, or an uninitialized <see cref="ArrayReader"/> otherwise.
        /// </summary>
        public ArrayReader ArrayValue { get; }

        /// <summary>
        /// An <see cref="ObjectReader"/> that can be used to iterate through the object properties if the
	    /// JSON value is an object, or an uninitialized <see cref="ObjectReader"/> otherwise.
        /// </summary>
        public ObjectReader ObjectValue { get; }

        private AnyValue(ValueType valueType, bool boolVal, double numberVal,
            string stringVal, ArrayReader arrayVal, ObjectReader objectVal)
        {
            Type = valueType;
            BoolValue = boolVal;
            NumberValue = numberVal;
            StringValue = stringVal;
            ArrayValue = arrayVal;
            ObjectValue = objectVal;
        }

        /// <summary>
        /// Initializes a null value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue Null() =>
            new AnyValue(ValueType.Null, false, 0, null,
                new ArrayReader(), new ObjectReader());

        /// <summary>
        /// Initializes a boolean value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue Bool(bool value) =>
            new AnyValue(ValueType.Bool, value, 0, null,
                new ArrayReader(), new ObjectReader());

        /// <summary>
        /// Initializes a number value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue Number(double value) =>
            new AnyValue(ValueType.Number, false, value, null,
                new ArrayReader(), new ObjectReader());

        /// <summary>
        /// Initializes a string value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue String(string value) =>
            new AnyValue(ValueType.String, false, 0, value,
                new ArrayReader(), new ObjectReader());

        /// <summary>
        /// Initializes an array value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue Array(ArrayReader value) =>
            new AnyValue(ValueType.Array, false, 0, null,
                value, new ObjectReader());

        /// <summary>
        /// Initializes an object value.
        /// </summary>
        /// <returns>an <c>AnyValue</c></returns>
        public static AnyValue Object(ObjectReader value) =>
            new AnyValue(ValueType.Object, false, 0, null,
                new ArrayReader(), value);
    }
}
