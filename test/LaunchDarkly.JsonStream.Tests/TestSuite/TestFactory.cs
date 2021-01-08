using System.Collections.Generic;
using System.Linq;

namespace LaunchDarkly.JsonStream.TestSuite
{
    public struct TestDef<ActionT>
    {
        public string Name;
        public IEnumerable<string> Encoding;
        public IEnumerable<ActionT> Actions;

        public TestDef<ActionT> Then(TestDef<ActionT> next)
        {
            return new TestDef<ActionT>
            {
                Name = this.Name + ", " + next.Name,
                Encoding = Encoding.Concat(next.Encoding),
                Actions = Actions.Concat(next.Actions)
            };
        }
    }

    public sealed class TestFactory<ActionT>
    {
        private readonly IValueTestFactory<ActionT> _valueTestFactory;
        private readonly EncodingBehavior _encodingBehavior;

        public TestFactory(IValueTestFactory<ActionT> valueTestFactory, EncodingBehavior encodingBehavior)
        {
            _valueTestFactory = valueTestFactory;
            _encodingBehavior = encodingBehavior;
        }

        public IEnumerable<TestDef<ActionT>> MakeAllValueTests()
        {
            var eofTest = new TestDef<ActionT>
            {
                Name = "EOF",
                Encoding = new string[0],
                Actions = new ActionT[] { _valueTestFactory.EOF() }
            };
            var tests = MakeScalarValueTests(true)
                .Concat(MakeArrayTests())
                .Concat(MakeObjectTests());
            foreach (var td in tests)
            {
                yield return td.Then(eofTest);
            }
        }

        public IEnumerable<TestDef<ActionT>> MakeScalarValueTests(bool allPermutations)
        {
            var values = new List<ValueTest<ActionT>>();
            values.Add(new ValueTest<ActionT>
            {
                Name = "null",
                Encoding = "null",
                Value = new TestValue<ActionT> { Type = ValueType.Null }
            });
            values.AddRange(ValueTests<ActionT>.MakeBools());
            values.AddRange(ValueTests<ActionT>.MakeNumbers(_encodingBehavior));
            values.AddRange(ValueTests<ActionT>.MakeStrings(_encodingBehavior, allPermutations));

            foreach (var vt in values)
            {
                var variants = _valueTestFactory.Variants(vt.Value);
                if (variants == null)
                {
                    variants = new ValueVariant[]{ null };
                }
                foreach (var variant in variants)
                {
                    var td = new TestDef<ActionT>
                    {
                        Name = variant == null ? vt.Name :
                            (variant.ToString() + " " + vt.Name),
                        Encoding = new string[] { vt.Encoding },
                        Actions = new ActionT[] { _valueTestFactory.Value(vt.Value, variant) }
                    };
                    yield return td;
                }
            }
        }

        public IEnumerable<TestDef<ActionT>> MakeArrayTests()
        {
            for (var elementCount = 0; elementCount <= 2; elementCount++)
            {
                foreach (var contents in MakeValueListsOfLength(elementCount))
                {

                    var names = new List<string>();
                    var encoding = new List<string>() { "[" };
                    var actions = new List<ActionT>();
                    for (var i = 0; i < contents.Count; i++)
                    {
                        var td = contents[i];
                        names.Add(td.Name);
                        if (i > 0)
                        {
                            encoding.Add(",");
                        }
                        encoding.AddRange(td.Encoding);
                        actions.AddRange(td.Actions);
                    }
                    encoding.Add("]");
                    var value = new TestValue<ActionT>
                    {
                        Type = ValueType.Array,
                        ArrayValue = actions.ToArray()
                    };
                    yield return new TestDef<ActionT>
                    {
                        Name = "array(" + string.Join(", ", names) + ")",
                        Encoding = encoding.ToArray(),
                        Actions = new ActionT[] { _valueTestFactory.Value(value, null) }
                    };
                }
            }
        }

        public IEnumerable<TestDef<ActionT>> MakeObjectTests()
        {
            for (var elementCount = 0; elementCount <= 2; elementCount++)
            {
                foreach (var contents in MakeValueListsOfLength(elementCount))
                {
                    var names = new List<string>();
                    var encoding = new List<string>() { "{" };
                    var propActions = new List<PropertyAction<ActionT>>();
                    for (var i = 0; i < contents.Count; i++)
                    {
                        var td = contents[i];
                        var propName = "prop" + i;
                        names.Add(propName + ": " + td.Name);
                        if (i > 0)
                        {
                            encoding.Add(",");
                        }
                        encoding.Add("\"" + propName + "\"");
                        encoding.Add(":");
                        encoding.AddRange(td.Encoding);
                        propActions.Add(new PropertyAction<ActionT>
                        {
                            Name = propName,
                            Actions = td.Actions
                        });
                    }
                    encoding.Add("}");
                    var value = new TestValue<ActionT>
                    {
                        Type = ValueType.Object,
                        ObjectValue = propActions.ToArray()
                    };
                    yield return new TestDef<ActionT>
                    {
                        Name="object(" + string.Join(", ", names) + ")",
                        Encoding = encoding.ToArray(),
                        Actions = new ActionT[] { _valueTestFactory.Value(value, null) }
                    };
                }
            }
        }

        public IEnumerable<List<TestDef<ActionT>>> MakeValueListsOfLength(int count)
        {
            if (count == 0)
            {
                yield return new List<TestDef<ActionT>>();
            }
            else
            {
                foreach (var previous in MakeValueListsOfLength(count - 1))
                {
                    foreach (var elementTest in MakeScalarValueTests(false))
                    {
                        var l = new List<TestDef<ActionT>>(previous);
                        l.Add(elementTest);
                        yield return l;
                    }
                }
            }
        }
    }
}
