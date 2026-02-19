using JPC.Common;
using JPC.Common.Testing;
using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class DirectoryFileCopyFactoryTests
    {
        private MockRuntime _mockRuntime;
        private Mock<IBackupEvents> _backupEvents;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRuntime = new MockRuntime();
            _backupEvents = new Mock<IBackupEvents>();
        }

        [TestMethod]

        public void Equal_options_returns_existing_instance()
        {
            var fileExcludes = new MatchExpression[]
            {
                new MatchExpression("x", MatchType.RegEx),
                new MatchExpression("y", MatchType.RegEx),
                new MatchExpression("z", MatchType.RegEx)
            };
            var optionsA = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.SizeDifferent,
                copySystemFiles: false, fileExcludeExpressions: fileExcludes,
                maxFileSize: FileSize.From(10, FileSizeUnits.GB));
            var optionsB = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.SizeDifferent,
                copySystemFiles: false, fileExcludeExpressions: fileExcludes,
                maxFileSize: FileSize.From(10, FileSizeUnits.GB));

            var testee = CreateTestee();
            var instanceA = testee.Create(optionsA, null);
            var instanceB = testee.Create(optionsB, instanceA);

            Assert.IsTrue(object.ReferenceEquals(instanceA, instanceB));
        }

        [TestMethod]
        public void Different_options_returns_new_instance()
        {
            var fileExcludes = new MatchExpression[]
            {
                new MatchExpression("x", MatchType.RegEx),
                new MatchExpression("y", MatchType.RegEx),
                new MatchExpression("z", MatchType.RegEx)
            };
            var backupEvents = new Mock<IBackupEvents>();
            var optionsA = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.SizeDifferent,
                copySystemFiles: false, fileExcludeExpressions: fileExcludes,
                maxFileSize: FileSize.From(10, FileSizeUnits.GB));
            var optionsB = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.SizeDifferent,
                copySystemFiles: false, fileExcludeExpressions: fileExcludes,
                maxFileSize: null);

            var testee = CreateTestee();
            var instanceA = testee.Create(optionsA, null);
            var instanceB = testee.Create(optionsB, instanceA);

            Assert.IsFalse(object.ReferenceEquals(instanceA, instanceB));
        }

        [TestMethod]
        public void Size_comparison_configures_SizeIsDifferentFileComparer()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.SizeDifferent);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).FileComparer is SizeIsDifferentFileComparer);
        }

        [TestMethod]
        public void LastWriteTimeDifferent_comparison_configures_LastWriteTimeDifferentFileComparer()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.LastWriteTimeDifferent);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).FileComparer is LastWriteTimeDifferentFileComparer);
        }

        [TestMethod]
        public void CopySystemFiles_false_configures_exclude_rule()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(copySystemFiles: false);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).ExcludeRules.Any(r => r is ExcludeIfSourceIsSystemObject));
        }

        [TestMethod]
        public void MaxFileSize_configures_exclude_rule()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(
                maxFileSize: FileSize.From(10, FileSizeUnits.GB));

            var testee = CreateTestee();
            var actual = testee.Create(options, null);
            var excludeRule = (actual as DirectoryFileCopy).ExcludeRules
                .OfType<ExcludeIfFileOverSize>()
                .FirstOrDefault();

            Assert.IsNotNull(excludeRule);
            Assert.AreEqual(excludeRule.CriticalSize, FileSize.From(10, FileSizeUnits.GB));
        }

        [TestMethod]
        public void Overwrite_readonly_files_false_configures_exclude_rule()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(overwriteReadOnlyFiles: false);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).ExcludeRules.Any(r => r is ExcludeIfDestinationFileReadOnly));
        }

        [TestMethod]
        public void WhatIf_configures_appropriate_file_operations_object()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(whatIf: true);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).FileOperations is WhatIfBackupFileOperations);
        }

        [TestMethod]
        public void ResetArchiveBit_true_configures_appropriate_file_operations_object()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create();

            var testee = CreateTestee();
            var actual = testee.Create(options, null);
            var fileCopyOperations = (actual as DirectoryFileCopy).FileOperations as BackupFileOperations;

            Assert.IsNotNull(fileCopyOperations);
            Assert.IsTrue(fileCopyOperations.ResetArchiveBit);
        }

        [TestMethod]
        public void ResetArchiveBit_false_configures_appropriate_file_operations_object()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(resetArchiveBit: false);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);
            var fileCopyOperations = (actual as DirectoryFileCopy).FileOperations as BackupFileOperations;

            Assert.IsNotNull(fileCopyOperations);
            Assert.IsFalse(fileCopyOperations.ResetArchiveBit);
        }

        [TestMethod]
        public void LastWriteTimeNewer_comparison_configures_LastWriteTimeNewerFileComparer()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.LastWriteTimeNewer);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).FileComparer is LastWriteTimeNewerFileComparer);
        }

        [TestMethod]
        public void ArchiveBit_comparison_configures_ArchiveBitFileComparer()
        {
            var backupEvents = new Mock<IBackupEvents>();
            var options = TestBackupOptions.Create(
                comparisonMethod: FileComparisonMethod.ArchiveBit);

            var testee = CreateTestee();
            var actual = testee.Create(options, null);

            Assert.IsTrue((actual as DirectoryFileCopy).FileComparer is ArchiveBitFileComparer);
        }

        private IDirectoryFileCopyFactory CreateTestee()
            => new DirectoryFileCopyFactory(_mockRuntime, _backupEvents.Object);
    }
}
