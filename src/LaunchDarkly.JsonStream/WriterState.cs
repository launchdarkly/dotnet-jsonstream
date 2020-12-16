
namespace LaunchDarkly.JsonStream
{
    /// <summary>
    /// Keeps track of semantic state such as whether we're within an array. This has
    /// stack-like behavior, but to avoid allocating an actual stack, we use ArrayState and
    /// ObjectState to hold previous values of this struct.
    /// </summary>
    internal struct WriterState
    {
        internal bool InArray;
        internal bool ArrayHasItems;
    }
}
