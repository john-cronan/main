using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class HelpSwitchUnitTests
    {
        [TestMethod]
        public void Errors_surface_as_warnings()
        {
            var commandLine =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddUnnamedArgument("Import")
                    .AddArgument("Files", "Authors.csv", "Titles.csv")
                    .AddArgument("Help")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[]{ "?", "Help" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = TestParseModel.Create(arguments: arguments, helpArgument: arguments[0]);
            var testee = new ActualModelResolution(commandLine, model);
            (var errors, var warnings) = testee.Validate();

            Assert.IsNull(errors);
            Assert.IsNotNull(warnings);
            Assert.AreEqual(1, warnings.ParseErrors.Count());
            Assert.IsTrue(warnings.ParseErrors.First().Message.Contains("Files", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(warnings.ParseErrors.First().Message.Contains("undefined", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Parser_parses_invalid_command_line_with_help_switch()
        {
            var commandLine = new string[]
            {
                "Program.exe",
                "Import",
                "-Files",
                "Authors.csv",
                "Titles.csv",
                "-?"
            };
            var arguments = new Argument[]
            {
                new Argument(new string[]{ "?", "Help" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = TestParseModel.Create(arguments: arguments, helpArgument: arguments[0]);
            ICommandLineParser testee = new CommandLineParser(model, new PropertyBinder());
            var results = testee.Parse(commandLine);

            Assert.IsNotNull(results);
            Assert.IsNotNull(results.ParseWarnings);
            Assert.IsTrue(results.ParseWarnings.ParseErrors.Any());
        }

        [TestMethod]
        public void ConstructorBinder_binds_to_help_argument()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("?")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Files", ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] { "?", "Help" }.ToImmutableArray(),  
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = TestParseModel.Create(arguments: arguments, helpArgument: arguments[1]);
            var amr = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();

            var result = testee.CreateObject<ConstructorBoundCommandLine>(amr);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Help);
            Assert.IsNotNull(result.ParseWarnings);
            Assert.AreEqual(1, result.ParseWarnings.ParseErrors.Count());
        }

        [TestMethod]
        public void PropertyBinder_binds_to_help_argument()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Batch-Size", "1000")
                    .AddArgument("?")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Files", ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] { "?", "Help" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = TestParseModel.Create(arguments: arguments, helpArgument: arguments[1]);
            var amr = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();

            var result = testee.CreateObject<PropertyBoundCommandLine>(amr);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Help);
            Assert.IsNotNull(result.ParseWarnings);
            Assert.AreEqual(2, result.ParseWarnings.ParseErrors.Count());
        }


        private class ConstructorBoundCommandLine
        {
            private readonly bool _help;
            private readonly CommandLineParseException _parseWarnings;

            public ConstructorBoundCommandLine(bool help, CommandLineParseException parseWarnings)
            {
                _help = help;
                _parseWarnings = parseWarnings;
            }

            public bool Help => _help;
            public CommandLineParseException ParseWarnings => _parseWarnings;
        }

        private class PropertyBoundCommandLine
        {
            public bool Help { get; set; }
            public CommandLineParseException ParseWarnings { get; set; }
        }
    }
}
