using System.IO;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ObjectWriterTest
    {
        [Fact]
        public void MaybeProperty()
        {
            var w = JWriter.New();
            var obj = w.Object();

            obj.MaybeProperty("a", true).Bool(true);
            obj.MaybeProperty("b", false).Null();
            obj.MaybeProperty("b", false).Bool(true);
            obj.MaybeProperty("b", false).BoolOrNull(true);
            obj.MaybeProperty("b", false).BoolOrNull(null);
            obj.MaybeProperty("b", false).Int(1);
            obj.MaybeProperty("b", false).IntOrNull(1);
            obj.MaybeProperty("b", false).IntOrNull(null);
            obj.MaybeProperty("b", false).Long(1);
            obj.MaybeProperty("b", false).LongOrNull(1);
            obj.MaybeProperty("b", false).LongOrNull(null);
            obj.MaybeProperty("b", false).Double(1);
            obj.MaybeProperty("b", false).DoubleOrNull(1);
            obj.MaybeProperty("b", false).DoubleOrNull(null);
            obj.MaybeProperty("b", false).String("x");
            obj.MaybeProperty("b", false).String(null);
            obj.MaybeProperty("b", false).Array();
            obj.MaybeProperty("b", false).Object();
            
            obj.End();
            var expected = @"{""a"":true}";
            Assert.Equal(expected, w.GetString());
        }

        [Fact]
        public void AutoClose()
        {
            var w = JWriter.New();
            using (var obj = w.Object())
            {
                obj.Property("a").Int(1);
                obj.Property("b").Int(2);
            }
            Assert.Equal(@"{""a"":1,""b"":2}", w.GetString());
        }
    }
}
