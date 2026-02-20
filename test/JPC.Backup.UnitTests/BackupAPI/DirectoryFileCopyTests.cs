using JPC.Common;
using JPC.Common.Testing;
using Moq;

namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class DirectoryFileCopyTests
    {
        private MockFileComparer _mockFileComparer;
        private MockRuntime _mockRuntime;
        private MockBackupFileOperations _mockBackupFileOperations;
        private IEnumerable<MockExcludeRule> _mockExcludeRules;
        private MockBackupEvents _mockBackupEvents;
        private BackupOptions _defaultBackupOptions;
        private string _defaultSourceDirectoryPath;
        private string _defaultSourceFilePath;
        private string _defaultDestinationDirectoryPath;
        private string _defaultDestinationFilePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFileComparer = new MockFileComparer();
            _mockRuntime = new MockRuntime();
            _mockRuntime.Filesystem.SplitPathDelegates();
            _mockRuntime.Filesystem.CombinePathDelegates();
            _mockBackupFileOperations = new MockBackupFileOperations();
            _mockExcludeRules = Enumerable.Range(0, 4).Select(i => new MockExcludeRule()).ToArray();
            _mockBackupEvents = new MockBackupEvents();
            _defaultBackupOptions = TestBackupOptions.Create();
            _defaultSourceDirectoryPath = @"C:\Users\You\Documents";
            _defaultSourceFilePath = @"C:\Users\You\Documents\Budget.xls";
            _defaultDestinationDirectoryPath = @"D:\Backup\C\Users\You\Documents";
            _defaultDestinationFilePath = @"D:\Backup\C\Users\You\Documents\Budget.xls";
        }

        [TestMethod]
        public async Task If_no_source_copy_not_called()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, false);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileNotCopied(_defaultSourceFilePath,
                _defaultDestinationFilePath);
        }

        [TestMethod]
        public async Task If_no_destination_exclude_rules_are_called()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, false);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            foreach (var rule in _mockExcludeRules)
            {
                rule.VerifyExcludeObjectCalled(_defaultSourceFilePath, _defaultDestinationFilePath);
            }
        }

        [TestMethod]
        public async Task If_any_exclude_rule_rejects_copy_not_called()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, false);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockExcludeRules.ElementAt(3).ExcludeObjectReturns(true);

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileNotCopied(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);
            _mockBackupFileOperations.VerifyAfterCopyNotCalled(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);
        }

        [TestMethod]
        public async Task If_files_identical_copy_not_called()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, false);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath, 
                _defaultDestinationFilePath, false);

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileNotCopied(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);
            _mockBackupFileOperations.VerifyAfterCopyNotCalled(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);
        }

        [TestMethod]
        public async Task If_copy_fails_after_copy_not_called()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, false);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);
            _mockBackupFileOperations.CopyAsyncThrows(_defaultSourceFilePath,
                _defaultDestinationFilePath, new AccessViolationException());

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath, _defaultDestinationFilePath);
            _mockBackupFileOperations.VerifyAfterCopyNotCalled(_defaultSourceFilePath, _defaultDestinationFilePath);
        }

        [TestMethod]
        public async Task File_copied()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath, _defaultDestinationFilePath);
            _mockBackupFileOperations.VerifyAfterCopyCalled(_defaultSourceFilePath, _defaultDestinationFilePath);
        }

        [TestMethod]
        public async Task File_fails_on_first_attempt()
        {
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);
            _mockBackupFileOperations.CopyAsyncThrows(_defaultSourceFilePath, 
                _defaultDestinationFilePath, new IOException("Access denied"));

            var testee = CreateTestee();
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockBackupEvents.VerifyFileFailedCalled(_defaultSourceFilePath, 
                _defaultDestinationFilePath);            
        }

        [TestMethod]
        public async Task File_retried_once()
        {
            var options = new BackupOptions(_defaultBackupOptions.CopySystemFiles,
                _defaultBackupOptions.MaxFileSize, _defaultBackupOptions.ComparisonMethod,
                _defaultBackupOptions.FileExcludeExpressions,
                _defaultBackupOptions.DirectoryStopOnColon,
                _defaultBackupOptions.DirectoryStopExpressions,
                _defaultBackupOptions.ResetArchiveBit,
                _defaultBackupOptions.OverwriteReadOnlyFiles, _defaultBackupOptions.MaxDepth,
                1, TimeSpan.FromSeconds(1), _defaultBackupOptions.WhatIf);
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);
            _mockBackupFileOperations.CopyAsyncThrows(_defaultSourceFilePath,
                _defaultDestinationFilePath, new IOException("Access denied"));

            var testee = CreateTestee(customBackupOptions: options);
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath,
                _defaultDestinationFilePath, 2);
            _mockBackupEvents.VerifyFileTransientFailureCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockBackupEvents.VerifyFileFailedCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockRuntime.Clock.VerifySleepAsyncCalled();
        }

        [TestMethod]
        public async Task File_retried_twice()
        {
            var options = new BackupOptions(_defaultBackupOptions.CopySystemFiles,
                _defaultBackupOptions.MaxFileSize, _defaultBackupOptions.ComparisonMethod,
                _defaultBackupOptions.FileExcludeExpressions,
                _defaultBackupOptions.DirectoryStopOnColon,
                _defaultBackupOptions.DirectoryStopExpressions,
                _defaultBackupOptions.ResetArchiveBit,
                _defaultBackupOptions.OverwriteReadOnlyFiles, _defaultBackupOptions.MaxDepth,
                2, TimeSpan.FromSeconds(1), _defaultBackupOptions.WhatIf);
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);
            _mockBackupFileOperations.CopyAsyncThrows(_defaultSourceFilePath,
                _defaultDestinationFilePath, new IOException("Access denied"));

            var testee = CreateTestee(customBackupOptions: options);
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath,
                _defaultDestinationFilePath, 3);
            _mockBackupEvents.VerifyFileTransientFailureCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockBackupEvents.VerifyFileFailedCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockRuntime.Clock.VerifySleepAsyncCalled(2);
        }

        [TestMethod]
        public async Task File_retried_and_succeeds()
        {
            var options = new BackupOptions(_defaultBackupOptions.CopySystemFiles,
                _defaultBackupOptions.MaxFileSize, _defaultBackupOptions.ComparisonMethod,
                _defaultBackupOptions.FileExcludeExpressions,
                _defaultBackupOptions.DirectoryStopOnColon,
                _defaultBackupOptions.DirectoryStopExpressions,
                _defaultBackupOptions.ResetArchiveBit,
                _defaultBackupOptions.OverwriteReadOnlyFiles, _defaultBackupOptions.MaxDepth,
                2, TimeSpan.FromSeconds(1), _defaultBackupOptions.WhatIf);
            var sourceFileInfo = BuildFileInformation(_defaultSourceFilePath, true);
            _mockRuntime.Filesystem.FileExists(sourceFileInfo);
            var destinationFileInfo = BuildFileInformation(_defaultDestinationFilePath, true);
            _mockRuntime.Filesystem.FileExists(destinationFileInfo);
            _mockBackupFileOperations.EnumerateFilesReturns(new string[] { _defaultSourceFilePath });
            _mockFileComparer.ShouldCopyReturns(_defaultSourceFilePath,
                _defaultDestinationFilePath, true);
            _mockBackupFileOperations.CopyAsyncThrowsOnceThenSucceeds(_defaultSourceFilePath,
                _defaultDestinationFilePath, new IOException("Access denied"));

            var testee = CreateTestee(customBackupOptions: options);
            await testee.CopyFilesAsync(_defaultSourceDirectoryPath, _defaultDestinationDirectoryPath);

            _mockBackupFileOperations.VerifyFileCopied(_defaultSourceFilePath,
                _defaultDestinationFilePath, 2);
            _mockBackupEvents.VerifyFileTransientFailureCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockBackupEvents.VerifyFileFailedNotCalled(_defaultSourceFilePath,
                _defaultDestinationFilePath);
            _mockRuntime.Clock.VerifySleepAsyncCalled();
        }


        private IDirectoryFileCopy CreateTestee(BackupOptions customBackupOptions = null)
        {
            var excludeRulesConverted = _mockExcludeRules.Select(r => r.Object).ToArray();
            var effectiveBackupOptions = customBackupOptions ?? _defaultBackupOptions;
            return new DirectoryFileCopy(_mockFileComparer.Object, excludeRulesConverted,
                _mockBackupFileOperations.Object, _mockRuntime, _mockBackupEvents.Object,
                effectiveBackupOptions);
        }

        private FileInformation BuildFileInformation(string filePath, bool exists)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            return new FileInformation(directoryPath, fileName, exists);
        }
    }
}
