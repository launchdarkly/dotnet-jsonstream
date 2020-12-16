
namespace LaunchDarkly.JsonStream.Implementation
{
    public interface ITokenReader
    {
		bool EOF { get; }

		int LastPos { get; }

		int Pos { get; }

		bool Null();

		bool Bool();

		double Number();

		StringToken String();

		void StartArray();

		bool ArrayNext(bool first);

		void StartObject();

		StringToken? ObjectNext(bool first);

		Token Any();
	}
}
