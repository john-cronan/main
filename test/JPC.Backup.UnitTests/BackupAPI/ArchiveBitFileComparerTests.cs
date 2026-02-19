using JPC.Common;
using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class ArchiveBitFileComparerTests
    {
        private MockFilesystem _mockFilesystem;
        private string _fileName;
        private string _sourceFileDirectory;
        private string _sourceFilePath;
        private string _destinationFileDirectory;
        private string _destinationFilePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFilesystem = new MockFilesystem();
            _fileName = "Budget.xls";
            _sourceFileDirectory = @"C:\Users\You\Documents";
            _sourceFilePath = Path.Combine(_sourceFileDirectory, _fileName);
            _destinationFileDirectory = @"D:\Backup\C\Users\You\Documents";
            _destinationFilePath = Path.Combine(_destinationFileDirectory, _fileName);
        }

        [TestMethod]
        public void Returns_true_if_archive_bit_set()
        {
            var sourceFileInfo = new FileInformation(_sourceFileDirectory, _fileName, true,
                attributes: FileAttributes.Archive | FileAttributes.ReadOnly);
            _mockFilesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = new FileInformation(_destinationFileDirectory,
                _fileName, true);
            _mockFilesystem.FileExists(destinationFileInfo);

            IFileComparer testee = new ArchiveBitFileComparer(_mockFilesystem.Object);

            Assert.IsTrue(testee.ShouldCopy(_sourceFilePath, _destinationFilePath));
        }

        [TestMethod]
        public void Returns_false_if_archive_bit_not_set_and_destination_exists()
        {
            var sourceFileInfo = new FileInformation(_sourceFileDirectory, _fileName, true);
            _mockFilesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = new FileInformation(_destinationFileDirectory,
                _fileName, true);
            _mockFilesystem.FileExists(destinationFileInfo);

            IFileComparer testee = new ArchiveBitFileComparer(_mockFilesystem.Object);

            Assert.IsFalse(testee.ShouldCopy(_sourceFilePath, _destinationFilePath));
        }

        [TestMethod]
        public void Returns_false_if_archive_bit_not_set_and_no_destination()
        {
            var sourceFileInfo = new FileInformation(_sourceFileDirectory, _fileName, true);
            _mockFilesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = new FileInformation(_destinationFileDirectory,
                _fileName, false);
            _mockFilesystem.FileExists(destinationFileInfo);

            IFileComparer testee = new ArchiveBitFileComparer(_mockFilesystem.Object);

            Assert.IsFalse(testee.ShouldCopy(_sourceFilePath, _destinationFilePath));
        }
    }
}
