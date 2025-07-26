using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineParseResultsUnitTests
    {
        private static readonly ParseModel Model = new ParseModel(
            new Argument[]
            {
                new Argument("Directory", ArgumentMultiplicity.One, false, ArgumentFlags.None),
                new Argument("Files", ArgumentMultiplicity.OneOrMore, false, ArgumentFlags.None),
                new Argument("OlderThan", ArgumentMultiplicity.One, false, ArgumentFlags.None),
                new Argument("Passes", ArgumentMultiplicity.OneOrMore, false, ArgumentFlags.None),
                new Argument("FileList", ArgumentMultiplicity.OneOrMore, false, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent),
                new Argument("Recurse", ArgumentMultiplicity.Zero, false, ArgumentFlags.None)
            }.ToImmutableArray(),
            "-/".ToImmutableArray(),
            false, NameMatchingOptions.Stem, true, '@');

        [TestMethod]
        public void Returns_single_argument_value()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("Directory", @"C:\Temp\Us")
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValue("Directory");
            Assert.IsNotNull(result);
            Assert.AreEqual(@"C:\Temp\Us", result);
        }

        [TestMethod]
        public void Returns_single_DateTime_value()
        {
            var datetimeValue = DateTime.Now - TimeSpan.FromDays(10);
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("OlderThan", datetimeValue.ToString())
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValueAs<DateTime>("OlderThan");

            //
            //  The ToString() above truncates the milliseconds, so the
            //  two dates won't be exactly equal.
            var delta = (datetimeValue - result).TotalMilliseconds;
            Assert.IsTrue(delta < 1000);
        }

        [TestMethod]
        public void Returns_multiple_string_values()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("Files", "FileA.txt", "FileB.docx")
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValues("Files");
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("FileA.txt", result.ElementAt(0));
            Assert.AreEqual("FileB.docx", result.ElementAt(1));
        }

        [TestMethod]
        public void Returns_multiple_int_values()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("Files", "FileA.txt", "FileB.docx")
                    .AddArgument("Passes", "3", "7")
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValuesAs<int>("Passes");
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(3, result.ElementAt(0));
            Assert.AreEqual(7, result.ElementAt(1));
        }

        [TestMethod]
        public void Returns_file_content_as_sequence_of_strings()
        {
            var fileName = "Files.txt";
            var fileContent = new string[]
            {
                "FileA.txt", "FileB.docx", "FileC.dll"
            };
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("FileList", fileName)
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == fileName))).Returns(fileContent);
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValuesAs<string>("FileList");
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(fileContent[0], result.ElementAt(0));
            Assert.AreEqual(fileContent[1], result.ElementAt(1));
            Assert.AreEqual(fileContent[2], result.ElementAt(2));
        }

        [TestMethod]
        public void Returns_file_content_as_string()
        {
            var fileName = "Files.txt";
            var fileContent = $"FileA.txt{Environment.NewLine}FileB.docx{Environment.NewLine}FileC.dll";
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("FileList", fileName)
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllText(It.Is<string>(s => s == fileName))).Returns(fileContent);
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.GetValueAs<string>("FileList");
            Assert.IsNotNull(result);
            Assert.AreEqual(fileContent, result);
        }

        [TestMethod]
        public void Returns_flag_is_present()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("SecureDelete.exe")
                    .AddArgument("Directory", @"C:\Temp\Us")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var resolutions = new ActualModelResolution(actuals, Model);
            var binder = new PropertyBinder();
            var filesystem = new Mock<IFilesystem>();
            ICommandLineParseResults testee = new CommandLineParseResults(binder,
                resolutions, filesystem.Object);
            var result = testee.IsPresent("Recurse");
            Assert.IsTrue(result);
            result = testee.IsPresent("Recycle");
            Assert.IsFalse(result);
        }
    }
}
