using JPC.Common;

namespace JPC.Backup.UnitTests.Backup
{
    [TestClass]
    public class SpecificationFileTests
    {
        [TestMethod]
        public void MaxFileSize_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.MaxFileSize = FileSize.From(10, FileSizeUnits.GB);

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.MaxFileSize, options.MaxFileSize);

            testee.MaxFileSize = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.MaxFileSize, options.MaxFileSize);
        }

        [TestMethod]
        public void CopySystemFiles_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.CopySystemFiles = true;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsTrue(options.CopySystemFiles);

            testee.CopySystemFiles = false;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsFalse(options.CopySystemFiles);

            testee.CopySystemFiles = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsFalse(options.CopySystemFiles);
        }

        [TestMethod]
        public void ComparisonMethod_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.ComparisonMethod = FileComparisonMethod.LastWriteTimeNewer;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.ComparisonMethod, options.ComparisonMethod);

            testee.ComparisonMethod = FileComparisonMethod.SizeDifferent;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.ComparisonMethod, options.ComparisonMethod);
        }

        [TestMethod]
        public void OverrwriteReadOnlyFiles_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.OverwriteReadOnlyFiles = true;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.OverwriteReadOnlyFiles, options.OverwriteReadOnlyFiles);

            testee.OverwriteReadOnlyFiles = false;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.OverwriteReadOnlyFiles, options.OverwriteReadOnlyFiles);

            testee.OverwriteReadOnlyFiles = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsTrue(options.OverwriteReadOnlyFiles);
        }

        [TestMethod]
        public void MaxDepth_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.MaxDepth = 4;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.MaxDepth, options.MaxDepth);

            testee.MaxDepth = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsNull(options.MaxDepth);
        }

        [TestMethod]
        public void MaxRetriesOnFailure_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.MaxRetriesOnFailure = 2;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.MaxRetriesOnFailure, options.MaxRetriesOnFailure);

            testee.MaxRetriesOnFailure = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(1, options.MaxRetriesOnFailure);
        }

        [TestMethod]
        public void ResetArchiveBit_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.ResetArchiveBit = true;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.ResetArchiveBit, options.ResetArchiveBit);

            testee.ResetArchiveBit = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsTrue(options.ResetArchiveBit);
        }

        [TestMethod]
        public void RetryDelay_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.RetryDelay = TimeSpan.FromSeconds(2);

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.RetryDelay, options.RetryDelay);

            testee.RetryDelay = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsNull(options.RetryDelay);
        }

        [TestMethod]
        public void WhatIf_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.WhatIf = true;

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.WhatIf, options.WhatIf);

            testee.WhatIf = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.IsFalse(options.WhatIf);
        }

        [TestMethod]
        public void ExcludeFilesMatching_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.ExcludeFilesMatching = new MutableMatchExpression[]
            {
                new MutableMatchExpression { Expression = "(?i)\\.mp3", MatchType = MatchType.RegEx},
                new MutableMatchExpression { Expression = "\"(?i)\\\\.mpeg\"", MatchType = MatchType.RegEx},
            };

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(testee.ExcludeFilesMatching.Count(), options.FileExcludeExpressions.Count());
            var matching = testee.ExcludeFilesMatching.Join(
                    options.FileExcludeExpressions,
                    m => new { m.Expression, m.MatchType },
                    e => new {e.Expression, e.MatchType},
                    (m, e) => new { SpecificationFile = m, Options = e});
            Assert.AreEqual(testee.ExcludeFilesMatching.Count(), matching.Count());

            testee.ExcludeFilesMatching = new MutableMatchExpression[0];
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(0, options.FileExcludeExpressions.Count());

            testee.ExcludeFilesMatching = null;
            options = SpecificationFileHelper.ToBackupOptions(testee);
            Assert.AreEqual(0, options.FileExcludeExpressions.Count());
        }

        [TestMethod]
        public void StopWhenDirectoryMatches_is_carried_through_to_options()
        {
            var testee = new SpecificationFile();
            testee.SourcePath = @"C:\Users\You\Documents";
            testee.DestinationPath = @"D:\Backup\C\Users\You\Documents";
            testee.StopWhenDirectoryMatches = new MutableMatchExpression[]
            {
                new MutableMatchExpression { Expression = @"\\$RECYCLE.BIN", MatchType = MatchType.RegEx },
                new MutableMatchExpression { Expression = @"\\System Volume Information", MatchType = MatchType.RegEx },
            };

            var options = SpecificationFileHelper.ToBackupOptions(testee);
            var matching = testee.StopWhenDirectoryMatches.Join(
                    options.DirectoryStopExpressions,
                    m => new { m.Expression, m.MatchType },
                    e => new { e.Expression, e.MatchType },
                    (m, e) => new { SpecificationFile = m, Options = e });
            Assert.AreEqual(testee.StopWhenDirectoryMatches.Count(), matching.Count());

        }
    }
}
