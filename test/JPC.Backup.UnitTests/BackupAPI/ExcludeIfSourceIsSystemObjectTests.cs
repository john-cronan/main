using JPC.Common;
using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class ExcludeIfSourceIsSystemObjectTests
    {
        private MockFilesystem _mockFilesystem;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFilesystem = new MockFilesystem();
        }

        [TestMethod]
        public void Excludes_system_file()
        {
            var sourcePath = @"C:\Users\You\SystemFile.dat";
            var destinationPath = @"D:\Backup\C\Users\You\SystemFile.dat";
            var sourceFileInfo = new FileInformation(Path.GetDirectoryName(sourcePath),
                Path.GetFileName(sourcePath), true, FileAttributes.Hidden | FileAttributes.System);
            _mockFilesystem.FileExists(sourceFileInfo);
            _mockFilesystem.DirectoryDoesNotExist(sourcePath);

            var testee = CreateTestee();
            var excluded = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsTrue(excluded);
        }

        [TestMethod]
        public void Does_not_exclude_normal_file()
        {
            var sourcePath = @"C:\Users\You\SystemFile.dat";
            var destinationPath = @"D:\Backup\C\Users\You\SystemFile.dat";
            var sourceFileInfo = new FileInformation(Path.GetDirectoryName(sourcePath),
                Path.GetFileName(sourcePath), true, FileAttributes.Normal);
            _mockFilesystem.FileExists(sourceFileInfo);
            _mockFilesystem.DirectoryDoesNotExist(sourcePath);

            var testee = CreateTestee();
            var excluded = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsFalse(excluded);
        }

        [TestMethod]
        public void Excludes_system_directory()
        {
            var sourcePath = @"C:\Users\You\SystemDirectory";
            var destinationPath = @"D:\Backup\C\Users\You\SystemDirectory";
            var directoryInfo = new DirectoryInformation(sourcePath, true, false,
                attributes: FileAttributes.Hidden | FileAttributes.System);
            _mockFilesystem.FileDoesNotExist(sourcePath);
            _mockFilesystem.DirectoryExists(directoryInfo);

            var testee = CreateTestee();
            var excluded = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsTrue(excluded);
        }

        [TestMethod]
        public void Does_not_exclude_normal_directory()
        {
            var sourcePath = @"C:\Users\You\SystemDirectory";
            var destinationPath = @"D:\Backup\C\Users\You\SystemDirectory";
            var directoryInfo = new DirectoryInformation(sourcePath, true, false,
                attributes: FileAttributes.Normal);
            _mockFilesystem.FileDoesNotExist(sourcePath);
            _mockFilesystem.DirectoryExists(directoryInfo);

            var testee = CreateTestee();
            var excluded = testee.ExcludeObject(sourcePath, destinationPath);

            Assert.IsFalse(excluded);
        }


        private IExcludeRule CreateTestee()
            => new ExcludeIfSourceIsSystemObject(_mockFilesystem.Object);    
    }
}
