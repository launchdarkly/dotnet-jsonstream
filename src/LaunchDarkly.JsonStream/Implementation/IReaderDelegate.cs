
namespace LaunchDarkly.JsonStream.Implementation
{
    /// <summary>
    /// An internal mechanism that allows us to substitute some other data source for
    /// <c>JReader</c>'s normal use of <c>Utf8JsonReader</c>. We use this in
    /// <c>JsonStreamConverterSystemTextJson</c> when we need to read some data that has
    /// already been read by a previously existing <c>Utf8JsonReader</c>.
    /// </summary>
    /// <remarks>
    /// We don't use this abstraction in any other circumstance. The reason JReader has a lot of
    /// tests like <c>if (_delegate == null) { _tr.DoSomething(); } else { _delegate.DoSomething() }"</c>
    /// is that <c>TokenReader</c>, being a <c>ref struct</c>, cannot implement an interface.
    /// </remarks>
    internal interface IReaderDelegate
    {
        bool EOF { get; }
        void Null();
        bool Bool();
        bool? BoolOrNull();
        double Number();
        double? NumberOrNull();
        string String();
        string StringOrNull();
        ArrayReader Array();
        ArrayReader ArrayOrNull();
        bool ArrayNext(bool first);
        ObjectReader Object();
        ObjectReader ObjectOrNull();
        PropertyNameToken ObjectNext(bool first);
        AnyValue Any();
        void SkipValue();
    }
}
