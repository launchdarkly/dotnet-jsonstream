using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LaunchDarkly.JsonStream.TestSuite;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class JWriterTest
    {
        public delegate void WriterAction(JWriter writer);

        [Theory]
        [MemberData(nameof(AllParams))]
        public void TestSuite(TestCase t)
        {
            var writer = JWriter.New();
            foreach (var action in t.Actions)
            {
                action(writer);
            }
            var output = writer.GetString();
            var pattern = "\\w*" +
                string.Join("\\w*", t.Encoding.Select(Regex.Escape)) +
                "\\w*";
            Assert.Matches(pattern, output);
        }

        [Fact]
        public void GetStringOrUtf8Bytes()
        {
            var stringValue = "enchanté 😀";
            var writer = JWriter.New();
            writer.String(stringValue);

            var expected = "\"" + PlatformBehavior.GetExpectedStringEncoding(stringValue) + "\"";

            Assert.Equal(expected, writer.GetString());

            var bytes = writer.GetUtf8Bytes();
            Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes);

            var stream = writer.GetUtf8Stream();
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            Assert.Equal(expected, streamReader.ReadToEnd());
        }

#if NETCOREAPP3_1 || NET5_0
        [Fact]
        public void GetUtf8ReadOnlyMemory()
        {
            var stringValue = "enchanté 😀";
            var writer = JWriter.New();
            writer.String(stringValue);

            var expected = "\"" + PlatformBehavior.GetExpectedStringEncoding(stringValue) + "\"";

            var memory = writer.GetUTF8ReadOnlyMemory();
            Assert.Equal(Encoding.UTF8.GetBytes(expected), memory.ToArray());
        }
#endif

        public static IEnumerable<object[]> AllParams()
        {
            foreach (var t in AllTestCases())
            {
                yield return new object[] { t };
            }
        }

        public static IEnumerable<TestCase> AllTestCases()
        {
            var tf = new TestFactory<WriterAction>(new WriterValueTestFactory(), new EncodingBehavior
            {
                ForParsing = false
            });
            foreach (var td in tf.MakeAllValueTests())
            {
                yield return new TestCase
                {
                    Name = td.Name,
                    Encoding = td.Encoding,
                    Actions = td.Actions
                };
            }
        }

        private static readonly ValueVariant NumberAsInt = new ValueVariant("int");
        private static readonly ValueVariant NumberAsLong = new ValueVariant("long");

        private static readonly ValueVariant[] VariantsForInts = { null, NumberAsInt, NumberAsLong };
        private static readonly ValueVariant[] VariantsForLongs = { null, NumberAsLong };

        public class TestCase
        {
            public string Name { get; set; }
            public IEnumerable<string> Encoding { get; set; }
            public IEnumerable<WriterAction> Actions { get; set; }

            public override string ToString()
            {
                return Name + " (expect JSON: `" + string.Join("", Encoding) + "`)";
            }
        }

        public class WriterValueTestFactory : IValueTestFactory<WriterAction>
        {
            public WriterAction EOF() => w => { };

            public ValueVariant[] Variants(TestValue<WriterAction> value)
            {
                if (value.Type == ValueType.Number)
                {
                    var n = value.NumberValue;
                    if (n <= int.MaxValue && n >= int.MinValue && (double)((int)n) == n)
                    {
                        return VariantsForInts;
                    }
                    if (n <= long.MaxValue && n >= long.MinValue && (double)((long)n) == n)
                    {
                        return VariantsForLongs;
                    }
                }
                return null;
            }

            public WriterAction Value(TestValue<WriterAction> value, ValueVariant variant)
            {
                return w =>
                {
                    switch (value.Type)
                    {
                        case ValueType.Bool:
                            w.Bool(value.BoolValue);
                            break;
                        case ValueType.Number:
                            if (variant == NumberAsLong)
                            {
                                w.Long((long)value.NumberValue);
                            }
                            else if (variant == NumberAsInt)
                            {
                                w.Int((int)value.NumberValue);
                            }
                            else
                            {
                                w.Double(value.NumberValue);
                            }
                            break;
                        case ValueType.String:
                            w.String(value.StringValue);
                            break;
                        case ValueType.Array:
                            using (var arr = w.Array())
                            {
                                foreach (var e in value.ArrayValue)
                                {
                                    e(w);
                                }
                            }
                            break;
                        case ValueType.Object:
                            using (var obj = w.Object())
                            {
                                foreach (var p in value.ObjectValue)
                                {
                                    var valueWriter = obj.Property(p.Name);
                                    foreach (var action in p.Actions)
                                    {
                                        action(w);
                                    }
                                }
                            }
                            break;
                        default:
                            w.Null();
                            break;
                    }
                };
            }
        }
    }
}
