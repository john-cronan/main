using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderStringsUnitTests
    {
        [TestMethod]
        public void Binds_to_scalar()
        {
            var instance = ArrangeAndAct<ScalarTarget>();
            Assert.AreEqual("delete", instance.Command);
        }

        [TestMethod]
        public void Binds_strings_to_string_array()
        {
            var result = ArrangeAndAct<ArrayTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Length);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_string_list()
        {
            var result = ArrangeAndAct<ListTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_immutable_array()
        {
            var result = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_IList_of_string()
        {
            var result = ArrangeAndAct<ListGenericInterfaceTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_IList()
        {
            var result = ArrangeAndAct<ListInterfaceTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories[0]);
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories[1]);
        }

        [TestMethod]
        public void Binds_strings_to_ICollection_of_string()
        {
            var result = ArrangeAndAct<CollectionGenericInterfaceTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count);
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }

        [TestMethod]
        public void Binds_strings_to_IEnumerable_of_string()
        {
            var result = ArrangeAndAct<EnumerableGenericInterfaceTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }

        [TestMethod]
        public void Binds_strings_to_IReadOnlyCollection_of_string()
        {
            var result = ArrangeAndAct<ReadOnlyCollectionInterfaceTarget>();
            Assert.AreEqual(result.Command, "delete");
            Assert.IsNotNull(result.Directories);
            Assert.AreEqual(2, result.Directories.Count());
            Assert.AreEqual(@"%TEMP%\Program.exe", result.Directories.ElementAt(0));
            Assert.AreEqual(@"%TEMP%\Program_exe", result.Directories.ElementAt(1));
        }



        private T ArrangeAndAct<T>() where T : StringUnitTestTarget
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Command", "delete")
                    .AddArgument("Directories", @"%TEMP%\Program.exe", @"%TEMP%\Program_exe")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Command", ArgumentMultiplicity.One, true),
                new Argument("Directories", ArgumentMultiplicity.OneOrMore, false),
                new Argument("Files", ArgumentMultiplicity.OneOrMore, false, ArgumentFlags.ExistingFile)
            }.ToImmutableArray();
            var delimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);
            IObjectBinder testee = new ConstructorBinder(filesystemMock.Object);
            var instance = testee.CreateObject<T>(resolution);
            return (T)instance;
        }
        
        #region "  Target classes  "

        private abstract class StringUnitTestTarget
        {
        }

        private class ScalarTarget : StringUnitTestTarget
        {
            private readonly string _command;
            public ScalarTarget(string command)
            {
                _command = command;
            }
            public string Command => _command;
        }

        private class ArrayTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly string[] _directories;
            public ArrayTarget(string command, string[] directories)
            {
                _command = command;
                _directories = directories;
            }
            public string[] Directories => _directories;
            public string Command => _command;
        }

        private class ListTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly List<string> _directories;
            public ListTarget(string command, List<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public List<string> Directories => _directories;
            public string Command => _command;
        }

        private class ImmutableArrayTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly ImmutableArray<string> _directories;
            public ImmutableArrayTarget(string command, ImmutableArray<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public ImmutableArray<string> Directories => _directories;
            public string Command => _command;
        }

        private class ListGenericInterfaceTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly IList<string> _directories;
            public ListGenericInterfaceTarget(string command, IList<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public IList<string> Directories => _directories;
            public string Command => _command;
        }

        private class ListInterfaceTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly IList _directories;
            public ListInterfaceTarget(string command, IList directories)
            {
                _command = command;
                _directories = directories;
            }
            public IList Directories => _directories;
            public string Command => _command;
        }

        private class CollectionGenericInterfaceTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly ICollection<string> _directories;
            public CollectionGenericInterfaceTarget(string command, ICollection<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public ICollection<string> Directories => _directories;
            public string Command => _command;
        }

        private class EnumerableGenericInterfaceTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly IEnumerable<string> _directories;
            public EnumerableGenericInterfaceTarget(string command, IEnumerable<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public IEnumerable<string> Directories => _directories;
            public string Command => _command;
        }

        private class ReadOnlyCollectionInterfaceTarget : StringUnitTestTarget
        {
            private readonly string _command;
            private readonly IReadOnlyCollection<string> _directories;
            public ReadOnlyCollectionInterfaceTarget(string command, IReadOnlyCollection<string> directories)
            {
                _command = command;
                _directories = directories;
            }
            public IReadOnlyCollection<string> Directories => _directories;
            public string Command => _command;
        }

        #endregion
    }
}
