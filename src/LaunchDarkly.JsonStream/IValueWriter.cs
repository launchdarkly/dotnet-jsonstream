
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Common interface for classes that allow writing a JSON value (<see cref="JWriter"/>
    /// and <see cref="ArrayWriter"/>).
    /// </summary>
    public interface IValueWriter
    {
        /// <summary>
        /// Writes a JSON null value to the output.
        /// </summary>
        void Null();

        /// <summary>
        /// Writes a JSON boolean value to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void Bool(bool value);

        /// <summary>
        /// Writes either a JSON boolean value or a JSON null to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void BoolOrNull(bool? value);

        /// <summary>
        /// Writes a JSON numeric value to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void Int(int value);

        /// <summary>
        /// Writes either a JSON numeric value or a JSON null to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void IntOrNull(int? value);

        /// <summary>
        /// Writes a JSON numeric value to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void Long(long value);

        /// <summary>
        /// Writes either a JSON numeric value or a JSON null to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void LongOrNull(long? value);

        /// <summary>
        /// Writes a JSON numeric value to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void Double(double value);

        /// <summary>
        /// Writes either a JSON numeric value or a JSON null to the output.
        /// </summary>
        /// <param name="value">the value</param>
        void DoubleOrNull(double? value);

        /// <summary>
        /// Writes a JSON string value to the output, adding quotes and performing any
        /// necessary escaping. If the value is <see langword="null"/>, it writes a null instead.
        /// </summary>
        /// <param name="value">the value</param>
        void String(string value);

        /// <summary>
        /// Begins writing a JSON array to the output.
        /// </summary>
        /// <remarks>
        /// It returns an <see cref="ArrayWriter"/> that provides the array formatting. You must
        /// call <see cref="ArrayWriter.End"/> when finished (or use a <c>using</c> block to
        /// cause it to close automatically).
        /// </remarks>
        /// <returns>an <see cref="ArrayWriter"/></returns>
        ArrayWriter Array();

        /// <summary>
        /// Begins writing a JSON object to the output.
        /// </summary>
        /// <remarks>
        /// It returns an <see cref="ObjectWriter"/> that provides the object formatting. You must
        /// call <see cref="ObjectWriter.End"/> when finished (or use a <c>using</c> block to
        /// cause it to close automatically).
        /// </remarks>
        /// <returns>an <see cref="ObjectWriter"/></returns>
        ObjectWriter Object();
    }
}
