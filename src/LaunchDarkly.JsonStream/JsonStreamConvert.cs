
namespace LaunchDarkly.JsonStream
{
    public static class JsonStreamConvert
    {
        public static string SerializeObject<T>(T instance)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var writer = JWriter.New();
            converter.WriteJson(instance, writer);
            return writer.GetString();
        }

        public static byte[] SerializeObjectToUTF8Bytes<T>(T instance)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var writer = JWriter.New();
            converter.WriteJson(instance, writer);
            return writer.GetUTF8Bytes();
        }

        public static T DeserializeObject<T>(string json)
        {
            var converter = JsonStreamConverterAttribute.GetConverter<T>();
            var reader = JReader.FromString(json);
            return converter.ReadJson(ref reader);
        }
    }
}
