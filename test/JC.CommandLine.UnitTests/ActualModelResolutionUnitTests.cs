using JC.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class ActualModelResolutionUnitTests
    {
        private readonly ParseModel _nullParseModel;

        public ActualModelResolutionUnitTests()
        {
            _nullParseModel = new ParseModel(ImmutableArray<Argument>.Empty,
                new char[] { '-', '/' }.ToImmutableArray(), false, NameMatchingOptions.Stem,
                true, '@');
        }
        [TestMethod]
        public void Actuals_match_ctor_parameter()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("program.exe")
                    .AddArgument("files", "FileA.txt", "FileB.txt")
                    .GetCommandLine();
            var testee = new ActualModelResolution(actuals, _nullParseModel);
            Assert.IsTrue(CommandLineNodeCompare.Equals(actuals, testee.Actuals, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Actual_matches_multiple_model_arguments()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("program.exe")
                    .AddArgument("f", "FileA.txt", "FileB.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Force", ArgumentMultiplicity.Zero, false),
                new Argument("Files", ArgumentMultiplicity.OneOrMore, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.IsFalse(testee.AmbiguousActuals.IsDefault);
            Assert.AreEqual(1, testee.AmbiguousActuals.Length);
            Assert.IsTrue(testee.AmbiguousActuals[0].KeyNode.Text.Equals(actuals[1].KeyNode.Text, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Actual_matches_with_stem_matching()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("Rec")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(0, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Actual_matches_single_model_argument_name()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("Files", "FileA.txt", "FileB.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Files", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(0, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Actual_matches_second_model_argument_name()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("f", "FileA.txt", "FileB.txt")
                    .GetCommandLine();
            var names = new string[] { "Files", "f" }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(names,  ArgumentMultiplicity.OneOrMore, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(0, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Model_arguments_match_ctor_parameter()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("Directory", @"C:\")
                    .GetCommandLine();
            var expectedDirectoryArgument = new Argument("Directory",
                ArgumentMultiplicity.OneOrMore, true);
            var expectedRecurseArgument = new Argument("Recurse",
                ArgumentMultiplicity.Zero, false);
            var arguments = new Argument[]
            {
                expectedDirectoryArgument,
                expectedRecurseArgument
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(arguments.Count(), testee.Model.Arguments.Count());
            var actualDirectoryArgument = testee.Model.Arguments.Single(m => m.Names.SequenceEqual(expectedDirectoryArgument.Names));
            Assert.AreEqual(expectedDirectoryArgument.Multiplicity, actualDirectoryArgument.Multiplicity);
            Assert.AreEqual(expectedDirectoryArgument.Required, actualDirectoryArgument.Required);
            var actualRecurseArgument = testee.Model.Arguments.Single(m => m.Names.SequenceEqual(expectedRecurseArgument.Names));
            Assert.AreEqual(expectedRecurseArgument.Multiplicity, actualRecurseArgument.Multiplicity);
            Assert.AreEqual(expectedRecurseArgument.Required, actualRecurseArgument.Required);
        }

        [TestMethod]
        public void Missing_and_required_argument_is_reported()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("recurse")
                    .GetCommandLine();
            var names = new string[] { "Files", "f" }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(names, ArgumentMultiplicity.OneOrMore, true),
                new Argument(ImmutableArray<string>.Empty.Add("Recurse"),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(1, testee.MissingModelArguments.Count());
            Assert.AreEqual(0, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Missing_and_not_required_argument_is_not_reported()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("recurse")
                    .GetCommandLine();
            var names = new string[] { "Files", "f" }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(names, ArgumentMultiplicity.OneOrMore, false),
                new Argument(ImmutableArray<string>.Empty.Add("Recurse"),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(0, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Undefine_actual_is_reported()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("Files", "FileA.txt", "FileB.txt")
                    .AddArgument("recurse")
                    .GetCommandLine();
            var names = new string[] { "Files", "f" }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(names, ArgumentMultiplicity.OneOrMore, false),
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                false, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(1, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(1, testee.UndefinedActuals.Count());
            Assert.IsTrue(testee.UndefinedActuals.Single().KeyNode.Text.Equals("recurse", StringComparison.InvariantCulture));
        }

        [TestMethod]
        public void Case_sensitive_arguments_dont_match()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files (x86)\Whatever\program.exe")
                    .AddArgument("files", "FileA.txt", "FileB.txt")
                    .AddArgument("recurse")
                    .GetCommandLine();
            var names = new string[] { "Files", "f" }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(names, ArgumentMultiplicity.OneOrMore, false),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(actuals, model);
            Assert.AreEqual(0, testee.Matches.Count());
            Assert.AreEqual(0, testee.AmbiguousActuals.Count());
            Assert.AreEqual(0, testee.MissingModelArguments.Count());
            Assert.AreEqual(2, testee.UndefinedActuals.Count());
        }

        [TestMethod]
        public void Generates_exception_for_undefined_arguments()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\FormatHD.exe")
                    .AddArgument("Really")
                    .AddArgument("YesReally")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[]{"Really" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("undefined", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Generates_exception_for_ambiguous_argument()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\FormatHD.exe")
                    .AddArgument("R")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[]{"Really" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, true),
                new Argument(new string[] { "Recursive" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("ambiguous", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Generates_exception_for_required_arguments()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\FormatHD.exe")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[]{"Really" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("required", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Generates_exception_for_unnamed_value()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\FormatHD.exe")
                    .AddUnnamedArgument("Really")
                    .GetCommandLine();
            var arguments = new Argument[] { }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, false, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("unnamed", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void More_than_one_value_generates_exception()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\RecycleDirectory.exe")
                    .AddArgument("Dir", "DirectoryA", "DirectoryB")
                    .GetCommandLine();
            var arguments = new Argument[] {
                new Argument(new string[] { "Directory" }.ToImmutableArray(),
                    ArgumentMultiplicity.One, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("exactly one value", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void No_values_generates_exception()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\RecycleDirectory.exe")
                    .AddArgument("Dir")
                    .GetCommandLine();
            var arguments = new Argument[] {
                new Argument(new string[] { "Directory" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("one or more values", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void Values_generates_exceptions()
        {
            var nodeGroups =
                new CommandLineBuilder()
                    .AddExeNode(@"C:\Program Files\OPAS\RecycleDirectory.exe")
                    .AddArgument("Recurse", "DirA", "DirB")
                    .GetCommandLine();
            var arguments = new Argument[] {
                new Argument(new string[] { "Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, true)
            }.ToImmutableArray();
            var model = new ParseModel(arguments, new char[] { '-', '/' }.ToImmutableArray(),
                true, NameMatchingOptions.Stem, true, '@');
            var testee = new ActualModelResolution(nodeGroups, model);
            (var errors, var warnings) = testee.Validate();
            Assert.IsNotNull(errors);
            Assert.IsNull(warnings);
            Assert.AreEqual(1, errors.ParseErrors.Count());
            Assert.IsTrue(errors.ParseErrors.Single().Message.Contains("may not have values", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
