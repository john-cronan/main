using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class PathUtilityTests
    {
        [TestMethod]
        public void ComputeDestinationPath_computes_correct_path()
        {
            var sourceRoot = @"C:\Windows";
            var destinationRoot = @"D:\Backup\C";
            var sourceSubDirectory = @"C:\Windows\System32\drivers";
            var expectedDestinationPath = @"D:\Backup\C\System32\drivers";

            var runtime = new MockRuntime();
            runtime.Filesystem.CombinePathDelegates();
            runtime.Filesystem.SplitPathDelegates();
            var actualDestinationPath = PathUtility.ComputeDestinationPath(
                runtime.Filesystem.Object, sourceRoot, destinationRoot, sourceSubDirectory);

            Assert.AreEqual(expectedDestinationPath, actualDestinationPath);
        }

        [TestMethod]
        public void ComputeDestinationPath_tolerates_trailing_separators()
        {
            var sourceRoot = @"C:\Windows\";
            var destinationRoot = @"D:\Backup\C\";
            var sourceSubDirectory = @"C:\Windows\System32\drivers";
            var expectedDestinationPath = @"D:\Backup\C\System32\drivers";

            var runtime = new MockRuntime();
            runtime.Filesystem.CombinePathDelegates();
            runtime.Filesystem.SplitPathDelegates();
            var actualDestinationPath = PathUtility.ComputeDestinationPath(
                runtime.Filesystem.Object, sourceRoot, destinationRoot, sourceSubDirectory);

            Assert.AreEqual(expectedDestinationPath, actualDestinationPath);
        }

        [TestMethod]
        public void ComputeDestinationPath_returns_destination_root()
        {
            var sourceRoot = @"C:\Windows";
            var destinationRoot = @"D:\Backup\C";
            var sourceSubDirectory = sourceRoot;
            var expectedDestinationPath = destinationRoot;

            var runtime = new MockRuntime();
            runtime.Filesystem.CombinePathDelegates();
            runtime.Filesystem.SplitPathDelegates();
            var actualDestinationPath = PathUtility.ComputeDestinationPath(
                runtime.Filesystem.Object, sourceRoot, destinationRoot, sourceSubDirectory);

            Assert.AreEqual(expectedDestinationPath, actualDestinationPath);
        }
    }
}
