using JPC.Common;
using JPC.Common.Testing;
using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class BackupFileOperationsTests
    {
        private MockRuntime _mockRuntime;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRuntime = new MockRuntime();
        }

        [TestMethod]
        public async Task AfterCopy_resets_archive_bit()
        {
            var directoryPath = @"C:\Users\SomebodyImportant\Documents";
            var fileName = "Budget.xls";
            var filePath = Path.Combine(directoryPath, fileName);
            var attributes = FileAttributes.Archive | FileAttributes.ReadOnly 
                | FileAttributes.Hidden;
            var fileInfo = new FileInformation(directoryPath, fileName, true,
                attributes: attributes, 
                created: DateTimeOffset.Now.Subtract(TimeSpan.FromDays(180)));
            _mockRuntime.Filesystem.FileExists(fileInfo);
            var setFileInformationCalls = new List<FileInformation>();
            _mockRuntime.Filesystem.OnSetFileInformation(fileInfo => setFileInformationCalls.Add(fileInfo));

            IBackupFileOperations testee = CreateTestee();
            await testee.AfterCopyAsync(filePath, "not-used");

            Assert.AreEqual(1, setFileInformationCalls.Count);
            Assert.AreEqual(fileInfo.DirectoryPath, setFileInformationCalls[0].DirectoryPath);
            Assert.AreEqual(fileInfo.Name, setFileInformationCalls[0].Name);
            Assert.AreEqual(FileAttributes.ReadOnly, setFileInformationCalls[0].Attributes & FileAttributes.ReadOnly);
            Assert.AreEqual(FileAttributes.Hidden, setFileInformationCalls[0].Attributes & FileAttributes.Hidden);
            Assert.AreEqual((FileAttributes)0, setFileInformationCalls[0].Attributes & FileAttributes.Archive);
        }

        [TestMethod]
        public async Task AfterCopy_does_not_reset_archive_bit()
        {
            var directoryPath = @"C:\Users\SomebodyImportant\Documents";
            var fileName = "Budget.xls";
            var filePath = Path.Combine(directoryPath, fileName);
            var attributes = FileAttributes.Archive | FileAttributes.ReadOnly
                | FileAttributes.Hidden;
            var fileInfo = new FileInformation(directoryPath, fileName, true,
                attributes: attributes,
                created: DateTimeOffset.Now.Subtract(TimeSpan.FromDays(180)));
            _mockRuntime.Filesystem.FileExists(fileInfo);
            var setFileInformationCalls = new List<FileInformation>();
            _mockRuntime.Filesystem.OnSetFileInformation(fileInfo => setFileInformationCalls.Add(fileInfo));

            IBackupFileOperations testee = CreateTestee(false);
            await testee.AfterCopyAsync(filePath, "not-used");

            Assert.AreEqual(0, setFileInformationCalls.Count);
        }

        [TestMethod]
        public async Task Copy_copies_file()
        {
            var sourceDirectoryPath = @"C:\Users\SomebodyImportant\Documents";
            var fileName = "Budget.xls";
            var sourceFilePath = Path.Combine(sourceDirectoryPath, fileName);
            var destinationDirectoryPath = @"D:\Backup\C\Users\SomebodyImportant\Documents";
            var destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);

            IBackupFileOperations testee = CreateTestee();
            await testee.CopyAsync(sourceFilePath, destinationFilePath);

            _mockRuntime.Filesystem.VerifyFileCopied(sourceFilePath, destinationFilePath, true);
        }

        [TestMethod]
        public async Task EnsureDirectoryExists_creates_directory()
        {
            var sourceDirectoryPath = @"C:\Users\SomebodyImportant\Documents";
            var sourceDirectoryInfo = new DirectoryInformation(sourceDirectoryPath,
                false, true);
            _mockRuntime.Filesystem.DirectoryExists(sourceDirectoryInfo);

            IBackupFileOperations testee = CreateTestee();
            await testee.EnsureDirectoryExistsAsync(sourceDirectoryPath);

            _mockRuntime.Filesystem.VerifyDirectoryCreated(sourceDirectoryPath);
        }

        [TestMethod]
        public async Task EnsureDirectoryExists_does_not_create_directory()
        {
            var sourceDirectoryPath = @"C:\Users\SomebodyImportant\Documents";
            var sourceDirectoryInfo = new DirectoryInformation(sourceDirectoryPath,
                true, true);
            _mockRuntime.Filesystem.DirectoryExists(sourceDirectoryInfo);

            IBackupFileOperations testee = CreateTestee();
            await testee.EnsureDirectoryExistsAsync(sourceDirectoryPath);

            _mockRuntime.Filesystem.VerifyDirectoryNotCreated(sourceDirectoryPath);
        }


        private IBackupFileOperations CreateTestee(bool resetArchiveBit = true)
            =>  new BackupFileOperations(_mockRuntime, resetArchiveBit);
            
    }
}
