using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderConstructorSelectionUnitTests
    {
        private readonly ImmutableArray<CommandLineNodeGroup> _actuals;
        private readonly ParseModel _model;
        private readonly ActualModelResolution _actualModelResolutions;
        private readonly IObjectBinder _testee;

        public ConstructorBinderConstructorSelectionUnitTests()
        {
            _actuals =
                new CommandLineBuilder()
                    .AddExeNode("ProcessFiles")
                    .AddArgument("Delete")
                    .AddArgument("Recycle")
                    .AddArgument("IncludeHidden")
                    .AddArgument("Verbose")
                    .AddUnnamedArgument("File1.txt")
                    .AddUnnamedArgument("File2.txt")
                    .AddUnnamedArgument("File3.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Delete", ArgumentMultiplicity.Zero, false),
                new Argument("Recycle", ArgumentMultiplicity.Zero, false),
                new Argument("IncludeHidden", ArgumentMultiplicity.Zero, false),
                new Argument("Verbose", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var delimitters = "-/".ToImmutableArray();
            _model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Stem, true, '@');
            _actualModelResolutions = new ActualModelResolution(_actuals, _model);
            _testee = new ConstructorBinder();
        }

        [TestMethod]
        public void Chooses_greediest_constructor()
        {
            var result = _testee.CreateObject<GreediestConstructorTarget>(_actualModelResolutions);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Pases_up_constructor_with_non_argument_parameters()
        {
            var result = _testee.CreateObject<NonArgumentConstructorParametersTarget>(_actualModelResolutions);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Only_no_argument_constructor()
        {
            try
            {
                _testee.CreateObject<OnlyNoArgumentConstructorTarget>(_actualModelResolutions);
                Assert.Fail($"Expected: {nameof(CommandLineBindingException)}");
            }
            catch (CommandLineBindingException ex)
            {
                Assert.IsTrue(Regex.IsMatch(ex.Message, @"suitable\s+constructor",
                    RegexOptions.IgnoreCase));
            }
        }

        [TestMethod]
        public void Selects_constructor_with_unnamed_values_parameters()
        {
            var result = _testee.CreateObject<UnnamedValuesParametersTarget>(_actualModelResolutions);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Tolerates_no_argument_constructor()
        {
            var result = _testee.CreateObject<NoArgumentConstructorTarget>(_actualModelResolutions);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Obeys_stem_matching()
        {
            var result = _testee.CreateObject<StemMatchingTarget>(_actualModelResolutions);
            Assert.IsNotNull(result);
        }


        #region "  Target Classes  "

        private class GreediestConstructorTarget
        {
            public GreediestConstructorTarget(bool delete, bool recycle, bool includeHidden,
                bool verbose)
            {
            }
            public GreediestConstructorTarget(bool delete, bool recycle, bool includeHidden)
            {
                Assert.Fail("This constructor should not be invoked");
            }
        }
        private class NonArgumentConstructorParametersTarget
        {
            public NonArgumentConstructorParametersTarget(bool delete, bool recycle, bool includeHidden)
            {
            }
            public NonArgumentConstructorParametersTarget(bool delete, bool recycle, bool includeHidden,
                IFilesystem filesystem)
            {
                Assert.Fail("This constructor should not be invoked");
            }
        }
        private class OnlyNoArgumentConstructorTarget
        {
            public OnlyNoArgumentConstructorTarget()
            {
                Assert.Fail("This constructor should not be invoked");
            }
        }
        private class UnnamedValuesParametersTarget
        {
            public UnnamedValuesParametersTarget(bool delete, bool recycle, bool includeHidden,
                bool verbose, IEnumerable<string> unnamedValues,
                IEnumerable<string> leadingUnnamedValues, IEnumerable<string> trailingUnnamedValues,
                CommandLineParseException parseWarnings)
            {
            }
        }
        public class NoArgumentConstructorTarget
        {
            public NoArgumentConstructorTarget()
            {
                Assert.Fail("This constructor should not be invoked");
            }
            public NoArgumentConstructorTarget(bool delete, bool recycle, bool includeHidden,
                bool verbose)
            {
            }
        }
        public class StemMatchingTarget
        {
            public StemMatchingTarget(bool d, bool r, bool i, bool v)
            {
            }
        }

        #endregion
    }
}
