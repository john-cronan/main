using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class PropertyBinderStringsUnitTests
    {
        [TestMethod]
        public void Binds_strings_to_ImmutableArray()
        {
            var result = ArrangeAndAct<TargetStringsToImmutableArray>();
            Assert.AreEqual(result.Command, "delete");
            Assert.AreEqual(2, result.Directories.Length);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_string_array()
        {
            var result = ArrangeAndAct<TargetStringsToImmutableArray>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Length);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_string_list()
        {
            var result = ArrangeAndAct<TargetStringsToStringList>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_IList_of_strings()
        {
            var result = ArrangeAndAct<TargetStringsToIListOfString>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_IList()
        {
            var result = ArrangeAndAct<TargetStringsToIList>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_ICollection_of_strings()
        {
            var result = ArrangeAndAct<TargetStringsToICollectionOfStrings>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }

        [TestMethod]
        public void Binds_strings_to_IEnumerable_of_strings()
        {
            var result = ArrangeAndAct<TargetStringsToIEnumerableOfString>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }

        [TestMethod]
        public void Binds_strings_to_IReadOnlyCollection_of_strings()
        {
            var result = ArrangeAndAct<TargetStringsToIReadOnlyCollectionOfString>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }

        private T ArrangeAndAct<T>()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Command", "delete")
                    .AddArgument("Directories", @"%TEMP%\Program.exe", @"%TEMP%\Program_exe")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(ImmutableArray<string>.Empty.Add("Command"),
                    ArgumentMultiplicity.One, true),
                new Argument(ImmutableArray<string>.Empty.Add("Directories"),
                    ArgumentMultiplicity.OneOrMore, false),
                new Argument(ImmutableArray<string>.Empty.Add("Files"),
                    ArgumentMultiplicity.OneOrMore, false, ArgumentFlags.ExistingFile)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);
            IObjectBinder testee = new PropertyBinder(filesystemMock.Object);
            var instance = testee.CreateObject<T>(resolution);
            return instance;
        }

        private class TargetStringsToImmutableArray
        {
            public string Command { get; set; }
            public ImmutableArray<string> Directories { get; set; }
        }

        private class TargetStringsToStringArray
        {
            public string Command { get; set; }
            public string[] Directories { get; set; }
        }

        private class TargetStringsToStringList
        {
            public string Command { get; set; }
            public List<string> Directories { get; set; }
        }

        private class TargetStringsToIListOfString
        {
            public string Command { get; set; }
            public IList<string> Directories { get; set; }
        }

        private class TargetStringsToIList
        {
            public string Command { get; set; }
            public IList Directories { get; set; }
        }

        private class TargetStringsToICollectionOfStrings
        {
            public string Command { get; set; }
            public ICollection<string> Directories { get; set; }
        }

        private class TargetStringsToIEnumerableOfString
        {
            public string Command { get; set; }
            public IEnumerable<string> Directories { get; set; }
        }

        private class TargetStringsToIReadOnlyCollectionOfString
        {
            public string Command { get; set; }
            public IReadOnlyCollection<string> Directories { get; set; }
        }
    }
}
