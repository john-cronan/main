using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineParserBuilderUnitTests
    {
        [TestMethod]
        public void Respects_delimitters_property()
        {
            var expected = new char[] { '/', '-', '_' };
            var parser =
                new CommandLineParserBuilder()
                    .UseArgumentDelimitters(expected)
                    .CreateParser();
            Assert.IsTrue(parser.ArgumentDelimitters.SequenceEqual(expected));
        }

        [TestMethod]
        public void Respects_case_sensitivity_property()
        {
            var testee =
                new CommandLineParserBuilder()
                    .IsCaseInsensitive();
            Assert.AreEqual(false, testee.CreateParser().CaseSensitive);
            testee = testee.IsCaseSensitive();
            Assert.AreEqual(true, testee.CreateParser().CaseSensitive);
        }

        [TestMethod]
        public void Switch_has_correct_name()
        {
            const string expected = "recurse";
            var parser = (CommandLineParser)
                new CommandLineParserBuilder()
                    .AddSwitch(expected)
                    .CreateParser();
            Assert.AreEqual(1, parser.Arguments.Length);
            Assert.AreEqual(1, parser.Arguments[0].Names.Length);
            Assert.AreEqual(expected, parser.Arguments[0].Names[0]);
        }

        [TestMethod]
        public void AddSwitch_throws_on_names_already_in_use()
        {
            var switchNames = new string[] { "Overwrite", "Clobber" };
            try
            {
                var builder =
                    new CommandLineParserBuilder()
                        .AddSwitch(switchNames)
                        .AddSwitch(switchNames[1]);
                Assert.Fail("Expected: ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(switchNames[1]));
            }
        }

        [TestMethod]
        public void AddSwitch_throws_on_duplicate_names_in_argument()
        {
            var switchNames = new string[] { "ArgA", "ArgB", "ArgC", "ArgA", "ArgB" };
            try
            {
                var builder =
                    new CommandLineParserBuilder()
                        .AddSwitch(switchNames);
                Assert.Fail("Expected: ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(switchNames[0]));
                Assert.IsTrue(ex.Message.Contains(switchNames[1]));
            }
        }

        [TestMethod]
        public void Switches_are_not_required()
        {
            var parser = (CommandLineParser)
                  new CommandLineParserBuilder()
                    .AddSwitch("recurse")
                    .CreateParser();
            Assert.AreEqual(1, parser.Arguments.Length);
            Assert.IsFalse(parser.Arguments[0].Required);
        }

        [TestMethod]
        public void Single_valued_argument_generates_correct_model()
        {
            var name = "Server";
            var required = true;
            var parser = (CommandLineParser)
                new CommandLineParserBuilder()
                    .AddArgument(name, ArgumentMultiplicity.One, required)
                    .CreateParser();
            Assert.AreEqual(1, parser.Arguments.Length);
            Assert.AreEqual(1, parser.Arguments[0].Names.Length);
            Assert.AreEqual(name, parser.Arguments[0].Names[0]);
            Assert.AreEqual(required, parser.Arguments[0].Required);
        }

        [TestMethod]
        public void MultiNamed_multi_valued_argument_has_both_names()
        {
            var names = new string[] { "Server", "DataSource" };
            var multiplicity = ArgumentMultiplicity.OneOrMore;
            var required = true;
            var parser = (CommandLineParser)
                new CommandLineParserBuilder()
                    .AddArgument(names, multiplicity, required)
                    .CreateParser();
            Assert.AreEqual(1, parser.Arguments.Length);
            Assert.AreEqual(2, parser.Arguments[0].Names.Length);
            Assert.IsTrue(parser.Arguments[0].Names.Contains("Server"));
            Assert.IsTrue(parser.Arguments[0].Names.Contains("DataSource"));
            Assert.AreEqual(required, parser.Arguments[0].Required);

        }

        [TestMethod]
        public void AddArgument_throws_on_name_already_in_use()
        {
            var names = new string[] { "File", "InputFile", "if" };
            try
            {
                var builder =
                    new CommandLineParserBuilder()
                        .AddArgument(names, ArgumentMultiplicity.One, true)
                        .AddArgument(names[2], ArgumentMultiplicity.One, true);
                Assert.Fail("Expected: ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(names[2]));
            }
        }

        [TestMethod]
        public void AddArgument_throws_on_duplicate_names_in_argument()
        {
            var argumentNames = new string[] { "ArgA", "ArgB", "ArgC", "ArgA", "ArgB" };
            try
            {
                var builder =
                    new CommandLineParserBuilder()
                        .AddSwitch(argumentNames);
                Assert.Fail("Expected: ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(argumentNames[0]));
                Assert.IsTrue(ex.Message.Contains(argumentNames[1]));
            }
        }
    }
}
