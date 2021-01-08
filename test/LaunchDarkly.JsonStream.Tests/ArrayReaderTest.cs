using System.Collections.Generic;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ArrayReaderTest
    {
        [Fact]
        public void ReadsValues()
        {
            var r = JReader.FromString("[10, 20, 30]");
            var values = new List<int>();

            for (var arr = r.Array(); arr.Next(ref r);)
            {
                values.Add(r.Int());
            }

            Assert.Equal(new List<int> { 10, 20, 30 }, values);
        }

        [Fact]
        public void RecursivelySkipsUnusedValue()
        {
            var r = JReader.FromString(@"[10, {""ignore"": [false,false,false]}, 20]");
            var values = new List<int>();

            var arr = r.Array();

            Assert.True(arr.Next(ref r));
            values.Add(r.Int());

            Assert.True(arr.Next(ref r));

            Assert.True(arr.Next(ref r));
            values.Add(r.Int());

            Assert.False(arr.Next(ref r));

            Assert.Equal(new List<int> { 10, 20 }, values);
        }
    }
}
