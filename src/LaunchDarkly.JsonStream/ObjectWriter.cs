using System;

namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// A a decorator that manages the state of a JSON object that is in the process of being
    /// written.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="JWriter.Object"/> creates an <see cref="ObjectWriter"/>. Use
    /// <see cref="Name(string)"/> or <see cref="MaybeName(string, bool)"/> to begin
    /// writing each object property, and <see cref="IValueWriter"/> methods to write each
    /// value. Calling <see cref="ObjectWriter.End"/> or <see cref="ObjectWriter.Dispose"/>
    /// terminates the object.
    /// </remarks>
    public struct ObjectWriter : IDisposable
    {
        private readonly JWriter _writer;
        private readonly WriterState _previousState;
        private bool _hasItems;
        private bool _done;

        internal ObjectWriter(JWriter writer, WriterState previousState)
        {
            _writer = writer;
            _previousState = previousState;
            _hasItems = false;
            _done = false;
        }

        /// <summary>
        /// Writes an object property name and a colon.
        /// </summary>
        /// <remarks>
        /// To write the property value, call any <see cref="IValueWriter"/> method on
        /// the returned <see cref="IValueWriter"/>.
        /// </summary>
        /// <example>
        /// <code>
        ///     var obj = writer.Object();
        ///     obj.Name("myStringProperty").String("the value");
        /// </code>
        /// </example>
        /// <param name="name">the property name</param>
        /// <returns>an <see cref="IValueWriter"/> for writing the value</returns>
        public IValueWriter Name(string name)
        {
            if (_writer is null || _done)
            {
                return NoOpWriter.Instance;
            }
            _writer._tw.NextObjectItem(name, !_hasItems);
            _hasItems = true;
            return _writer;
        }

        /// <summary>
        /// Writes an object property name conditionally depending on a boolean parameter.
        /// </summary>
        /// <remarks>
        /// If <paramref name="present"/> is <see langword="true"/>, this behaves the same as
        /// <see cref="Name(string)"/>. However, if <paramref name="present"/> is
        /// <see langword="false"/>, it does not write a property name and instead of
        /// returning a functional <see cref="IValueWriter"/>, it returns a stub object that
        /// does not produce any output.
        /// </remarks>
        /// <example>
        /// <code>
        ///     var obj = writer.Object();
        ///
        ///     // this writes a property "present":"value"
        ///     obj.MaybeName("present", true).String("value");
        ///
        ///     // this writes nothing
        ///     obj.MaybeName("absent", false).String("value");
        /// </code>
        /// </example>
        /// <param name="name">the property name</param>
        /// <param name="present">true if the property should be written</param>
        /// <returns>an <see cref="IValueWriter"/> which may or may not be enabled</returns>
        public IValueWriter MaybeName(string name, bool present)
        {
            if (present)
            {
                return Name(name);
            }
            return NoOpWriter.Instance;
        }

        /// <summary>
        /// Writes the closing delimiter of the object.
        /// </summary>
        public void End()
        {
            if (_writer != null && !_done)
            {
                _writer._tw.EndObject();
                _writer._state = _previousState;
                _done = true;
            }
        }

        /// <summary>
        /// The <c>Dispose</c> method calls <c>End()</c>, so that you can use a <c>using</c>
        /// block to make the object close automatically.
        /// </summary>
        public void Dispose() => End();
    }
}
