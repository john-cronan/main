using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderFilesUnitTests
    {
        private readonly string[] _lines =
        {
            "<document>",
            "\t<text>",
            "\t\tCall me Ahab",
            "\t</text>",
            "</document>"
        };
        private readonly string _content;
        private const string FilePath = @"C:\Users\Melville\Documents\MobyDick.txt";
        private Mock<IFilesystem> _virtualFilesystem;

        public ConstructorBinderFilesUnitTests()
        {
            _content = string.Join(Environment.NewLine, _lines);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _virtualFilesystem = new Mock<IFilesystem>();
            Expression<Func<string, bool>> matchesFilePath = s => s.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase);
            _virtualFilesystem.Setup(m => m.FileExists(It.Is<string>(s => s.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(true);
            _virtualFilesystem.Setup(m => m.ReadAllText(It.Is<string>(s => s.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(_content);
            _virtualFilesystem.Setup(m => m.ReadAllLines(It.Is<string>(matchesFilePath))).Returns(_lines);
        }

        [TestMethod]
        public void Reads_content_into_string()
        {
            var instance = ArrangeAndAct<StringTarget>();
            Assert.AreEqual(_content, instance.File);
        }

        [TestMethod]
        public void Reads_content_into_string_array()
        {
            var instance = ArrangeAndAct<StringArrayTarget>();
            Assert.IsTrue(_lines.SequenceEqual(instance.File));
        }

        private T ArrangeAndAct<T>()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Find.exe")
                    .AddArgument("File", FilePath)
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("File", ArgumentMultiplicity.One, true, 
                    ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
            }.ToImmutableArray();
            var delimitters = "/-".ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model, _virtualFilesystem.Object);
            IObjectBinder testee = new ConstructorBinder(_virtualFilesystem.Object);
            var instance = testee.CreateObject<T>(resolution);
            return instance;
        }

        private class StringTarget
        {
            private readonly string _file;
            public StringTarget(string file)
            {
                _file = file;
            }
            public string File => _file;
        }

        private class StringArrayTarget
        {
            private readonly string[] _file;
            public StringArrayTarget(string[] file)
            {
                _file = file;
            }
            public string[] File => _file;
        }
    }
}
