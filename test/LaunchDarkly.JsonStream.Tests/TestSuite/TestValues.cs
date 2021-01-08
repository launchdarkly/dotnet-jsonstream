using System;
using System.Collections.Generic;
using System.Text;

namespace LaunchDarkly.JsonStream.TestSuite
{
    public struct EncodingBehavior
    {
        public bool ForParsing;
    }

    public struct ValueTest<T>
    {
        public string Name;
        public TestValue<T> Value;
        public string Encoding;
    }

    public struct NumberTestValue
    {
        public string Name;
        public double Value;
        public string Encoding;
        public string SimplestEncoding;

        public NumberTestValue(string name, double value, string encoding, string simplestEncoding)
        {
            Name = name;
            Value = value;
            Encoding = encoding;
            SimplestEncoding = simplestEncoding;
        }
    }

    public struct StringTestValue
    {
        public string Name;
        public string Value;
        public string Encoding;

        public StringTestValue(string name, string value, string encoding)
        {
            Name = name;
            Value = value;
            Encoding = encoding;
        }
    }

    public static class ValueTests<T>
    {
        public static IEnumerable<ValueTest<T>> MakeBools()
        {
            return new ValueTest<T>[]
            {
                new ValueTest<T>
                {
                    Name = "bool true",
                    Value = new TestValue<T>
                    {
                        Type = ValueType.Bool,
                        BoolValue = true
                    },
                    Encoding = "true"
                },
                new ValueTest<T>
                {
                    Name = "bool false",
                    Value = new TestValue<T>
                    {
                        Type = ValueType.Bool,
                        BoolValue = false
                    },
                    Encoding = "false"
                }
            };
        }

        public static IEnumerable<ValueTest<T>> MakeNumbers(EncodingBehavior encodingBehavior)
        {
            var ntvs = new NumberTestValue[]
            {
                new NumberTestValue("zero", 0, "0", null),
                new NumberTestValue("int", 3, "3", null),
                new NumberTestValue("int negative", -3, "-3", null),
                new NumberTestValue("int large", 1603312301195, "1603312301195", null), // enough magnitude for a millisecond timestamp
		        new NumberTestValue("float", 3.5, "3.5", null),
                new NumberTestValue("float negative", -3.5, "-3.5", null),
                new NumberTestValue("float with exp", 3500, "3.5e3", "3500"),
                new NumberTestValue("float with Exp", 3500, "3.5E3", "3500"),
                new NumberTestValue("float with exp+", 3500, "3.5e+3", "3500"),
                new NumberTestValue("float with exp-", 0.0035, "3.5e-3", "0.0035")
            };
            var ret = new List<ValueTest<T>>();
            foreach (var ntv in ntvs)
            {
                ret.Add(new ValueTest<T>
                {
                    Name = "number " + ntv.Name,
                    Value = new TestValue<T>
                    {
                        Type = ValueType.Number,
                        NumberValue = ntv.Value
                    },
                    Encoding = (encodingBehavior.ForParsing || ntv.SimplestEncoding == null) ?
                        ntv.Encoding : ntv.SimplestEncoding
                });
            }
            return ret;
        }

        public static IEnumerable<ValueTest<T>> MakeStrings(EncodingBehavior encodingBehavior,
            bool allPermutations)
        {
            var testValues = new List<StringTestValue>()
            {
                new StringTestValue("empty", "", ""),
                new StringTestValue("simple", "abc", "abc"),
            };
            var allEscapeTests = new List<StringTestValue>();
            if (encodingBehavior.ForParsing)
            {
                // These escapes are not used when writing, but may be encountered when parsing
                allEscapeTests.Add(new StringTestValue("", "/", "\\/"));
                allEscapeTests.Add(new StringTestValue("", "も", "\\u3082"));
            }
            if (allPermutations)
            {
                var stringsToEscape = new string[] {
                    "\"", "\\", "\x05", "\x1c", "🦜🦄😂🧶😻 yes"
                };

                var escapeTests = new List<StringTestValue>();
                foreach (var stringValue in stringsToEscape)
                {
                    // JSON writers have some leeway in how they choose to do character escaping. The PlatformBehavior
                    // class will tell us how we expect the writer implementation we're testing to represent the string.
                    escapeTests.Add(new StringTestValue("", stringValue, PlatformBehavior.GetExpectedStringEncoding(stringValue)));
                }
                foreach (var et in escapeTests)
                {
                    allEscapeTests.Add(et);
                    foreach (var s in new string[] { "{0}abcd", "abcd{0}", "ab{0}cd" })
                    {
                        allEscapeTests.Add(new StringTestValue("",
                            String.Format(s, et.Value),
                            String.Format(s, et.Encoding)));
                    }
                    foreach (var et2 in escapeTests)
                    {
                        foreach (var s in new string[] { "{0}{1}abcd", "ab{0}{1}cd", "a{0}bc{1}d", "abcd{0}{1}" })
                        {
                            allEscapeTests.Add(new StringTestValue("",
                                String.Format(s, et.Value, et2.Value),
                                String.Format(s, et.Encoding, et2.Encoding)));
                        }
                    }
                }
            }
            else
            {
                allEscapeTests.Add(new StringTestValue("", "simple\tescape", "simple\\tescape"));
            }
            var i = 0;
            foreach (var et in allEscapeTests)
            {
                testValues.Add(new StringTestValue("with escapes " + i++, et.Value, et.Encoding));
            }
            var ret = new List<ValueTest<T>>();
            foreach (var tv in testValues)
            {
                ret.Add(new ValueTest<T>
                {
                    Name = "string " + tv.Name,
                    Value = new TestValue<T>
                    {
                        Type = ValueType.String,
                        StringValue = tv.Value
                    },
                    Encoding = '"' + tv.Encoding + '"'
                });
            }
            return ret;
        }

        public static List<ValueTest<T>> MakeWhitespaceOptions()
        {
            return new List<ValueTest<T>>()
            {
                new ValueTest<T> { Name = "", Encoding = "" },
                new ValueTest<T> { Name = "spaces", Encoding = "  " },
                new ValueTest<T> { Name = "tab", Encoding = "\t" },
                new ValueTest<T> { Name = "newline", Encoding = "\n" }
            };
        }
    }
}
