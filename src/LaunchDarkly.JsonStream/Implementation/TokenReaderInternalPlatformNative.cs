#if NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0

using System;
using System.Text;
using System.Text.Json;

namespace LaunchDarkly.JsonStream.Implementation
{
    internal ref partial struct TokenReader
    {
		private readonly byte[] _input;
		private Utf8JsonReader _nativeReader;
		private bool _alreadyRead;

		public TokenReader(string input)
		{
			_input = Encoding.UTF8.GetBytes(input);
			_nativeReader = new Utf8JsonReader(_input);
			_unreadToken = null;
			_alreadyRead = false;
		}

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
			string[] lines = inputStr.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
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
					return Token.String(StringToken.FromString(_nativeReader.GetString()));
				case JsonTokenType.StartArray:
					return Token.Array();
				case JsonTokenType.StartObject:
					return Token.Object();
				default:
					throw MakeSyntaxException(string.Format("unexpected JSON token \"{0}\"",
						Encoding.UTF8.GetString(_nativeReader.ValueSpan)));
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

		private StringToken? ObjectNextInternal(bool first)
		{
			if (!_nativeReader.Read())
            {
				throw MakeSyntaxException("unexpected end of input");
            }
			switch (_nativeReader.TokenType)
			{
				case JsonTokenType.EndObject:
					return null;
				case JsonTokenType.PropertyName:
					// It's common for property names to contain only printable ASCII characters. In that
					// case, we can avoid a string allocation by returning a view on the internal data.
					return StringToken.FromString(_nativeReader.GetString());
				default:
					throw MakeSyntaxException(string.Format("unexpected JSON token \"{0}\"",
						_nativeReader.ValueSpan.ToString()));
            }
		}
	}
}

#endif
