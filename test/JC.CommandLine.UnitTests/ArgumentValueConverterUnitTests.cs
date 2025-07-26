using JC.CommandLine;
using JC.CommandLine.TargetTypeConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class ArgumentValueConverterUnitTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrWhiteSpace(home))
            {
                Environment.SetEnvironmentVariable("HOME", "%USERPROFILE%");
            }
        }

        [TestMethod]
        public void Throws_on_non_existent_directory()
        {
            var directoryName = @"C:\Whatever";
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(false);
            //filesystemMock.Setup(m => m.IsPathFullyQualified(It.IsAny<string>())).Returns(true);
            filesystemMock.Setup(m => m.IsPathRooted(It.IsAny<string>())).Returns(true);
            filesystemMock.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(s => s));
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            try
            {
                var output = testee.Convert(directoryName, typeof(string), ArgumentFlags.ExistingDirectory);
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                var pattern = directoryName.Replace("\\", "\\\\");
                pattern = pattern + ".*not\\s+found";
                Assert.IsTrue(Regex.IsMatch(ex.Message, pattern, RegexOptions.IgnoreCase));
            }
        }

        [TestMethod]
        public void Existing_directory_expands_environment_variables()
        {
            var testee = new ArgumentValueConverter();
            var expected = Environment.ExpandEnvironmentVariables("%USERPROFILE%");
            var output = (string)testee.Convert("%USERPROFILE%", typeof(string), ArgumentFlags.ExistingDirectory).First();
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void Existing_directory_throws_on_string_array()
        {
            var testee = new ArgumentValueConverter();
            try
            {
                var output = (string[])testee.Convert("%USERPROFILE%", typeof(string[]), ArgumentFlags.ExistingDirectory);
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                Assert.IsTrue(ex.Message.Contains("string[]", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestMethod]
        public void Returns_text_of_file()
        {
            var filePath = @"C:\Program Files\SomeFile.txt";
            var content = "Some text";

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(content);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = testee.Convert(filePath, typeof(string), ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                            .Single();
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void Reading_file_expands_environment_variables()
        {
            var pathWithVariable = @"%HOME%\SomeFile.txt";
            var expandedPath = Environment.ExpandEnvironmentVariables(pathWithVariable);
            var content = "This is file's contents";

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString().Equals(expandedPath, StringComparison.InvariantCultureIgnoreCase)))).Returns(content);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var value = testee.Convert(pathWithVariable, typeof(string), ArgumentFlags.ReadFileContent)
                            .Single();
            Assert.AreEqual(content, value);
        }

        [TestMethod]
        public void Reading_file_throws_file_not_found()
        {
            var testee = new ArgumentValueConverter();
            try
            {
                var value = testee.Convert(@"%USERPROFILE%\NonExistentFile.txt",
                    typeof(string), ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent);
                Assert.Fail($"Expected: {nameof(CommandLineParseException)}");
            }
            catch (FileNotFoundException)
            {
            }
        }

        [TestMethod]
        public void Read_file_reads_collection_of_lines()
        {
            var filePath = @"/home/you/SomeFile.txt";
            var content = "Line 1\nLine 2";

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllLines(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(content.Split("\n"));
            filesystemMock.Setup(m => m.FileExists(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(true);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = testee
                            .Convert(filePath, typeof(IEnumerable<string>), ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                            .Cast<string>();
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Count() == 2);
            Assert.IsTrue(output.ElementAt(0).Equals("Line 1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(output.ElementAt(1).Equals("Line 2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void Read_bytes_returns_contents_of_file()
        {
            var filePath = @"/home/you/SomeFile.txt";
            var contentAsString = "This is the content";
            var contentAsBytes = Encoding.UTF8.GetBytes(contentAsString);

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllBytes(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(contentAsBytes);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = testee
                            .Convert(filePath, typeof(byte[]), ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                            .Cast<byte>();
            contentAsBytes.SequenceEqual(output);
            Assert.IsTrue(contentAsBytes.SequenceEqual(output));
        }

        [TestMethod]
        public void Read_bytes_returns_immutable_array_of_bytes()
        {
            var filePath = @"/home/you/Documents/SomeFile.txt";
            var contentAsString = "This is the content";
            var contentAsBytes = Encoding.UTF8.GetBytes(contentAsString);

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllBytes(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(contentAsBytes);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = testee
                            .Convert(filePath, typeof(ImmutableArray<byte>),
                                ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                            .Cast<byte>()
                            .ToImmutableArray();
            Assert.IsNotNull(output);
            Assert.IsTrue(contentAsBytes.SequenceEqual(output));
        }

        [TestMethod]
        public void Read_file_content_returns_xml_document()
        {
            var filePath = @"C:\Program Files\Microsoft\SomeProgram.xml";
            var documentAsString = "<document><element>Hello</element></document>";
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString().Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(documentAsString);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var result = (XmlDocument)testee
                            .Convert(filePath, typeof(XmlDocument), ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                            .Single();
            var elementValue = result.DocumentElement.SelectSingleNode("element").InnerText;
            Assert.AreEqual(elementValue, "Hello");
        }

        [TestMethod]
        public void Read_file_content_returns_xml_node()
        {
            var filePath = @"C:\Program Files\IBM\SomeProgram.xml";
            var documentAsString = "<document><element>Hello</element></document>";
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString().Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(documentAsString);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var result = (XmlNode)testee
                            .Convert(filePath, typeof(XmlNode), ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                            .Single();
            var elementValue = result.SelectSingleNode("element").InnerText;
            Assert.AreEqual(elementValue, "Hello");
        }

        [TestMethod]
        public void Read_file_content_returns_XDocument()
        {
            var filePath = @"C:\Program Files\Apple\SomeProgram.xml";
            var documentAsString = "<document><element>Hello</element></document>";
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString().Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(documentAsString);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var result = (XDocument)testee
                            .Convert(filePath, typeof(XDocument),
                                ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                            .Single();
            var elementValue = result.Root.Element("element").Value;
            Assert.AreEqual(elementValue, "Hello");
        }

        [TestMethod]
        public void Read_file_content_returns_XElement()
        {
            var filePath = @"C:\Program Files\IBM\SomeProgram.xml";
            var documentAsString = "<document><element>Hello</element></document>";
            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllText(It.Is<string>((obj, t) => obj.ToString().Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))).Returns(documentAsString);
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var result = (XElement)testee
                            .Convert(filePath, typeof(XElement), ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                            .Single();
            var elementValue = result.Element("element").Value;
            Assert.AreEqual(elementValue, "Hello");
        }

        [TestMethod]
        public void Read_file_content_returns_int()
        {
            var filePath = @"C:\Program Files\SomeFile.txt";
            var content = "27";

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllLines(It.Is<string>(path => path == filePath))).Returns(new string[] { content });
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = (int)testee
                            .Convert(filePath, typeof(int), ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                            .Cast<int>()
                            .Single();
            Assert.AreEqual(int.Parse(content), output);
        }

        [TestMethod]
        public void Read_file_content_returns_TimeSpan()
        {
            var filePath = @"C:\Program Files\SomeFile.txt";
            var content = TimeSpan.FromSeconds(1.5).ToString();

            var filesystemMock = new Mock<IFilesystem>();
            filesystemMock.Setup(m => m.ReadAllLines(It.Is<string>((obj, t) => obj.ToString() == filePath))).Returns(new string[] { content });
            var testee = new ArgumentValueConverter(filesystemMock.Object);
            var output = (TimeSpan)testee.Convert(filePath, typeof(TimeSpan), ArgumentFlags.ReadFileContent | ArgumentFlags.ExistingFile)
                                        .Single();
            Assert.AreEqual(TimeSpan.Parse(content), output);
        }

        [TestMethod]
        public void Parses_strings()
        {
            var testee = new ArgumentValueConverter();
            var input = "f9c17f74e674464caba2804a49a081ea";
            var output = (string)testee.Convert(input, typeof(string), ArgumentFlags.None).Single();
            Assert.IsTrue(output == input);
        }

        [TestMethod]
        public void Parses_int32s()
        {
            const int value = 42;

            var testee = new ArgumentValueConverter();
            var output = (int)testee.Convert(value.ToString(), typeof(int), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_int64s()
        {
            const long value = (long)int.MaxValue + (long)int.MaxValue;

            var testee = new ArgumentValueConverter();
            var output = (long)testee.Convert(value.ToString(), typeof(long), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_doubles()
        {
            const double value = 3.14159265358;

            var testee = new ArgumentValueConverter();
            var output = (double)testee.Convert(value.ToString(), typeof(double), ArgumentFlags.None).Single();
            Assert.AreEqual(value, output, .0001);
        }

        [TestMethod]
        public void Parses_singles()
        {
            const double value = 3.14159;

            var testee = new ArgumentValueConverter();
            var output = (Single)testee.Convert(value.ToString(), typeof(Single), ArgumentFlags.None).Single();
            Assert.AreEqual(value, output, .001);
        }

        [TestMethod]
        public void Parses_decimals()
        {
            const decimal value = 1.41M;

            var testee = new ArgumentValueConverter();
            var output = (decimal)testee.Convert(value.ToString(), typeof(decimal), ArgumentFlags.None).Single();
            Assert.AreEqual(value, output);
        }

        [TestMethod]
        public void Parses_Guids()
        {
            var value = Guid.Parse("165613ae-1e83-4ec0-8afd-12e1793e7239");

            var testee = new ArgumentValueConverter();
            var output = (Guid)testee.Convert(value.ToString(), typeof(Guid), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_DateTimes()
        {
            var value = DateTime.Parse("1/1/1970");

            var testee = new ArgumentValueConverter();
            var output = (DateTime)testee.Convert(value.ToString(), typeof(DateTime), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_TimeSpans()
        {
            var value = TimeSpan.FromSeconds(45.72);

            var testee = new ArgumentValueConverter();
            var output = (TimeSpan)testee.Convert(value.ToString(), typeof(TimeSpan), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_Uris()
        {
            var value = new Uri("http://www.microsoft.com");
            var testee = new ArgumentValueConverter();
            var output = (Uri)testee.Convert(value.ToString(), typeof(Uri), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void Parses_Enum()
        {
            var value = StringComparison.CurrentCultureIgnoreCase;
            var testee = new ArgumentValueConverter();
            var output = (StringComparison)testee.Convert(value.ToString(), typeof(StringComparison), ArgumentFlags.None).Single();
            Assert.IsTrue(value == output);
        }

        [TestMethod]
        public void ReadExistingDirectory_returns_FileSystemInfos()
        {
            var virtualFS = new VirtualFilesystem();
            var testee = new ArgumentValueConverter(virtualFS.FileSystem);
            var targetType = typeof(FileSystemInfo[]);
            var returnValue = testee.Convert(virtualFS.ParentDirectory, targetType, ArgumentFlags.ExistingDirectory);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(4, returnValue.Count());
            Assert.IsTrue(returnValue.All(i => i is FileSystemInfo));
            Assert.AreEqual(2, returnValue.OfType<DirectoryInfo>().Count());
            Assert.AreEqual(2, returnValue.OfType<FileInfo>().Count());
        }

        [TestMethod]
        public void ReadExistingDirectory_returns_DirectoryInfos()
        {
            var virtualFS = new VirtualFilesystem();
            var testee = new ArgumentValueConverter(virtualFS.FileSystem);
            var targetType = typeof(DirectoryInfo[]);
            var returnValue = testee.Convert(virtualFS.ParentDirectory, targetType, ArgumentFlags.ExistingDirectory);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count());
            Assert.IsTrue(returnValue.All(i => i is DirectoryInfo));
        }

        [TestMethod]
        public void ReadExistingDirectory_returns_FileInfos()
        {
            var virtualFS = new VirtualFilesystem();
            var testee = new ArgumentValueConverter(virtualFS.FileSystem);
            var targetType = typeof(FileInfo[]);
            var returnValue = testee.Convert(virtualFS.ParentDirectory, targetType, ArgumentFlags.ExistingDirectory);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count());
            Assert.IsTrue(returnValue.All(i => i is FileInfo));
        }

        [TestMethod]
        public void ReadExistingDirectory_returns_string()
        {
            var directoryPath = @"C:\Program Files\SomeCompany";
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);
            //filesystem.Setup(m => m.IsPathFullyQualified(It.IsAny<string>())).Returns(true);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(s => s));
            var testee = new ArgumentValueConverter(filesystem.Object);
            var returnValue = testee.Convert(directoryPath, typeof(string), ArgumentFlags.ExistingDirectory);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(1, returnValue.Count());
            Assert.AreEqual(directoryPath, returnValue.Single());
        }

        [TestMethod]
        public void ReadExistingDirectory_returns_DirectoryInfo()
        {
            var directoryPath = @"C:\Program Files\SomeCompany";
            var filesystem = new Mock<IFilesystem>();
            filesystem.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);
            //filesystem.Setup(m => m.IsPathRooted(It.IsAny<string>())).Returns(true);
            filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(s => s));
            var testee = new ArgumentValueConverter(filesystem.Object);
            var returnValue = testee.Convert(directoryPath, typeof(DirectoryInfo), ArgumentFlags.ExistingDirectory);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(1, returnValue.Count());
            var ok = directoryPath.Equals(returnValue.OfType<DirectoryInfo>().Single().FullName)
                    || directoryPath.Equals(returnValue.OfType<DirectoryInfo>().Single().Name);
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void Covnerts_hex_to_byte_array_with_preamble()
        {
            var expectedAsString = "And in the end, the love you take is equal to the love you make.";
            var expectedAsBytes = Encoding.UTF8.GetBytes(expectedAsString);
            var expectedAsHex = Binary.ToHex(expectedAsBytes, true);
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(expectedAsHex, typeof(byte[]), ArgumentFlags.None);
            Assert.IsNotNull(conversionResult);
            Assert.IsTrue(conversionResult.Success);
            var actualAsBytes = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(actualAsBytes.SequenceEqual(expectedAsBytes));
        }


        private class VirtualFilesystem
        {
            public VirtualFilesystem()
            {
                ParentDirectory = @"C:\Program Files\SomeCompany";
                var directoryA = Path.Combine(ParentDirectory, "DirectoryA");
                var directoryB = Path.Combine(ParentDirectory, "DirectoryB");
                Directories = new DirectoryInfo[]
                {
                    new DirectoryInfo(directoryA),
                    new DirectoryInfo(directoryB)
                };
                var fileA = Path.Combine(ParentDirectory, "FileA.txt");
                var fileB = Path.Combine(ParentDirectory, "FileB.xls");
                Files = new FileInfo[]
                {
                    new FileInfo(fileA),
                    new FileInfo(fileB)
                };
                var filesystemInfos = new FileSystemInfo[]
                {
                    new DirectoryInfo(directoryA),
                    new DirectoryInfo(directoryB),
                    new FileInfo(fileA),
                    new FileInfo(fileB)
                };
                var filesystem = new Mock<IFilesystem>();
                //filesystem.Setup(m => m.IsPathFullyQualified(It.IsAny<string>())).Returns(true);
                filesystem.Setup(m => m.GetFileSystemEntries(It.Is<string>(v => v.Equals(ParentDirectory, StringComparison.InvariantCultureIgnoreCase)))).Returns(filesystemInfos);
                filesystem.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);
                filesystem.Setup(m => m.MakePathFullyQualified(It.IsAny<string>())).Returns((Func<string, string>)(s => s));
                FileSystem = filesystem.Object;
            }

            public IFilesystem FileSystem { get; private set; }
            public IEnumerable<DirectoryInfo> Directories { get; private set; }
            public IEnumerable<FileInfo> Files { get; private set; }
            public string ParentDirectory { get; private set; }
        }
    }
}
