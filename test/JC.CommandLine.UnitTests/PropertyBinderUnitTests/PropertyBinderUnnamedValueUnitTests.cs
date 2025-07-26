using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderUnnamedValueUnitTests
    {
        [TestMethod]
        public void Assigns_all_unnamed_values()
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
            var result = testee.CreateObject<AllUnnamedValuesValidTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Count());
        }

        [TestMethod]
        public void Assigns_all_unnamed_values_to_ImmutableArray()
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
            var result = testee.CreateObject<AllUnnamedValuesImmutableArrayTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Count());
        }

        [TestMethod]
        public void Assigns_leading_unnamed_values_to_Enumerable()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddUnnamedArgument("all")
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
            var result = testee.CreateObject<LeadingUnnamedValuesEnumerableTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(2, result.LeadingUnnamedValues.Count());
        }

        [TestMethod]
        public void Assigns_trailing_unnamed_values()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("ProcessFiles.exe")
                    .AddUnnamedArgument("encrypt")
                    .AddArgument("Delete")
                    .AddUnnamedArgument("FileA.txt")
                    .AddUnnamedArgument("FileB.docx")
                    .AddUnnamedArgument("FileC.js")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Delete", ArgumentMultiplicity.Zero, true)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolutions = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<TrailingUnnamedValuesEnumerableTarget>(resolutions);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(3, result.TrailingUnnamedValues.Count());
        }

        [TestMethod]
        public void No_trailing_unnamed_args_is_OK()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("ProcessFiles.exe")
                    .AddUnnamedArgument("encrypt")
                    .AddArgument("Files", "FileA.txt", "FileB.docx", "FileC.js")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Files", ArgumentMultiplicity.OneOrMore, true)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolutions = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var result = testee.CreateObject<TrailingUnnamedValuesEnumerableTarget>(resolutions);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.IsFalse(result.TrailingUnnamedValues.Any());
        }


        private class AllUnnamedValuesValidTarget
        {
            public IEnumerable<string> UnnamedValues { get; set; }
        }

        private class AllUnnamedValuesImmutableArrayTarget
        {
            public ImmutableArray<string> UnnamedValues { get; set; }
        }

        private class LeadingUnnamedValuesEnumerableTarget
        {
            public IEnumerable<string> LeadingUnnamedValues { get; set; }
        }

        private class TrailingUnnamedValuesEnumerableTarget
        {
            public IEnumerable<string> TrailingUnnamedValues { get; set; }
        }
    }
}
