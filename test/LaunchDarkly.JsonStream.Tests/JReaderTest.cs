using System;
using System.Collections.Generic;
using LaunchDarkly.JsonStream.TestSuite;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class JReaderTest
    {
        public delegate void ReaderAction(ref JReader reader);

        [Theory]
        [MemberData(nameof(AllParams))]
        public void TestSuite(TestCase t)
        {
            var reader = JReader.FromString(t.Input);
            foreach (var action in t.Actions)
            {
                action(ref reader);
            }
        }

        [Fact]
        public void SyntaxErrorsUseOurExceptionType()
        {
            var reader = JReader.FromString("{no");
            try
            {
                var obj = reader.Object();
                reader.Int();
                Assert.True(false, "expected exception");
            }
            catch (Exception ex)
            {
                var realEx = reader.TranslateException(ex);
                Assert.IsType<SyntaxException>(realEx);
            }
        }

        [Fact]
        public void TypeErrorsUseOurExceptionType()
        {
            var reader = JReader.FromString("3");
            try
            {
                reader.Bool();
                Assert.True(false, "expected exception");
            }
            catch (Exception ex)
            {
                var realEx = reader.TranslateException(ex);
                var te = Assert.IsType<TypeException>(realEx);
                Assert.Equal(ValueType.Bool, te.ExpectedType);
                Assert.Equal(ValueType.Number, te.ActualType);
            }
        }

        public static IEnumerable<object[]> AllParams()
        {
            foreach (var t in AllTestCases())
            {
                yield return new object[] { t };
            }
        }

        public static IEnumerable<TestCase> AllTestCases()
        {
            var tf = new TestFactory<ReaderAction>(new ReaderValueTestFactory(), new EncodingBehavior
            {
                ForParsing = true
            });
            var tds = tf.MakeAllValueTests();
            var ws = ValueTests<ReaderAction>.MakeWhitespaceOptions();
            foreach (var td in tds)
            {
                foreach (var w in ws)
                {
                    var testName = td.Name;
                    if (w.Name != "")
                    {
                        testName = testName + " [with whitespace: " + w.Name + "]";
                    }
                    var input = w.Encoding + String.Join(w.Encoding, td.Encoding) +
                        w.Encoding;
                    yield return new TestCase
                    {
                        Name = testName,
                        Input = input,
                        Actions = td.Actions
                    };
                }
            }
        }

        public class TestCase
        {
            public string Name { get; set; }
            public string Input { get; set; }
            public IEnumerable<ReaderAction> Actions { get; set; }

            public override string ToString()
            {
                return Name + " (JSON input: `" + Input + "`)";
            }
        }

        // The behavior of Reader is flexible so that callers can choose to read the same JSON value in
        // several different ways. Therefore, we generate variants for each value test as follows:
        // - A null JSON value could be read either as a null, or as a nullable value of another type.
        // - A JSON number could be read as an int (if the test value is an int), a float, or a nullable
        // int or float.
        // - Any other non-null value could be read as its own type, or as a nullable value of that type.
        // - Any value could be read with a nonspecific type using the Any() method.
        // - Any value could be skipped instead of read.
        private static readonly ValueVariant NullableValue = new ValueVariant("nullable");
        private static readonly ValueVariant NumberAsInt = new ValueVariant("int");
        private static readonly ValueVariant NumberAsLong = new ValueVariant("long");
        private static readonly ValueVariant NullableNumberAsInt = new ValueVariant("nullable int");
        private static readonly ValueVariant NullableNumberAsLong = new ValueVariant("nullable long");
        private static readonly ValueVariant NullableBoolIsNull = new ValueVariant("nullable bool is");
        private static readonly ValueVariant NullableIntIsNull = new ValueVariant("nullable int is");
        private static readonly ValueVariant NullableLongIsNull = new ValueVariant("nullable long is");
        private static readonly ValueVariant NullableDoubleIsNull = new ValueVariant("nullable double is");
        private static readonly ValueVariant NullableStringIsNull = new ValueVariant("nullable string is");
        private static readonly ValueVariant NullableArrayIsNull = new ValueVariant("nullable array is");
        private static readonly ValueVariant NullableObjectIsNull = new ValueVariant("nullable object is");
        private static readonly ValueVariant AnyValue = new ValueVariant("any:");
        private static readonly ValueVariant SkipValue = new ValueVariant("skip:");

        private static ValueVariant[] VariantsForNullValues = {
            null, NullableBoolIsNull, NullableIntIsNull, NullableLongIsNull, NullableDoubleIsNull, NullableStringIsNull,
                NullableArrayIsNull, NullableObjectIsNull, AnyValue, SkipValue
        };
        private static ValueVariant[] VariantsForInts = {
            null, NumberAsInt, NullableValue, NullableNumberAsInt, AnyValue, SkipValue
        };
        private static ValueVariant[] VariantsForLongs = {
            null, NumberAsInt, NullableValue, NullableNumberAsLong, AnyValue, SkipValue
        };
        private static ValueVariant[] VariantsForFloats = {
            null, NumberAsInt, NullableValue, AnyValue, SkipValue
        };
        private static ValueVariant[] VariantsForNonNullValues = {
            null, NullableValue, AnyValue, SkipValue
        };

        public ref struct ReaderTestContext
        {
            public readonly JReader Reader;

            public ReaderTestContext(string input)
            {
                Reader = JReader.FromString(input);
            }
        }

        public class ReaderValueTestFactory : IValueTestFactory<ReaderAction>
        {
            public ReaderAction EOF()
            {
                return (ref JReader r) =>
                {
                    if (!r.EOF)
                    {
                        throw new Exception("unexpected data after end");
                    }
                };
            }

            public ValueVariant[] Variants(TestValue<ReaderAction> value)
            {
                switch (value.Type)
                {
                    case ValueType.Null:
                        return VariantsForNullValues;
                    case ValueType.Number:
                        var n = value.NumberValue;
                        if (n <= int.MaxValue && n >= int.MinValue && (double)((int)n) == n)
                        {
                            return VariantsForInts;
                        }
                        if (n <= long.MaxValue && n >= long.MinValue && (double)((long)n) == n)
                        {
                            return VariantsForLongs;
                        }
                        return VariantsForFloats;
                    default:
                        return VariantsForNonNullValues;
                }
            }

            public ReaderAction Value(TestValue<ReaderAction> value, ValueVariant variant)
            {
                return (ref JReader r) =>
                {
                    if (variant == SkipValue)
                    {
                        r.SkipValue();
                        return;
                    }
                    if (variant == AnyValue)
                    {
                        ReadAnyValue(ref r, value);
                        return;
                    }

                    switch (value.Type)
                    {
                        case ValueType.Bool:
                            var bVal = variant == NullableValue ?
                                RequireNonNull(r.BoolOrNull()) : r.Bool();
                            Assert.Equal(value.BoolValue, bVal);
                            break;
                        case ValueType.Number:
                            if (variant == NumberAsInt || variant == NullableNumberAsInt)
                            {
                                var nVal = variant == NullableValue ?
                                    RequireNonNull(r.IntOrNull()) : r.Int();
                                Assert.Equal((int)value.NumberValue, nVal);
                            }
                            else if (variant == NumberAsLong || variant == NullableNumberAsLong)
                            {
                                var nVal = variant == NullableValue ?
                                    RequireNonNull(r.LongOrNull()) : r.Long();
                                Assert.Equal((long)value.NumberValue, nVal);
                            }
                            else
                            {
                                var nVal = variant == NullableValue ?
                                    RequireNonNull(r.DoubleOrNull()) : r.Double();
                                Assert.Equal(value.NumberValue, nVal);
                            }
                            break;
                        case ValueType.String:
                            var sVal = variant == NullableValue ? r.StringOrNull() : r.String();
                            Assert.False(sVal == null, "expected non-null");
                            Assert.Equal(value.StringValue, sVal);
                            break;
                        case ValueType.Array:
                            ReadArray(ref r, variant == NullableValue ? r.ArrayOrNull() : r.Array(), value);
                            break;
                        case ValueType.Object:
                            ReadObject(ref r, variant == NullableValue ? r.ObjectOrNull() : r.Object(), value);
                            break;
                        default:
                            ReadNull(ref r, variant);
                            break;
                    }
                };
            }

            private T RequireNonNull<T>(Nullable<T> maybeValue) where T : struct
            {
                Assert.True(maybeValue.HasValue, "expected non-null");
                return maybeValue.Value;
            }

            private void RequireNull<T>(Nullable<T> maybeValue) where T : struct
            {
                Assert.False(maybeValue.HasValue, "expected null");
            }

            private void ReadNull(ref JReader r, ValueVariant variant)
            {
                if (variant == NullableBoolIsNull)
                    RequireNull(r.BoolOrNull());
                else if (variant == NullableIntIsNull)
                    RequireNull(r.IntOrNull());
                else if (variant == NullableLongIsNull)
                    RequireNull(r.LongOrNull());
                else if (variant == NullableDoubleIsNull)
                    RequireNull(r.DoubleOrNull());
                else if (variant == NullableStringIsNull)
                    Assert.True(r.StringOrNull() == null, "expected null");
                else if (variant == NullableArrayIsNull)
                {
                    Assert.False(r.ArrayOrNull().IsDefined, "expected null");
                }
                else if (variant == NullableObjectIsNull)
                {
                    Assert.False(r.ObjectOrNull().IsDefined, "expected null");
                }
                else
                    r.Null();
            }

            private void ReadArray(ref JReader r, ArrayReader arr, TestValue<ReaderAction> value)
            {
                Assert.True(arr.IsDefined);
                foreach (var e in value.ArrayValue)
                {
                    Assert.True(arr.Next(ref r), "expected array item");
                    e(ref r);
                }
                Assert.False(arr.Next(ref r), "expected end of array");
            }

            private void ReadObject(ref JReader r, ObjectReader obj, TestValue<ReaderAction> value)
            {
                Assert.True(obj.IsDefined);
                foreach (var p in value.ObjectValue)
                {
                    Assert.True(obj.Next(ref r), "expected object property");
                    Assert.Equal(p.Name, obj.Name.ToString());
                    foreach (var action in p.Actions)
                    {
                        action(ref r);
                    }
                }
                Assert.False(obj.Next(ref r), "expected end of object");

            }

            private void ReadAnyValue(ref JReader r, TestValue<ReaderAction> value)
            {
                var av = r.Any();
                Assert.Equal(value.Type, av.Type);
                switch (value.Type)
                {
                    case ValueType.Bool:
                        Assert.Equal(value.BoolValue, av.BoolValue);
                        break;
                    case ValueType.Number:
                        Assert.Equal(value.NumberValue, av.NumberValue);
                        break;
                    case ValueType.String:
                        Assert.Equal(value.StringValue, av.StringValue.ToString());
                        break;
                    case ValueType.Array:
                        ReadArray(ref r, av.ArrayValue, value);
                        break;
                    case ValueType.Object:
                        ReadObject(ref r, av.ObjectValue, value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
