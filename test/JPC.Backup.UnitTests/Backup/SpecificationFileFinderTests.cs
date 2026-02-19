using JPC.Common.Testing;

namespace JPC.Backup.UnitTests.Backup
{
    [TestClass]
    public class SpecificationFileFinderTests
    {
        [TestMethod]
        public void Finds_file_on_command_line()
        {
            var expectedSpecFile = new SpecificationFile
            {
                SourcePath = "D:\\",
                DestinationPath = "E:\\",
                ExcludeFilesMatching = new MutableMatchExpression[]
                {
                    new MutableMatchExpression { Expression = "xxx", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "yyy", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "zzz", MatchType = MatchType.RegEx }
                }
            };
            var specFileJson = SpecificationFile.ToJson(expectedSpecFile);
            var mockRuntime = new MockRuntime();
            mockRuntime.Environment.CommandLineArgs = new string[]
            {
                "Backup.exe",
                "Backup-OnCommandLine.json"
            };
            mockRuntime.Filesystem.FileHasContent("Backup-OnCommandLine.json", specFileJson);
            var testee = new SpecificationFileFinder(mockRuntime);
            testee.Find();

            Assert.AreEqual(expectedSpecFile.SourcePath, testee.FoundFile.SourcePath);
            Assert.AreEqual(expectedSpecFile.DestinationPath, testee.FoundFile.DestinationPath);
            var matching =
                expectedSpecFile.ExcludeFilesMatching.Join(
                    testee.FoundFile.ExcludeFilesMatching,
                    o => new { o.Expression, o.MatchType },
                    i => new { i.Expression, i.MatchType },
                    (o, i) => new { Outer = o, Inner = i });
            Assert.AreEqual(expectedSpecFile.ExcludeFilesMatching.Count(), matching.Count());
        }

        [TestMethod]
        public void Finds_file_in_current_directory()
        {
            var expectedSpecFile = new SpecificationFile
            {
                SourcePath = "D:\\",
                DestinationPath = "E:\\",
                ExcludeFilesMatching = new MutableMatchExpression[]
                {
                    new MutableMatchExpression { Expression = "xxx", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "yyy", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "zzz", MatchType = MatchType.RegEx }
                }
            };
            var specFileJson = SpecificationFile.ToJson(expectedSpecFile);
            var mockRuntime = new MockRuntime();
            mockRuntime.Environment.CommandLineArgs = new string[]
            {
                "Backup.exe"
            };
            mockRuntime.Filesystem.CombinePathDelegates();
            mockRuntime.Filesystem.SetCurrentDirectory("D:\\");
            mockRuntime.Filesystem.FileHasContent("D:\\Backup.json", specFileJson);
            var testee = new SpecificationFileFinder(mockRuntime);
            testee.Find();

            Assert.AreEqual(expectedSpecFile.SourcePath, testee.FoundFile.SourcePath);
            Assert.AreEqual(expectedSpecFile.DestinationPath, testee.FoundFile.DestinationPath);
            var matching = 
                expectedSpecFile.ExcludeFilesMatching.Join(
                    testee.FoundFile.ExcludeFilesMatching,
                    o => new { o.Expression, o.MatchType },
                    i => new { i.Expression, i.MatchType},
                    (o, i) => new { Outer = o, Inner = i});
            Assert.AreEqual(expectedSpecFile.ExcludeFilesMatching.Count(), matching.Count());
        }

        [TestMethod]
        public void File_on_command_line_takes_precedence()
        {
            var specFileOnCommandLine = new SpecificationFile
            {
                SourcePath = "D:\\",
                DestinationPath = "E:\\",
                ExcludeFilesMatching = new MutableMatchExpression[]
                {
                    new MutableMatchExpression { Expression = "xxx", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "yyy", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "zzz", MatchType = MatchType.RegEx }
                }
            };
            var mockRuntime = new MockRuntime();
            mockRuntime.Filesystem.CombinePathDelegates();
            mockRuntime.Environment.CommandLineArgs = new string[]
            {
                "Backup.exe",
                "Backup-OnCommandLine.json"
            };
            mockRuntime.Filesystem.FileHasContent("Backup-OnCommandLine.json", 
                SpecificationFile.ToJson(specFileOnCommandLine));

            var specFileInCurrentDirectory = new SpecificationFile
            {
                SourcePath = "D:\\",
                DestinationPath = "E:\\",
                ExcludeFilesMatching = new MutableMatchExpression[]
                {
                    new MutableMatchExpression { Expression = "aaa", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "bbb", MatchType = MatchType.RegEx },
                    new MutableMatchExpression { Expression = "ccc", MatchType = MatchType.RegEx }
                }
            };
            mockRuntime.Filesystem.SetCurrentDirectory("D:\\");
            mockRuntime.Filesystem.FileHasContent("D:\\Backup.json", SpecificationFile.ToJson(specFileInCurrentDirectory));

            var testee = new SpecificationFileFinder(mockRuntime);
            testee.Find();

            var actualVsCommandLineMatches =
                testee.FoundFile.ExcludeFilesMatching.Join(
                    specFileOnCommandLine.ExcludeFilesMatching,
                    o => new { o.Expression, o.MatchType },
                    i => new { i.Expression, i.MatchType },
                    (o, i) => new { Outer = o, Inner = i});
            Assert.AreEqual(testee.FoundFile.ExcludeFilesMatching.Count(), actualVsCommandLineMatches.Count());

            var actualVsCurrentDirectory =
                testee.FoundFile.ExcludeFilesMatching.Join(
                    specFileInCurrentDirectory.ExcludeFilesMatching,
                    o => new { o.Expression, o.MatchType },
                    i => new { i.Expression, i.MatchType },
                    (o, i) => new { Outer = o, Inner = i });
            Assert.AreNotEqual(testee.FoundFile.ExcludeFilesMatching.Count(), actualVsCurrentDirectory.Count());
        }
    }
}
