using JPC.Common;
using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class ExcludeIfDestinationReadOnlyTests
    {
        private MockRuntime _mockRuntime;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRuntime = new MockRuntime();
        }

        [TestMethod]
        public void Excludes_if_destination_is_read_only()
        {
            var sourcePath = @"C:\Users\Somebody\Documents\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\Somebody\Documents\Budget.xls";

            var sourceAttributes = FileAttributes.Normal;
            var sourceFileInfo = new FileInformation(
                Path.GetDirectoryName(sourcePath), Path.GetFileName(sourcePath),
                true, attributes: sourceAttributes);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);

            var destinationAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden;
            var destinationFileInfo = new FileInformation(
                Path.GetDirectoryName(destinationPath), Path.GetFileName(destinationPath),
                true, attributes: destinationAttributes);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);

            IExcludeRule testee = new ExcludeIfDestinationFileReadOnly(_mockRuntime.Filesystem.Object);
            var actual = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Does_not_exclude_if_destination_is_not_read_only()
        {
            var sourcePath = @"C:\Users\Somebody\Documents\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\Somebody\Documents\Budget.xls";

            var sourceAttributes = FileAttributes.ReadOnly;
            var sourceFileInfo = new FileInformation(
                Path.GetDirectoryName(sourcePath), Path.GetFileName(sourcePath),
                true, attributes: sourceAttributes);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);

            var destinationAttributes = FileAttributes.Normal;
            var destinationFileInfo = new FileInformation(
                Path.GetDirectoryName(destinationPath), Path.GetFileName(destinationPath),
                true, attributes: destinationAttributes);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);

            IExcludeRule testee = new ExcludeIfDestinationFileReadOnly(_mockRuntime.Filesystem.Object);
            var actual = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsFalse(actual);
        }
    }
}
