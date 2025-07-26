using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderParseWarningsUnitTests
    {
        [TestMethod]
        public void Assigns_parse_warnings_to_property()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddArgument("f")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<ParseWarningsTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ParseWarnings);
            Assert.IsNotNull(result.ParseWarnings.ParseErrors);
            Assert.AreEqual(1, result.ParseWarnings.ParseErrors.Count());
            var ex = result.ParseWarnings.ParseErrors.Single();
            Assert.IsTrue(Regex.IsMatch(ex.Message, @"more\s+than\s+two", RegexOptions.IgnoreCase));
        }

        [TestMethod]
        public void Wont_attempt_assign_to_read_only_property()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddArgument("f")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<ParseWarningsTargetWithReadOnlyProperty>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNull(result.ParseWarnings);
        }

        [TestMethod]
        public void Wont_attempt_assign_to_invalid_typed_property()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddArgument("f")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<InvalidParseWarningsTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNull(result.ParseWarnings);
        }

        [TestMethod]
        public void Assigns_to_property_of_type_Exception()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddArgument("f")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<MoreGeneralTypeParseWarningsTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ParseWarnings);
            Assert.IsInstanceOfType(result.ParseWarnings, typeof(CommandLineParseException));
        }


        private class ParseWarningsTarget
        {
            public CommandLineParseException ParseWarnings { get; set; }
        }

        private class ParseWarningsTargetWithReadOnlyProperty
        {
            public CommandLineParseException ParseWarnings { get; private set; }
        }

        private class InvalidParseWarningsTarget
        {
            public FileNotFoundException ParseWarnings { get; set; }
        }
        private class MoreGeneralTypeParseWarningsTarget
        {
            public Exception ParseWarnings { get; set; }
        }
    }
}
