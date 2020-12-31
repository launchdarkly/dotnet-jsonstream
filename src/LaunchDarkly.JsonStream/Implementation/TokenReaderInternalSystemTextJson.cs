#if USE_SYSTEM_TEXT_JSON

using System;
using System.Text;
using System.Text.Json;

namespace LaunchDarkly.JsonStream.Implementation
{
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct (that's OK)
	internal ref partial struct TokenReader
#pragma warning restore CS0282
	{
		private readonly byte[] _input;
		private Utf8JsonReader _nativeReader;
		private bool _alreadyRead;

		public TokenReader(string input) : this(Encoding.UTF8.GetBytes(input)) { }

		public TokenReader(byte[] input)
		{
			_input = input;
			_nativeReader = new Utf8JsonReader(_input);
			_unreadToken = null;
			_alreadyRead = false;
		}

		public static bool IsSystemTextJsonImplementation => true;

		public Exception TranslateException(Exception e)
        {
			switch (e)
            {
				case JsonException je:
					return new SyntaxException(je.Message,
						CalculateCharOffsetFromLineAndPos(je.LineNumber, je.BytePositionInLine));
				default:
					return e;
            }
        }

		private int? CalculateCharOffsetFromLineAndPos(long? line, long? column)
        {
			// This is an inefficient calculation, but it only happens if we have tried to parse
			// malformed JSON. Error handling in that case is not expected to be highly performant.
			if (!line.HasValue || line.Value < 0)
            {
				return null;
            }
			string inputStr = Encoding.UTF8.GetString(_input);
			string[] lines = inputStr.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			if (line.Value >= lines.Length)
            {
				return null;
            }
			int lineStartCharPos = 0;
			for (int i = 0; i < line.Value; i++)
            {
				lineStartCharPos += lines[i].Length + 1;
            }
			if (!column.HasValue)
            {
				return lineStartCharPos;
            }
			byte[] lineBytes = Encoding.UTF8.GetBytes(lines[line.Value]);
			if (column.Value > lineBytes.Length)
			{
				return lineStartCharPos;
			}
			return lineStartCharPos + Encoding.UTF8.GetString(lineBytes, 0, (int)column.Value).Length;
        }

		private int LastPosInternal =>
			(int)_nativeReader.BytesConsumed;

		private Token? ParseTokenInternal()
		{
			if (_alreadyRead)
            {
				_alreadyRead = false;
            }
			else if (!_nativeReader.Read())
			{
				return null;
			}
			switch (_nativeReader.TokenType)
            {
				case JsonTokenType.Null:
					return Token.Null();
				case JsonTokenType.True:
					return Token.Bool(true);
				case JsonTokenType.False:
					return Token.Bool(false);
				case JsonTokenType.Number:
					return Token.Number(_nativeReader.GetDouble());
				case JsonTokenType.String:
					return Token.String(_nativeReader.GetString());
				case JsonTokenType.StartArray:
					return Token.Array();
				case JsonTokenType.StartObject:
					return Token.Object();
				default:
					throw MakeSyntaxException(string.Format("unexpected JSON token \"{0}\"",
						Encoding.UTF8.GetString(_nativeReader.ValueSpan.ToArray())));
            }
		}

		private bool ArrayNextInternal(bool first)
		{
			if (!_nativeReader.Read())
            {
				throw MakeSyntaxException("unexpected end of input");
			}
			if (_nativeReader.TokenType == JsonTokenType.EndArray)
            {
				return false;
            }
			_alreadyRead = true;
			return true;
		}

		private PropertyNameToken ObjectNextInternal(bool first)
		{
			if (!_nativeReader.Read())
            {
				throw MakeSyntaxException("unexpected end of input");
            }
			switch (_nativeReader.TokenType)
			{
				case JsonTokenType.EndObject:
					return new PropertyNameToken();
				case JsonTokenType.PropertyName:
					// It's common for property names to contain only unescaped ASCII characters. In that
					// case, we can avoid a string allocation by returning a view on the internal data
					// (assuming that data is contiguous in memory, which it is if HasValueSequence is
					// false).
					if (!_nativeReader.HasValueSequence)
                    {
						var span = _nativeReader.ValueSpan;
						var len = span.Length;
						bool isASCII = true;
						for (var i = 0; i < len; i++)
                        {
							var ch = span[i];
							if (ch < 32 || ch > 127 || ch == '\\')
                            {
								isASCII = false;
								break;
                            }
                        }
						if (isASCII)
                        {
							return new PropertyNameToken(span);
                        }
                    }
					return new PropertyNameToken(_nativeReader.GetString());
				default:
					throw MakeSyntaxException(string.Format("unexpected JSON token \"{0}\"",
						_nativeReader.ValueSpan.ToString()));
            }
		}
	}
}

#endif
