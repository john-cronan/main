namespace JPC.Common.UnitTests
{
    [TestClass]
    public class StringEditorTests
    {
        [TestMethod]
        public void Can_start_with_empty_instance()
        {
            var testee = new StringEditor();
            testee.Append("ABC");
            testee.Append("DEF");
            Assert.AreEqual("ABCDEF", testee.ToString());
        }

        [TestMethod]
        public void TrimStart_trims_start_of_single_segment()
        {
            const string TestString = "\tFour score and seven years ago";
            var testee = new StringEditor(TestString);
            testee.TrimStart();
            Assert.AreEqual(TestString.TrimStart(), testee.ToString());
        }

        [TestMethod]
        public void TrimStart_trims_start_of_two_segment_string_when_first_segment_is_all_whitespace()
        {
            const string TestString = "Four score and seven years ago";

            var testee = new StringEditor("\r\n");
            testee.Append(TestString);
            testee.TrimStart();
            Assert.AreEqual(TestString, testee.ToString());
        }

        [TestMethod]
        public void TrimStart_trims_start_of_two_segment_string_when_first_segment_is_not_all_whitespace()
        {
            const string FirstSegment = "    Four ";
            const string SecondSegment = "score and seven years ago";
            const string Expected = "Four score and seven years ago";

            var testee = new StringEditor(FirstSegment);
            testee.Append(SecondSegment);
            testee.TrimStart();
            Assert.AreEqual(Expected, testee.ToString());
        }

        [TestMethod]
        public void Remove_removes_single_character()
        {
            const string TestString = "ABCDEFG";
            var testee = new StringEditor(TestString);
            testee.Remove(chr => chr == 'D');
            Assert.AreEqual("ABCEFG", testee.ToString());
        }

        [TestMethod]
        public void Remove_removes_character_from_two_segments()
        {
            var testee = new StringEditor("AB.C");
            testee.Append("DE.F");
            testee.Remove(chr => chr == '.');
            Assert.AreEqual("ABCDEF", testee.ToString());
        }

        [TestMethod]
        public void Removes_all_characters()
        {
            var testee = new StringEditor(" \t\r\n ");
            testee.Remove(chr => char.IsWhiteSpace(chr));
            Assert.AreEqual("", testee.ToString());
        }

        [TestMethod]
        public void Truncate_removes_nothing()
        {
            var testee = new StringEditor("ABC");
            testee.Append("DEF");
            testee.Append("GHI");
            testee.Truncate(255);
            Assert.AreEqual("ABCDEFGHI", testee.ToString());
        }

        [TestMethod]
        public void Truncate_removes_one_character()
        {
            var testee = new StringEditor("ABCDEF");
            testee.Truncate(5);
            Assert.AreEqual("ABCDE", testee.ToString());
        }

        [TestMethod]
        public void Truncate_removes_entire_segment()
        {
            var testee = new StringEditor("ABC");
            testee.Append("DEFGHI", 0, 3);
            testee.Truncate(3);
            Assert.AreEqual("ABC", testee.ToString());
        }

        [TestMethod]
        public void Truncate_removes_across_two_segments()
        {
            var testee = new StringEditor("ABC");
            testee.Append("DEF");
            testee.Truncate(2);
            Assert.AreEqual("AB", testee.ToString());
        }
    }
}
