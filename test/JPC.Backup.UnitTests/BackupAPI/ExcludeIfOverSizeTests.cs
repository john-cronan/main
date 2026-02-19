using JPC.Common;
using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class ExcludeIfOverSizeTests
    {
        private MockRuntime _mockRuntime;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRuntime = new MockRuntime();
        }

        [TestMethod]
        public void Excludes_if_too_big()
        {
            var sourceFile = new FileInformation(@"C:\Users\You\Documents", "Budget.xls",
                true, length: (long)FileSize.From(10, FileSizeUnits.GB).Value(FileSizeUnits.Bytes) + 1);
            _mockRuntime.Filesystem.FileExists(sourceFile);

            IExcludeRule testee = new ExcludeIfFileOverSize(_mockRuntime.Filesystem.Object,
                FileSize.From(10, FileSizeUnits.GB));
            var actual = testee.ExcludeObject(Path.Combine(sourceFile.DirectoryPath, sourceFile.Name),
                "doesnt-matter");

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Does_not_exclude_if_too_small()
        {
            var sourceFile = new FileInformation(@"C:\Users\You\Documents", "Budget.xls",
                true, length: (long)FileSize.From(9.5, FileSizeUnits.GB).Value(FileSizeUnits.Bytes));
            _mockRuntime.Filesystem.FileExists(sourceFile);

            IExcludeRule testee = new ExcludeIfFileOverSize(_mockRuntime.Filesystem.Object,
                FileSize.From(10, FileSizeUnits.GB));
            var actual = testee.ExcludeObject(Path.Combine(sourceFile.DirectoryPath, sourceFile.Name),
                "doesnt-matter");

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Does_not_exclude_if_equal()
        {
            var sourceFile = new FileInformation(@"C:\Users\You\Documents", "Budget.xls",
                true, length: (long)FileSize.From(10, FileSizeUnits.GB).Value(FileSizeUnits.Bytes));
            _mockRuntime.Filesystem.FileExists(sourceFile);

            IExcludeRule testee = new ExcludeIfFileOverSize(_mockRuntime.Filesystem.Object,
                FileSize.From(10, FileSizeUnits.GB));
            var actual = testee.ExcludeObject(Path.Combine(sourceFile.DirectoryPath, sourceFile.Name),
                "doesnt-matter");

            Assert.IsFalse(actual);
        }
    }
}
