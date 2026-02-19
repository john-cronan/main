using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class SourceDirectoryWalkerTests
    {
        private MockRuntime _mockRuntime;
        private MockBackupEvents _mockBackupEvents;
        private IList<IExcludeRule> _stopRules;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRuntime = new MockRuntime();
            _mockRuntime.Filesystem.GetFileNameDelegates();
            _mockBackupEvents = new MockBackupEvents();
            _stopRules = new List<IExcludeRule>();
        }

        [TestMethod]
        public void Enumerates_starting_path()
        {
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents",
                "Pictures", "Downloads", "Music");

            var testee = CreateTestee();
            var actual = testee.Enumerate(@"D:\My Documents",
                TestBackupOptions.Create());

            Assert.IsTrue(actual.Any(d => d.Path.Equals(@"D:\My Documents", StringComparison.InvariantCultureIgnoreCase)));
        }

        [TestMethod]
        public void Enumerates_subdirectories()
        {
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents",
                "Pictures", "Music");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Pictures",
                "Christmas 2024", "Vacation to Aruba");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Music",
                "Classic Rock", "Jazz", "Classical");

            var testee = CreateTestee();
            var actual = testee.Enumerate(@"D:\My Documents",
                TestBackupOptions.Create());

            Assert.AreEqual(8, actual.Count());
            Assert.AreEqual(8, actual.Select(d => d.Path).Distinct().Count());
        }

        [TestMethod]
        public void Enumerates_only_root()
        {
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents",
                "Pictures", "Music");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Pictures",
                "Christmas 2024", "Vacation to Aruba");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Music",
                "Classic Rock", "Jazz", "Classical");

            var options = TestBackupOptions.Create(maxDepth: 0);
            var testee = CreateTestee();
            var actual = testee.Enumerate(@"D:\My Documents", options);

            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(@"D:\My Documents", actual.First().Path);
        }

        [TestMethod]
        public void Enumerates_first_level_subdirectories()
        {
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents",
                "Pictures", "Music");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Pictures",
                "Christmas 2024", "Vacation to Aruba");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Music",
                "Classic Rock", "Jazz", "Classical");

            var options = TestBackupOptions.Create(maxDepth: 1);
            var testee = CreateTestee();
            var actual = testee.Enumerate(@"D:\My Documents", options);

            Assert.AreEqual(3, actual.Count());
            Assert.AreEqual(3, actual.Select(d => d.Path).Distinct().Count());
        }

        [TestMethod]
        public void Stops_when_exclude_rule_returns_true()
        {
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents",
                "Pictures", "Music");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Pictures",
                "Christmas 2024", "Vacation to Aruba");
            _mockRuntime.Filesystem.DirectoryHasSubdirectories(@"D:\My Documents\Music",
                "Classic Rock", "Jazz", "Classical");
            var mockExcludeRule = new MockExcludeRule();
            mockExcludeRule.ExcludeObjectReturns(@"D:\My Documents\Pictures", true);
            _stopRules.Add(mockExcludeRule.Object);

            var testee = CreateTestee();
            var actual = testee.Enumerate(@"D:\My Documents",
                TestBackupOptions.Create())
                    .Select(d => d.Path);
            
            Assert.AreEqual(5, actual.Count());
            Assert.IsFalse(actual.Any(d => d.Contains("Pictures")));
            Assert.IsFalse(actual.Any(d => d.Contains("Aruba")));
        }

        private ISourceDirectoryWalker CreateTestee()
        {
            return new SourceDirectoryWalker(_stopRules, _mockRuntime, 
                _mockBackupEvents.Object);
        }
    }
}
