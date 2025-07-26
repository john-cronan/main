using System.Text;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class StringBuilderExtensionTests
    {
        [TestMethod]
        public void Remove_removes_non_ascii_characters()
        {
            var str = new StringBuilder("xyz°abc");
            str.Remove(chr => (int)chr > 0x7b);
            Assert.AreEqual(6, str.Length);
            Assert.AreEqual("xyzabc", str.ToString());
        }

        [TestMethod]
        public void Remove_removes_nothing()
        {
            var str = new StringBuilder("xyz°abc");
            str.Remove(chr => false);
            Assert.AreEqual(7, str.Length);
            Assert.AreEqual("xyz°abc", str.ToString());
        }

        [TestMethod]
        public void Trim_removes_everything()
        {
            var str = new StringBuilder("  \t  ");
            str = str.Trim();
            Assert.AreEqual(0, str.Length);
        }

        [TestMethod]
        public void Trim_removes_nothing()
        {
            var str = new StringBuilder("When in the course of human events");
            str = StringBuilderExtensions.Trim(str);
            Assert.AreEqual("When in the course of human events", str.ToString());
        }

        [TestMethod]
        public void Trims_from_start()
        {
            var str = new StringBuilder("     When in the course of human events");
            str = StringBuilderExtensions.Trim(str);
            Assert.AreEqual("   When in the course of human events".TrimStart(), str.ToString());
        }

        [TestMethod]
        public void Trims_from_end()
        {
            var str = new StringBuilder("When in the course of human events   ");
            str = StringBuilderExtensions.Trim(str);
            Assert.AreEqual("When in the course of human events   ".TrimEnd(), str.ToString());
        }
    }
}
