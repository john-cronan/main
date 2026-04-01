using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineParserUnitTests
    {
        [TestMethod]
        public void Parses_leading_unnamed_values()
        {
            var model = TestParseModel.Create();
            ICommandLineParser testee = new CommandLineParser(model, new PropertyBinder());
            var args = new string[] { "Utility.exe", "Import", "Authors", "-file", "authors.csv" };
            var leadingValues = testee.ParseLeadingUnnamedValues(args);
            Assert.IsTrue(leadingValues.SequenceEqual(new string[] { "Import", "Authors" }));
        }

        [TestMethod]
        public void Help_switch_is_present()
        {
            var arguments = new Argument[]
            {
                new Argument("File", ArgumentMultiplicity.One, true),
                new Argument(new string[] { "?", "Help" }.ToImmutableArray(), 
                    ArgumentMultiplicity.Zero, false)
            };
            var model = TestParseModel.Create(arguments: arguments.ToImmutableArray(),
                helpArgument: arguments[1]);
            ICommandLineParser testee = new CommandLineParser(model, new PropertyBinder());
            var commandLine = new string[] { "Utility.exe", "Import", "/?", "/file" };
            Assert.IsTrue(testee.IsHelpSwitchPresent(commandLine));
        }

        [TestMethod]
        public void Help_switch_defined_not_present()
        {
            var arguments = new Argument[]
            {
                new Argument("File", ArgumentMultiplicity.One, true),
                new Argument(new string[] { "?", "Help" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            };
            var model = TestParseModel.Create(arguments: arguments.ToImmutableArray(),
                helpArgument: arguments[1]);
            ICommandLineParser testee = new CommandLineParser(model, new PropertyBinder());
            var commandLine = new string[] { "Utility.exe", "Import", "/file" };
            Assert.IsFalse(testee.IsHelpSwitchPresent(commandLine));
        }

        [TestMethod]
        public void Help_switch_must_be_defined_as_such()
        {
            var arguments = new Argument[]
            {
                new Argument("File", ArgumentMultiplicity.One, true),
                new Argument(new string[] { "?", "Help" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            };
            var model = TestParseModel.Create(arguments: arguments.ToImmutableArray());
            ICommandLineParser testee = new CommandLineParser(model, new PropertyBinder());
            var commandLine = new string[] { "Utility.exe", "Import", "/?", "/file" };
            Assert.IsFalse(testee.IsHelpSwitchPresent(commandLine));
        }
    }
}
