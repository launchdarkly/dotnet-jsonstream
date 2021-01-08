using System.Text;

namespace LaunchDarkly.JsonStream
{
    public static class PlatformBehavior
    {
        public static bool WriterMustEncodeAsHex(char ch)
        {
#if USE_SYSTEM_TEXT_JSON
            return ch < 32 || ch == '"' || ch > 127;
#else
            return false;
#endif
        }

        public static string GetExpectedStringEncoding(string stringValue)
        {
            var enc = new StringBuilder();
            foreach (var ch in stringValue)
            {
                if (WriterMustEncodeAsHex(ch))
                {
                    enc.Append(HexEscape(ch));
                }
                else
                {
                    switch (ch)
                    {
                        case '"':
                            enc.Append("\\\"");
                            break;
                        case '\b':
                            enc.Append("\\b");
                            break;
                        case '\n':
                            enc.Append("\\n");
                            break;
                        case '\r':
                            enc.Append("\\r");
                            break;
                        case '\t':
                            enc.Append("\\t");
                            break;
                        case '\\':
                            enc.Append("\\\\");
                            break;
                        default:
                            if (ch < 32)
                            {
                                enc.Append(HexEscape(ch));
                            }
                            else
                            {
                                enc.Append(ch);
                            }
                            break;
                    }
                }
            }
            return enc.ToString();
        }

        private static string HexEscape(char ch)
        {
            var hex = ((int)ch).ToString("X");
            while (hex.Length < 4)
            {
                hex = "0" + hex;
            }
            return "\\u" + hex;
        }
    }
}
