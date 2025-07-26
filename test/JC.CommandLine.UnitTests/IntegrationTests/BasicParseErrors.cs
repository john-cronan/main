using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace JC.CommandLine.UnitTests.IntegrationTests
{
    [TestClass]
    public class BasicParseErrors
    {
        [TestMethod]
        public void Too_many_values()
        {
            var args = new string[]
            {
                Environment.GetCommandLineArgs()[0],
                "/f", "FileA.txt", "FileB.txt", "FileC.txt",
                "/r"
            };
            try
            {
                var results =
                    new CommandLineParserBuilder()
                        .UseStemNameMatching()
                        .UseArgumentDelimitters('-', '/')
                        .IsCaseInsensitive()
                        .AddArgument("File", ArgumentMultiplicity.One, true)
                        .AddSwitch("Recycle")
                        .CreateParser()
                        .Parse(args);
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                Assert.IsTrue(Regex.IsMatch(ex.Message, @"exactly\s+one\s+value"));
            }
        }

        [TestMethod]
        public void Required_argument_missing()
        {
            var args = new string[]
            {
                Environment.GetCommandLineArgs()[0],
                "/r"
            };
            try
            {
                var results =
                    new CommandLineParserBuilder()
                        .UseStemNameMatching()
                        .UseArgumentDelimitters('-', '/')
                        .IsCaseInsensitive()
                        .AddArgument("File", ArgumentMultiplicity.One, true)
                        .AddSwitch("Recycle")
                        .CreateParser()
                        .Parse(args);
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                Assert.IsTrue(Regex.IsMatch(ex.Message, @"required"));
            }
        }

        [TestMethod]
        public void Required_argument_missing_case_sensitive_edition()
        {
            var args = new string[]
            {
                Environment.GetCommandLineArgs()[0],
                "/f", "FileA.txt",
                "/attributes", "hsr"
            };
            try
            {
                var results =
                    new CommandLineParserBuilder()
                        .UseStemNameMatching()
                        .UseArgumentDelimitters('-', '/')
                        .IsCaseSensitive()
                        .AddArgument("file", ArgumentMultiplicity.One, true)
                        .AddArgument("Attributes", ArgumentMultiplicity.One, true)
                        .AddArgument("attributes", ArgumentMultiplicity.One, false)
                        .CreateParser()
                        .Parse(args);
                Assert.Fail("Expected: CommandLineParseException");
            }
            catch (CommandLineParseException ex)
            {
                Assert.IsTrue(Regex.IsMatch(ex.Message, @"attributes.*required", RegexOptions.IgnoreCase));
            }
        }
    }
}
