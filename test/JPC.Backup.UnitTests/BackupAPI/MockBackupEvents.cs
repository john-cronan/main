using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    internal class MockBackupEvents : Mock<IBackupEvents>
    {
        public void VerifyFileFailedCalled(string sourcePath, string destinationPath)
            => Verify(m => m.FileFailed(sourcePath, destinationPath, It.IsAny<Exception>()),
                Times.Once);

        public void VerifyFileFailedNotCalled(string sourcePath, string destinationPath)
            => Verify(m => m.FileFailed(sourcePath, destinationPath, It.IsAny<Exception>()),
                Times.Never);

        public void VerifyFileTransientFailureCalled(string sourcePath, string destinationPath)
            => Verify(m => m.FileTransientFailure(sourcePath, destinationPath,
                It.IsAny<Exception>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>()),
                Times.Once);
    }
}
