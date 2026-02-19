using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    internal class MockExcludeRule : Mock<IExcludeRule>
    {
        public string FriendlyName 
        {
            get { return Object.FriendlyName; }
            set { Setup(p => p.FriendlyName).Returns(value); }
        }

        public void ExcludeObjectReturns(bool value)
            => Setup(m => m.ExcludeObject(It.IsAny<string>(), It.IsAny<string>())).Returns(value);

        public void ExcludeObjectReturns(string sourcePath, bool value)
            => Setup(m => m.ExcludeObject(sourcePath, It.IsAny<string>())).Returns(value);

        public void ExcludeObjectReturns(string sourcePath, string destinationPath, bool value)
            => Setup(m => m.ExcludeObject(sourcePath, destinationPath)).Returns(value);

        public void VerifyExcludeObjectCalled(string sourcePath, string destinationPath)
            => Verify(m => m.ExcludeObject(sourcePath, destinationPath), Times.Once());
 
        public void VerifyExcludeObjectNotCalled(string sourcePath, string destinationPath)
            => Verify(m => m.ExcludeObject(sourcePath, destinationPath), Times.Never());
    }
}
