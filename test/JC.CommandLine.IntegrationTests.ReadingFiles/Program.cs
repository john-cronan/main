using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace JC.CommandLine.IntegrationTests.ReadingFiles
{
    public static class Program
    {
        public static void Main()
        {
            var args = new string[]
            {
                "-Xml", Path.Join("Data", "XmlFile.xml"),
                "-XDoc", Path.Join("Data", "XmlFile.xml"),
                "-L", Path.Join("Data", "XmlFile.xml"),
                "-Text", Path.Join("Data", "XmlFile.xml"),
                "-Bytes", Path.Join("Data", "XmlFile.xml"),
                "-Numbers", Path.Join("Data", "Numbers.txt")
            };
            var documentPath = Path.Combine(Environment.CurrentDirectory, "Data", "XmlFile.xml");
            args[3] = documentPath;
            var commandLine =
                new CommandLineParserBuilder()
                    .UseArgumentDelimitter('-')
                    .AddArgument("XmlDoc", ArgumentMultiplicity.One, true, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .AddArgument("XDoc", ArgumentMultiplicity.One, true, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .AddArgument("Lines", ArgumentMultiplicity.One, true, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .AddArgument("Text", ArgumentMultiplicity.One, false, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .AddArgument("Bytes", ArgumentMultiplicity.One, true, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .AddArgument("Numbers", ArgumentMultiplicity.One, true, ArgumentFlags.ExistingFile | ArgumentFlags.ReadFileContent)
                    .CreateParser()
                    .Parse(args)
                    .Bind<CommandLine>();
            Assert.IsNotNull(commandLine);
            VerifyXmlDoc(commandLine);
            VerifyXDoc(commandLine);
            VerifyLines(commandLine);
            VerifyText(commandLine);
            VerifyBytes(commandLine);
            VerifyNumbers(commandLine);
        }

        private static void VerifyXmlDoc(CommandLine commandLine)
        {
            Assert.IsNotNull(commandLine.XmlDoc);
            var nodes = commandLine.XmlDoc
                            .DocumentElement
                            .SelectNodes("//File");
            foreach (XmlNode node in nodes)
            {
                var text = node.InnerText;
                if (text.Equals("File A.csv"))
                    continue;
                if (text.Equals("File B.csv"))
                    continue;
                if (text.Equals("File C.del"))
                    continue;
                Assert.Fail($"Unknown node {text}");
            }
        }

        private static void VerifyXDoc(CommandLine commandLine)
        {
            var names =
                from node in commandLine.XDoc.Root.Descendants("File")
                select node.Value;
            foreach (var name in names)
            {
                if (name.Equals("File A.csv"))
                    continue;
                if (name.Equals("File B.csv"))
                    continue;
                if (name.Equals("File C.del"))
                    continue;
                Assert.Fail($"Unknown node {name}");
            }
        }

        private static void VerifyLines(CommandLine commandLine)
        {
            Assert.IsNotNull(commandLine.Lines);
            Assert.AreEqual(8, commandLine.Lines.Length);
        }

        private static void VerifyText(CommandLine commandLine)
        {
            var actualText = File.ReadAllText(Path.Combine("Data", "XmlFile.xml"));
            Assert.AreEqual(actualText, commandLine.Text);
        }

        private static void VerifyBytes(CommandLine commandLine)
        {
            var actualBytes = File.ReadAllBytes(Path.Combine("Data", "XmlFile.xml"));
            Assert.IsTrue(commandLine.Bytes.SequenceEqual(actualBytes));
        }

        private static void VerifyNumbers(CommandLine commandLine)
        {
            Assert.IsNotNull(commandLine.Numbers);
            Assert.AreEqual(4, commandLine.Numbers.Length);
            Assert.AreEqual(1024, commandLine.Numbers[0]);
            Assert.AreEqual(2048, commandLine.Numbers[1]);
            Assert.AreEqual(4096, commandLine.Numbers[2]);
            Assert.AreEqual(8192, commandLine.Numbers[3]);
        }
    }
}
