using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ObjectWriterTest
    {
        [Fact]
        public void MaybeName()
        {
            var w = JWriter.New();
            var obj = w.Object();

            obj.MaybeName("a", true).Bool(true);
            obj.MaybeName("b", false).Null();
            obj.MaybeName("b", false).Bool(true);
            obj.MaybeName("b", false).BoolOrNull(true);
            obj.MaybeName("b", false).BoolOrNull(null);
            obj.MaybeName("b", false).Int(1);
            obj.MaybeName("b", false).IntOrNull(1);
            obj.MaybeName("b", false).IntOrNull(null);
            obj.MaybeName("b", false).Long(1);
            obj.MaybeName("b", false).LongOrNull(1);
            obj.MaybeName("b", false).LongOrNull(null);
            obj.MaybeName("b", false).Double(1);
            obj.MaybeName("b", false).DoubleOrNull(1);
            obj.MaybeName("b", false).DoubleOrNull(null);
            obj.MaybeName("b", false).String("x");
            obj.MaybeName("b", false).String(null);
            obj.MaybeName("b", false).Array();
            obj.MaybeName("b", false).Object();
            
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
                obj.Name("a").Int(1);
                obj.Name("b").Int(2);
            }
            Assert.Equal(@"{""a"":1,""b"":2}", w.GetString());
        }
    }
}
