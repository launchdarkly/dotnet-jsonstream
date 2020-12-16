
namespace LaunchDarkly.JsonStream.Implementation
{
    public struct Token
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
        /// The value if the JSON value is a string, or an empty <see cref="StringToken"/> otherwise.
        /// </summary>
        public StringToken StringValue { get; }

        private Token(ValueType valueType, bool boolVal, double numberVal, StringToken stringVal)
        {
            Type = valueType;
            BoolValue = boolVal;
            NumberValue = numberVal;
            StringValue = stringVal;
        }

        /// <summary>
        /// Initializes a null value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token Null() =>
            new Token(ValueType.Null, false, 0, StringToken.Empty);

        /// <summary>
        /// Initializes a boolean value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token Bool(bool value) =>
            new Token(ValueType.Bool, value, 0, StringToken.Empty);

        /// <summary>
        /// Initializes a number value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token Number(double value) =>
            new Token(ValueType.Number, false, value, StringToken.Empty);

        /// <summary>
        /// Initializes a string value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token String(StringToken value) =>
            new Token(ValueType.String, false, 0, value);

        /// <summary>
        /// Initializes an array value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token Array() =>
            new Token(ValueType.Array, false, 0, StringToken.Empty);

        /// <summary>
        /// Initializes an object value.
        /// </summary>
        /// <returns>a <c>Token</c></returns>
        public static Token Object() =>
            new Token(ValueType.Object, false, 0, StringToken.Empty);
    }
}
