using JPC.Common;
using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class LastWriteTimeDifferentFileComparerTests
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
        public void Returns_true_if_times_are_different()
        {
            var sourceFileInfo = new FileInformation(_sourceFileDirectory, _fileName, true,
                lastWrite: DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30)));
            _mockFilesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = new FileInformation(_destinationFileDirectory, 
                _fileName, true, lastWrite: DateTimeOffset.Now);
            _mockFilesystem.FileExists(destinationFileInfo);

            IFileComparer testee = new LastWriteTimeDifferentFileComparer(_mockFilesystem.Object);

            Assert.IsTrue(testee.ShouldCopy(_sourceFilePath, _destinationFilePath));
        }

        [TestMethod]
        public void Returns_false_if_times_are_equal()
        {
            var lastWriteTime = DateTimeOffset.Parse("1/2/2026 15:53");
            var sourceFileInfo = new FileInformation(_sourceFileDirectory, _fileName, true,
                lastWrite: lastWriteTime);
            _mockFilesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = new FileInformation(_destinationFileDirectory,
                _fileName, true, lastWrite: lastWriteTime);
            _mockFilesystem.FileExists(destinationFileInfo);

            IFileComparer testee = new LastWriteTimeDifferentFileComparer(_mockFilesystem.Object);

            Assert.IsFalse(testee.ShouldCopy(_sourceFilePath, _destinationFilePath));
        }
    }
}
