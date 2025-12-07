namespace JPC.Common.UnitTests
{
    [TestClass]
    public class FileSizeTests
    {
        [TestMethod]
        public void Four_GB_is_greater_than_two_bytes()
        {
            var value1 = FileSize.From(4.0, FileSizeUnits.GB);
            var value2 = FileSize.From(2, FileSizeUnits.Bytes);

            Assert.IsFalse(value1 == value2);
            Assert.IsFalse(value1.Equals(value2));
            Assert.IsTrue(value1 > value2);
            Assert.IsTrue(value2 < value1);
        }

        [TestMethod]
        public void Two_MB_equals_two_MB()
        {
            var value1 = FileSize.From(2, FileSizeUnits.MB);
            var value2 = FileSize.From(2 * 1024 * 1024, FileSizeUnits.Bytes);

            Assert.IsTrue(value1 == value2);
            Assert.IsFalse(value1 != value2);
            Assert.IsTrue(value1.Equals(value2));
            Assert.IsFalse(value1 < value2);
            Assert.IsFalse(value1 < value2);
        }

        [TestMethod]
        public void EB_are_greater_than_KB()
        {
            var value1 = FileSize.From(13, FileSizeUnits.EB);
            var value2 = FileSize.From(13, FileSizeUnits.KB);

            Assert.IsFalse(value1 == value2);
            Assert.IsTrue(value1 != value2);
            Assert.IsFalse(value1.Equals(value2));
            Assert.IsFalse(value1 < value2);
            Assert.IsTrue(value1 > value2);
        }

        [TestMethod]
        public void Parses_13_MB()
        {
            var values = new FileSize[]
            {
                FileSize.Parse("13 MB"),
                FileSize.Parse("13MB"),
                FileSize.Parse("13312 KB"),
                FileSize.Parse("13312KB")
            };
            Assert.IsTrue(values.All(v => v.Value(FileSizeUnits.MB) == 13));
        }

    }
}
