
namespace LaunchDarkly.JsonStream
{
    internal class NoOpWriter : IValueWriter
    {
        internal static readonly NoOpWriter Instance = new NoOpWriter();

        private NoOpWriter() { }

        public ArrayWriter Array() => new ArrayWriter(null, new WriterState());

        public void Bool(bool value) { }

        public void BoolOrNull(bool? value) { }

        public void Double(double value) { }

        public void DoubleOrNull(double? value) { }

        public void Int(int value) { }

        public void IntOrNull(int? value) { }

        public void Long(long value) { }

        public void LongOrNull(long? value) { }

        public void Null() { }

        public ObjectWriter Object() => new ObjectWriter(null, new WriterState());

        public void String(string value) { }
    }
}
