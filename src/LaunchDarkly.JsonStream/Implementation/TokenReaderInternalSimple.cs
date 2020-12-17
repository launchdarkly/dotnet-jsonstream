#if !USE_SYSTEM_TEXT_JSON

// This implementation of low-level TokenReader methods is used on platforms that do not have the
// System.Text.Json API.

using System;
using System.Text;

namespace LaunchDarkly.JsonStream.Implementation
{
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct (that's OK)
    internal ref partial struct TokenReader
#pragma warning restore CS0282
    {
		private readonly char[] _buf;
		private readonly int _length;
		private int _pos;
		private int _lastPos;

		public TokenReader(string input) : this(input.ToCharArray()) { }

		public TokenReader(char[] input)
		{
			_buf = input;
			_length = input.Length;
			_pos = 0;
			_lastPos = 0;
			_unreadToken = null;
		}

		public static bool IsSystemTextJsonImplementation => false;

		// Don't need to translate exceptions in this implementation because they're all thrown by us
		public Exception TranslateException(Exception e) => e;

		private int LastPosInternal => _lastPos;

		private Token? ParseTokenInternal()
        {
			var maybeCh = SkipWhitespaceAndMaybeReadChar();
			if (!maybeCh.HasValue)
            {
				return null;
            }
			var ch = maybeCh.Value;
			if (char.IsLetter(ch))
			{
				for (; _pos < _length && char.IsLetter(_buf[_pos]); _pos++) { }
				var st = StringToken.FromChars(_buf, _lastPos, _pos - _lastPos);
				if (st.Equals("null"))
				{
					return Token.Null();
				}
				if (st.Equals("true"))
				{
					return Token.Bool(true);
				}
				if (st.Equals("false"))
				{
					return Token.Bool(false);
				}
				throw MakeSyntaxException("invalid token");
			}
			else if (char.IsDigit(ch) || ch == '-')
			{
				return Token.Number(ReadNumber(ch));
			}
			else if (ch == '"')
			{
				var st = ReadString();
				return Token.String(st.ToString());
			}
			else if (ch == '[')
			{
				return Token.Array();
			}
			else if (ch == '{')
			{
				return Token.Object();
			}
			else
			{
				throw MakeSyntaxException("unexpected character '" + ch + "'");
			}
		}

		private bool ArrayNextInternal(bool first)
        {
			var ch = SkipWhitespaceAndRequireChar();
			if (ch == ']')
			{
				return false;
			}
			if (first)
			{
				_pos--;
				return true;
			}
			if (ch != ',')
			{
				throw MakeSyntaxException("expected comma or end of array");
			}
			return true;
		}

		private PropertyNameToken ObjectNextInternal(bool first)
		{
			var ch = SkipWhitespaceAndRequireChar();
			if (ch == '}')
			{
				return new PropertyNameToken();
			}
			if (!first)
			{
				if (ch != ',')
				{
					throw MakeSyntaxException("expected comma or end of object");
				}
				ch = SkipWhitespaceAndRequireChar();
			}
			if (ch != '"')
			{
				UnreadChar();
				var t = Next(); // will throw SyntaxException if there's not a valid token
				throw MakeSyntaxException(string.Format("expected property name, found {0}", t.Type));
			}
			var st = ReadString();
			ch = SkipWhitespaceAndRequireChar();
			if (ch != ':')
			{
				throw MakeSyntaxException("expected colon");
			}
			return new PropertyNameToken(st);
		}

		private char? SkipWhitespaceAndMaybeReadChar()
		{
			while (_pos < _length)
			{
				var ch = _buf[_pos];
				if (!Char.IsWhiteSpace(ch))
				{
					_lastPos = _pos;
					_pos++;
					return ch;
				}
				_pos++;
			}
			return null;
		}

		private char SkipWhitespaceAndRequireChar()
		{
			var maybeCh = SkipWhitespaceAndMaybeReadChar();
			if (maybeCh == null)
			{
				throw MakeSyntaxException("unexpected end of input");
			}
			return maybeCh.Value;
		}

		private void UnreadChar()
		{
			_pos--;
		}

		private double ReadNumber(char firstCh)
		{
			var hasDecimal = false;
			char ch = (char)0;
			for (; _pos < _length; _pos++)
			{
				ch = _buf[_pos];
				if (!char.IsDigit(ch) && !(ch == '.' && !hasDecimal))
				{
					break;
				}
				if (ch == '.')
				{
					hasDecimal = true;
				}
			}
			if (ch == 'e' || ch == 'E')
			{
				// exponent must match this regex: [eE][-+]?[0-9]+
				_pos++;
				if (_pos >= _length)
				{
					throw new Exception("no"); // TODO
				}
				ch = _buf[_pos];
				if (ch == '+' || ch == '-')
				{
					_pos++;
				}
				else if (ch < '0' || ch > '9')
				{
					throw new Exception("no"); // TODO
				}
				var haveExpDigits = false;
				for (; _pos < _length; _pos++)
				{
					ch = _buf[_pos];
					if (ch < '0' || ch > '9')
					{
						break;
					}
					haveExpDigits = true;
				}
				if (!haveExpDigits)
				{
					throw new Exception("no"); // TODO
				}
			}
			var st = StringToken.FromChars(_buf, _lastPos, _pos - _lastPos);
			return hasDecimal ? st.ParseDouble() : st.ParseLong();
		}

		private StringToken ReadString()
		{
			var startPos = _pos; // the opening quote mark has already been read
			StringBuilder tempStr = null;
			while (true)
			{
				if (_pos >= _length)
				{
					throw new Exception("badstr"); // TODO
				}
				var ch = _buf[_pos++];
				if (ch == '"')
				{
					break;
				}
				if (ch != '\\')
				{
					if (tempStr != null)
					{
						tempStr.Append(ch);
					}
					continue;
				}
				if (_pos >= _length)
				{
					throw MakeSyntaxException("unterminated string");
				}
				if (tempStr == null)
				{
					var lengthSoFar = _pos - startPos - 1;
					tempStr = new StringBuilder(lengthSoFar + 10);
					tempStr.Append(_buf, startPos, lengthSoFar);
				}
				ch = _buf[_pos++];
				switch (ch)
				{
					case '"':
					case '\\':
					case '/':
						tempStr.Append(ch);
						break;
					case 'b':
						tempStr.Append('\b');
						break;
					case 'f':
						tempStr.Append('\f');
						break;
					case 'n':
						tempStr.Append('\n');
						break;
					case 'r':
						tempStr.Append('\r');
						break;
					case 't':
						tempStr.Append('\t');
						break;
					case 'u':
						tempStr.Append(ReadHexChar());
						break;
					default:
						throw MakeSyntaxException("invalid string escape sequence");
				}
			}
			var endPos = _pos - 1;
			if (tempStr == null)
			{
				return StringToken.FromChars(_buf, startPos, endPos - startPos);
			}
			return StringToken.FromString(tempStr.ToString());
		}

		private char ReadHexChar()
		{
			uint ret = 0;
			for (int i = 0; i < 4; i++)
			{
				if (_pos >= _length)
				{
					throw new Exception("unexpected end of input");
				}
				var ch = _buf[_pos++];
				ret <<= 4;
				if (ch >= '0' && ch <= '9')
				{
					ret += (uint)(ch - '0');
				}
				else if (ch >= 'A' && ch <= 'F')
				{
					ret += (uint)(ch - 'A' + 10);
				}
				else if (ch >= 'a' && ch <= 'f')
				{
					ret += (uint)(ch - 'a' + 10);
				}
				else
				{
					throw MakeSyntaxException("invalid string escape sequence");
				}
			}
			return (char)ret;
		}
	}
}

#endif
