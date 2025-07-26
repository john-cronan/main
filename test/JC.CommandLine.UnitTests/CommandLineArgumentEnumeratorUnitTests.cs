using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineArgumentEnumeratorUnitTests
    {
        [TestMethod]
        public void Returns_arguments_passed_in()
        {
            var arguments = new string[]
            {
                "Program.exe", "/Recurse", "/Force", "-Files", "FileA.txt", "FileB.docx",
                "FileC.cs"
            };
            var filesystem = new Mock<IFilesystem>();
            var testee = new CommandLineArgumentEnumerator('@', filesystem.Object);
            var actuals = testee.Enumerate(arguments).ToArray();
            Assert.IsTrue(arguments.SequenceEqual(actuals));
        }

        [TestMethod]
        public void Expands_args_file()
        {
            var arguments = new string[]
            {
                "Program.exe", "/Recurse", "/Force", "-Files", "@args.txt"
            };
            var argsDotTXT = new string[]
            {
                "FileA.txt", "FileB.docx", "FileC.cs"
            };
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "args.txt"))).Returns(argsDotTXT);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(input => Path.Combine(Environment.CurrentDirectory, input)));
            var testee = new CommandLineArgumentEnumerator('@', filesystem.Object);
            var expected = arguments.Take(4).Concat(argsDotTXT).ToArray();
            var actual = testee.Enumerate(arguments).ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Expands_args_file_recursively()
        {
            var arguments = new string[]
            {
                "Program.exe", "/Recurse", "/Force", "-Files", "@args.txt"
            };
            var argsDotTXT = new string[]
            {
                "FileA.txt", "@args2.txt"
            };
            var argsTwoDotTXT = new string[]
            {
                "FileB.docx", "FileC.cs"
            };
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "args.txt"))).Returns(argsDotTXT);
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "args2.txt"))).Returns(argsTwoDotTXT);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(input => Path.Combine(Environment.CurrentDirectory, input)));
            var testee = new CommandLineArgumentEnumerator('@', filesystem.Object);
            var expected = arguments.Take(4).Concat(argsDotTXT.Take(1)).Concat(argsTwoDotTXT).ToArray();
            var actual = testee.Enumerate(arguments).ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Throws_on_cyclic_dependency()
        {
            var arguments = new string[]
            {
                "Program.exe", "/Recurse", "/Force", "-Files", "@args.txt"
            };
            var argsDotTXT = new string[]
            {
                "FileA.txt", "@args2.txt"
            };
            var argsTwoDotTXT = new string[]
            {
                "FileB.docx", "@args.txt"
            };
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "args.txt"))).Returns(argsDotTXT);
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "args2.txt"))).Returns(argsTwoDotTXT);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(input => Path.Combine(Environment.CurrentDirectory, input)));
            var testee = new CommandLineArgumentEnumerator('@', filesystem.Object);
            var expected = arguments.Take(4).Concat(argsDotTXT.Take(1)).Concat(argsTwoDotTXT).ToArray();
            try
            {
                var actual = testee.Enumerate(arguments).ToArray();
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                Assert.IsTrue(Regex.IsMatch(ex.Message, "cyclic", RegexOptions.IgnoreCase));
            }
        }

        [TestMethod]
        public void Supports_multiple_references_to_file()
        {
            var arguments = new string[]
            {
                "Program.exe", "/process", "@fileList.txt", "/delete", "@fileList.txt"
            };
            var fileList = new string[]
            {
                "FileA.txt", "FileB.docx", "FileC.cs"
            };
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.ReadAllLines(It.Is<string>(s => s == "fileList.txt"))).Returns(fileList);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(input => Path.Combine(Environment.CurrentDirectory, input)));
            var testee = new CommandLineArgumentEnumerator('@', filesystem.Object);
            var expected = arguments.Take(4).Concat(fileList).ToArray();
            var actual = testee.Enumerate(arguments).ToArray();
            Assert.AreEqual(2, actual.Count(arg => arg.Equals("FileA.txt", StringComparison.InvariantCultureIgnoreCase)));
            Assert.AreEqual(2, actual.Count(arg => arg.Equals("FileB.docx", StringComparison.InvariantCultureIgnoreCase)));
            Assert.AreEqual(2, actual.Count(arg => arg.Equals("FileC.cs", StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
