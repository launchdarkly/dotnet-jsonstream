using System;
using System.IO;
using Xunit;

namespace LaunchDarkly.JsonStream
{
    public class ArrayWriterTest
    {
        [Fact]
        public void WritingToJWriterIsSameAsWritingToArrayWriter()
        {
            Action<IValueWriter> writeValues = w =>
            {
                w.Null();
                w.Bool(true);
                w.Int(1);
                w.String("x");
                var arr = w.Array();
                arr.End();
                var obj = w.Object();
                obj.End();
            };
            Action<string> verify = s =>
                Assert.Equal(@"[null,true,1,""x"",[],{}]", s);

            var w1 = JWriter.New();
            var arr1 = w1.Array();
            writeValues(arr1);
            arr1.End();
            verify(w1.GetString());

            var w2 = JWriter.New();
            var arr2 = w2.Array();
            writeValues(w2);
            arr2.End();
            verify(w2.GetString());
        }

        [Fact]
        public void AutoClose()
        {
            var w = JWriter.New();
            using (var arr = w.Array())
            {
                arr.Int(1);
                arr.Int(2);
            }
            Assert.Equal("[1,2]", w.GetString());
        }
    }
}
