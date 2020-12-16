using System.Collections.Generic;

namespace LaunchDarkly.JsonStream.TestSuite
{
    public struct PropertyAction<ActionT>
    {
        public string Name;
        public IEnumerable<ActionT> Actions;
    }

    public sealed class ValueVariant
    {
        private readonly string _name;

        public ValueVariant(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name == null ? "default" : _name;
        }
    }

    public interface IValueTestFactory<ActionT>
    {
        ActionT EOF();
        ActionT Value(TestValue<ActionT> value, ValueVariant variant);
        ValueVariant[] Variants(TestValue<ActionT> value);
    }

    public struct TestValue<ActionT>
    {
        public ValueType Type;
        public bool BoolValue;
        public double NumberValue;
        public string StringValue;
        public IEnumerable<ActionT> ArrayValue;
        public IEnumerable<PropertyAction<ActionT>> ObjectValue;
    }
}
