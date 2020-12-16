using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class StringTokenTest
    {
        [Fact]
        public void Empty()
        {
            var st = StringToken.Empty;
            Assert.Equal(0, st.Length);
            ShouldEqual("", st);
            ShouldEqual(StringToken.Empty, st);
            ShouldEqual(StringToken.FromChars("x".ToCharArray(), 1, 0), st);
            ShouldNotEqual("x", st);
            ShouldNotEqual(StringToken.FromString("x"), st);
            ShouldNotEqual(StringToken.FromChars("x".ToCharArray(), 0, 1), st);
        }

        [Fact]
        public void FromChars()
        {
            var st = StringToken.FromChars("xabcz".ToCharArray(), 1, 3);
            Assert.Equal(3, st.Length);
            ShouldEqual("abc", st);
            ShouldEqual(StringToken.FromString("abc"), st);
            ShouldEqual(StringToken.FromChars("xabcz".ToCharArray(), 1, 3), st);
            ShouldNotEqual("def", st);
            ShouldNotEqual(StringToken.FromString("def"), st);
            ShouldNotEqual(StringToken.FromChars("def".ToCharArray(), 0, 3), st);
            ShouldNotEqual(StringToken.FromChars("abc".ToCharArray(), 0, 2), st);
        }

        [Fact]
        public void FromString()
        {
            var st = StringToken.FromString("abc");
            Assert.Equal(3, st.Length);
            ShouldEqual("abc", st);
            ShouldEqual(StringToken.FromString("abc"), st);
            ShouldEqual(StringToken.FromChars("xabcz".ToCharArray(), 1, 3), st);
            ShouldNotEqual("def", st);
            ShouldNotEqual(StringToken.FromString("def"), st);
            ShouldNotEqual(StringToken.FromChars("def".ToCharArray(), 0, 3), st);
            ShouldNotEqual(StringToken.FromChars("abc".ToCharArray(), 0, 2), st);
        }

#if NET5_0
        [Fact]
        public void AsSpan()
        {
            var span0 = StringToken.Empty.AsSpan();
            Assert.Equal(0, span0.Length);
            Assert.Equal("", span0.ToString());

            var span1 = StringToken.FromString("abc").AsSpan();
            Assert.Equal(3, span1.Length);
            Assert.Equal("abc", span1.ToString());

            var span2 = StringToken.FromChars("xabcz".ToCharArray(), 1, 3).AsSpan();
            Assert.Equal(3, span2.Length);
            Assert.Equal("abc", span2.ToString());
        }
#endif

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void ParseLong(long n)
        {
            var s = n.ToString();
            Assert.Equal(n, StringToken.FromString(s).ParseLong());
            Assert.Equal(n, StringToken.FromChars(("x" + s + "z").ToCharArray(),
                1, s.Length).ParseLong());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(1234567890.5)]
        [InlineData(-1234567890.5)]
        public void ParseDouble(double n)
        {
            var s = n.ToString();
            Assert.Equal(n, StringToken.FromString(s).ParseDouble());
            Assert.Equal(n, StringToken.FromChars(("x" + s + "z").ToCharArray(),
                1, s.Length).ParseDouble());
        }

        void ShouldEqual(string expected, StringToken actual)
        {
            Assert.True(actual.Equals(expected));
            Assert.True(actual == expected);
            Assert.False(actual != expected);
            Assert.Equal(expected, actual.ToString());
        }

        void ShouldEqual(StringToken expected, StringToken actual)
        {
            Assert.True(actual.Equals(expected));
            Assert.True(actual == expected);
            Assert.False(actual != expected);
        }

        void ShouldNotEqual(string expected, StringToken actual)
        {
            Assert.False(actual.Equals(expected));
            Assert.False(actual == expected);
            Assert.True(actual != expected);
            Assert.NotEqual(expected, actual.ToString());
        }

        void ShouldNotEqual(StringToken expected, StringToken actual)
        {
            Assert.False(actual.Equals(expected));
            Assert.False(actual == expected);
            Assert.True(actual != expected);
        }
    }
}
