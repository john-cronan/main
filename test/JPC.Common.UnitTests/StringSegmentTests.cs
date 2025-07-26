namespace JPC.Common.UnitTests
{
    [TestClass]
    public class StringSegmentTests
    {
        [TestMethod]
        public void ToString_returns_specified_subset()
        {
            var testee = new StringSegment("String", 1, "String".Length - 2);
            Assert.AreEqual("trin", testee.ToString());
        }

        [TestMethod]
        public void TrimStart_removes_leading_whitespace_from_start_of_string()
        {
            const string TestString = "   \t\t Four score and seven years ago";
            var testee = new StringSegment(TestString);
            var actual = testee.TrimStart();
            Assert.AreEqual(TestString.TrimStart(), actual.ToString());
            Assert.AreNotEqual(0, actual.Start);
            Assert.IsTrue(actual.Length < TestString.Length);
        }

        [TestMethod]
        public void TrimStart_removes_leading_whitespace_from_start_of_substring()
        {
            const string TestString = "Four Four score and seven years ago";
            const string Expected = "Four score and seven years ago";
            var testee = new StringSegment(TestString, 4, TestString.Length - 4);
            var actual = testee.TrimStart();
            Assert.AreEqual(Expected, actual.ToString());
        }

        [TestMethod]
        public void TrimStart_returns_null_if_segment_is_all_whitespace()
        {
            const string TestString = "Four         \t\t\r\n";
            var testee = new StringSegment(TestString, 4, TestString.Length - 4);
            Assert.AreEqual(StringSegment.Null, testee.TrimStart());
        }

        [TestMethod]
        public void TrimEnd_removes_trailing_whitespace_from_end_of_string()
        {
            const string TestString = "Four score and seven years ago   \t\t  \r\n\r\n  \t\t";
            var testee = new StringSegment(TestString);
            var actual = testee.TrimEnd();
            Assert.AreEqual(TestString.TrimEnd(), actual.ToString());
        }

        [TestMethod]
        public void TrimEnd_removes_trailing_whitespace_from_middle_of_substring()
        {
            const string TestString = "Four     score and seven years ago";
            var testee = new StringSegment(TestString, 0, 8);
            var actual = testee.TrimEnd();
            Assert.AreEqual("Four", actual.ToString());
        }

        [TestMethod]
        public void TrimEnd_returns_null_if_segment_is_all_whitespace()
        {
            var testee = new StringSegment("    ");
            var actual = testee.TrimEnd();
            Assert.AreEqual(StringSegment.Null, actual);
        }

        [TestMethod]
        public void Removes_nothing()
        {
            const string TestString = "ABCDEFG";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'Q').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(1, actualResults.Length);
            Assert.IsTrue(object.ReferenceEquals(testee, actualResults[0]));
        }

        [TestMethod]
        public void Removes_everything()
        {
            const string TestString = " \t\r\n";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => char.IsWhiteSpace(chr)).ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(0, actualResults.Length);
        }

        [TestMethod]
        public void Removes_one_character_from_middle_of_string()
        {
            const string TestString = "ABCDEFG";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'D').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(2, actualResults.Length);
            Assert.AreEqual("ABC", actualResults[0].ToString());
            Assert.AreEqual("EFG", actualResults[1].ToString());
        }

        [TestMethod]
        public void Removes_two_characters_from_middle_of_string()
        {
            const string TestString = "ABCDEFG";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'C' || chr == 'E').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(3, actualResults.Length);
            Assert.AreEqual("AB", actualResults[0].ToString());
            Assert.AreEqual("D", actualResults[1].ToString());
            Assert.AreEqual("FG", actualResults[2].ToString());
        }

        [TestMethod]
        public void Removes_characters_at_start_of_string()
        {
            const string TestString = "ABCDEF";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'A' || chr == 'B').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(1, actualResults.Length);
            Assert.AreEqual("CDEF", actualResults[0].ToString());
        }

        [TestMethod]
        public void Removes_characters_at_end_of_string()
        {
            const string TestString = "ABCDEF";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'F').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(1, actualResults.Length);
            Assert.AreEqual("ABCDE", actualResults[0].ToString());
        }

        [TestMethod]
        public void Removes_three_contiguous_characters()
        {
            const string TestString = "ABCDEF";
            var testee = new StringSegment(TestString);
            var actualResults = testee.Remove(chr => chr == 'B' || chr == 'C' || chr == 'D').ToArray();
            Assert.IsNotNull(actualResults);
            Assert.AreEqual(2, actualResults.Length);
            Assert.AreEqual("A", actualResults[0].ToString());
            Assert.AreEqual("EF", actualResults[1].ToString());
        }

        [TestMethod]
        public void Writes_all_characters_to_TextWriter()
        {
            const string TestString = "XYZABC123";
            var testee = new StringSegment(TestString);
            using var actualOutput = new StringWriter();
            testee.WriteTo(actualOutput);
            actualOutput.Flush();
            var actuallyWritten = actualOutput.ToString();
            Assert.AreEqual(TestString, actuallyWritten);
        }

        [TestMethod]
        public void Truncates_to_requested_length()
        {
            const string TestString = "ABCDEFHI";
            var testee = new StringSegment(TestString);
            var actual = testee.Truncate(5);
            Assert.AreEqual("ABCDE", actual.ToString());
        }

        [TestMethod]
        public void Truncate_returns_original_instance()
        {
            const string TestString = "ABCDEFHI";
            var testee = new StringSegment(TestString);
            var actual = testee.Truncate(128);
            Assert.IsTrue(object.ReferenceEquals(testee, actual));
        }
    }
}
