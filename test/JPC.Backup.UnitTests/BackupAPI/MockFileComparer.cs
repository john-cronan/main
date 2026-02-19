using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    internal class MockFileComparer : Mock<IFileComparer>
    {
        public void ShouldCopyReturns(string sourcePath, string destinationPath, bool value)
            => Setup(m => m.ShouldCopy(sourcePath, destinationPath)).Returns(value);

        public void VerifyShouldCopyNotCalled()
            => Verify(m => m.ShouldCopy(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

        public void VerifyShouldCopyNotCalled(string sourcePath, string destinationPath)
            => Verify(m => m.ShouldCopy(sourcePath, destinationPath), Times.Never());

    }
}
