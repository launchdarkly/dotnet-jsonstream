using System;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// A decorator that manages the state of a JSON array that is in the process of being
    /// written.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="JWriter.Array"/> creates an <see cref="ArrayWriter"/>. Until
    /// <see cref="ArrayWriter.End"/> or <see cref="ArrayWriter.Dispose"/> is called,
    /// writing any value to the <see cref="ArrayWriter"/> (or to the original
    /// <see cref="JWriter"/>) will cause commas to be added between values as needed.
    /// </remarks>
    public struct ArrayWriter : IValueWriter, IDisposable
    {
        private readonly JWriter _writer;
        private readonly WriterState _previousState;
        private bool _done;

        internal ArrayWriter(JWriter writer, WriterState previousState)
        {
            _writer = writer;
            _previousState = previousState;
            _done = false;
        }

        /// <inheritdoc/>
        public void Null() =>
            _writer?.Null();

        /// <inheritdoc/>
        public void Bool(bool value) =>
            _writer?.Bool(value);

        /// <inheritdoc/>
        public void BoolOrNull(bool? value) =>
            _writer?.BoolOrNull(value);

        /// <inheritdoc/>
        public void Int(int value) =>
            _writer?.Int(value);

        /// <inheritdoc/>
        public void IntOrNull(int? value) =>
            _writer?.IntOrNull(value);

        /// <inheritdoc/>
        public void Long(long value) =>
            _writer?.Long(value);

        /// <inheritdoc/>
        public void LongOrNull(long? value) =>
            _writer?.LongOrNull(value);

        /// <inheritdoc/>
        public void Double(double value) =>
            _writer?.Double(value);

        /// <inheritdoc/>
        public void DoubleOrNull(double? value) =>
            _writer?.DoubleOrNull(value);

        /// <inheritdoc/>
        public void String(string value) =>
            _writer?.String(value);

        /// <inheritdoc/>
        public ArrayWriter Array() =>
            _writer is null ? new ArrayWriter(null, new WriterState()) : _writer.Array();

        /// <inheritdoc/>
        public ObjectWriter Object() =>
            _writer is null ? new ObjectWriter(null, new WriterState()) : _writer.Object();

        /// <summary>
        /// Writes the closing delimiter of the array.
        /// </summary>
        public void End()
        {
            if (_writer != null && !_done)
            {
                _writer._tw.EndArray();
                _writer._state = _previousState;
                _done = true;
            }
        }

        /// <summary>
        /// The <c>Dispose</c> method calls <c>End()</c>, so that you can use a <c>using</c>
        /// block to make the array close automatically.
        /// </summary>
        public void Dispose() => End();
    }
}
