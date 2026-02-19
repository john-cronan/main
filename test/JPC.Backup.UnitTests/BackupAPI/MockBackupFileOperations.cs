using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    internal class MockBackupFileOperations : Mock<IBackupFileOperations>
    {
        public MockBackupFileOperations()
        {
            Setup(m => m.CopyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            Setup(m => m.AfterCopyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        }

        public void CopyAsyncThrows(string source, string destination, Exception ex)
            => Setup(m => m.CopyAsync(source, destination)).Throws(ex);

        public void CopyAsyncThrowsOnceThenSucceeds(string source, string destination, 
            Exception ex) => SetupSequence(m => m.CopyAsync(source, destination)).Throws(ex)
                .Returns(Task.CompletedTask);

        public void EnumerateFilesReturns(IEnumerable<string> filePaths)
            => Setup(m => m.EnumerateFiles(It.IsAny<string>())).Returns(filePaths);

        public void VerifyAfterCopyCalled(string source, string destination)
            => Verify(m => m.AfterCopyAsync(source, destination), Times.Once());

        public void VerifyAfterCopyNotCalled(string source, string destination)
            => Verify(m => m.AfterCopyAsync(source, destination), Times.Never());

        public void VerifyEnsureDirectoryExistsCalled(string directoryPath)
            => Verify(m => m.EnsureDirectoryExistsAsync(directoryPath), Times.Once());

        public void VerifyFileCopied(string source, string destination)
            => Verify(m => m.CopyAsync(source, destination), Times.Once());

        public void VerifyFileCopied(string source, string destination, int attempts)
            => Verify(m => m.CopyAsync(source, destination), Times.Exactly(attempts));

        public void VerifyFileNotCopied(string source, string destination)
            => Verify(m => m.CopyAsync(source, destination), Times.Never());
    }
}
