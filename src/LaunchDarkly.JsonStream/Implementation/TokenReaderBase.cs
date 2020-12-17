using System;
using System.Text;

// The implementations of some methods in this class are conditionally compiled from
// either TokenReaderInternalSimple.cs or TokenReaderInternalSystemTextJson.cs.

namespace LaunchDarkly.JsonStream.Implementation
{
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct (that's OK)
    internal ref partial struct TokenReader
#pragma warning restore CS0282
    {
		private Token? _unreadToken;

		public bool EOF
		{
			get
			{
				if (_unreadToken.HasValue)
				{
					return false;
				}
				var maybeToken = ParseTokenInternal();
				if (!maybeToken.HasValue)
                {
					return true;
                }
				PutBack(maybeToken.Value);
				return false;
			}
		}

		public int LastPos => LastPosInternal;

		public bool Null()
		{
			var t = Next();
			if (t.Type == ValueType.Null)
            {
				return true;
            }
			PutBack(t);
			return false;
		}

		public bool Bool() =>
			Consume(ValueType.Bool).BoolValue;

		public double Number() =>
			Consume(ValueType.Number).NumberValue;
		
		public string String() =>
			Consume(ValueType.String).StringValue;

		public void StartArray() =>
			Consume(ValueType.Array);

		public bool ArrayNext(bool first) =>
			ArrayNextInternal(first);

		public void StartObject() =>
			Consume(ValueType.Object);

		public PropertyNameToken ObjectNext(bool first) =>
			ObjectNextInternal(first);

		public Token Any() =>
			Next();

		private Token Next()
        {
			if (_unreadToken.HasValue)
            {
				var t = _unreadToken.Value;
				_unreadToken = null;
				return t;
            }
			var maybeToken = ParseTokenInternal();
			if (!maybeToken.HasValue)
            {
				throw MakeSyntaxException("unexpected end of input");
            }
			return maybeToken.Value;
        }

		private Token Consume(ValueType expectedType)
        {
			var t = Next();
			if (t.Type != expectedType)
            {
				throw new TypeException(expectedType, t.Type, LastPos);
            }
			return t;
        }

		private void PutBack(Token t)
        {
			_unreadToken = t;
        }

		private SyntaxException MakeSyntaxException(string message) =>
        	new SyntaxException(message, LastPos);

		// The following methods are conditionally compiled from either TokenReaderInternalDefault.cs
		// or TokenReaderInternalPlatformNative.cs. They can't be declared here as "partial" because
		// they have non-void return types.

		// public static bool IsPlatformNativeImplementation { get; }
		// public Exception TranslateException(Exception);
		// private int LastPosInternal { get; }
		// private Token? ParseTokenInternal();
		// private bool ArrayNextInternal(bool first);
		// private PropertyNameToken ObjectNextInternal(bool first);
	}
}
