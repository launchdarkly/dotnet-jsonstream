
// The implementations of partial methods in this class are conditionally compiled from
// either TokenWriterInternalSimple.cs or TokenWriterInternalSystemTextJson.cs.

namespace LaunchDarkly.JsonStream.Implementation
{
    internal partial class TokenWriterBase
    {
        partial void Null();
        partial void Bool(bool value);
        partial void Long(long value);
        partial void Double(double value);
        partial void String(string value);
        partial void StartArray();
        partial void NextArrayItem();
        partial void EndArray();
        partial void StartObject();
        partial void NextObjectItem(string propertyName, bool first);
        partial void EndObject();

        // Non-void methods can't be declared as partial, but they are still expected to
        // be implemented conditionally like the others:

        // TokenWriter(int initialCapacity);
        // string GetString();
        // byte[] GetUtf8Bytes();
        // Stream GetUtf8Stream();
    }
}
