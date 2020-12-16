using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ObjectReaderTest
    {
        [Fact]
        public void ReadsProperties()
        {
            var r = JReader.FromString(@"{""a"":1, ""b"":2}");
            int a = 0, b = 0;

            for (var obj = r.Object(); obj.Next(ref r);)
            {
                if (obj.Name == "a")
                {
                    a = r.Int();
                }
                else if (obj.Name == "b")
                {
                    b = r.Int();
                }
            }

            Assert.Equal(1, a);
            Assert.Equal(2, b);
        }

        [Fact]
        public void RecursivelySkipsUnusedValue()
        {
            var r = JReader.FromString(@"{""a"":1, ""ignore"":[false,false,false], ""b"":2}");
            int a = 0, b = 0;

            for (var obj = r.Object(); obj.Next(ref r);)
            {
                if (obj.Name == "a")
                {
                    a = r.Int();
                }
                else if (obj.Name == "b")
                {
                    b = r.Int();
                }
            }

            Assert.Equal(1, a);
            Assert.Equal(2, b);
        }

        [Fact]
        public void CanExplicitlySkipValue()
        {
            var r = JReader.FromString(@"{""a"":1, ""ignore"":[false,false,false], ""b"":2}");
            int a = 0, b = 0;

            for (var obj = r.Object(); obj.Next(ref r);)
            {
                if (obj.Name == "a")
                {
                    a = r.Int();
                }
                else if (obj.Name == "b")
                {
                    b = r.Int();
                }
                else
                {
                    r.SkipValue();
                }
            }

            Assert.Equal(1, a);
            Assert.Equal(2, b);
        }

        [Fact]
        public void RequiredPropertiesAreAllFound()
        {
            var r = JReader.FromString(@"{""a"":1, ""b"":2, ""c"":3}");
            var requiredProps = new string[] { "c", "b", "a" };
            for (var obj = r.Object().WithRequiredProperties(requiredProps); obj.Next(ref r);)
            { }
        }

        [Fact]
        public void RequiredPropertyIsNotFound()
        {
            var r = JReader.FromString(@"{""a"":1, ""c"":3}");
            var requiredProps = new string[] { "c", "b", "a" };
            try
            {
                for (var obj = r.Object().WithRequiredProperties(requiredProps); obj.Next(ref r);)
                { }
                Assert.True(false, "expected RequiredPropertyException");
            }
            catch (RequiredPropertyException e)
            {
                Assert.Equal("b", e.Name);
            }
        }
    }
}
