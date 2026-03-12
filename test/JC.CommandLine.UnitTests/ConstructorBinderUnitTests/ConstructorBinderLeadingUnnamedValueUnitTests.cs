using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderLeadingUnnamedValueUnitTests
    {
        [TestMethod]
        public void Assigns_leading_unnamed_values_to_ImmutableArray()
        {
            var result = ArrangeAndAct<LeadingUnnamedValuesImmutableArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(2, result.LeadingUnnamedValues.Length);
            Assert.AreEqual("delete", result.LeadingUnnamedValues[0]);
            Assert.AreEqual("files", result.LeadingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_leading_unnamed_values_to_array()
        {
            var result = ArrangeAndAct<LeadingUnnamedValuesArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(2, result.LeadingUnnamedValues.Length);
            Assert.AreEqual("delete", result.LeadingUnnamedValues[0]);
            Assert.AreEqual("files", result.LeadingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_leading_unnamed_values_to_list()
        {
            var result = ArrangeAndAct<LeadingUnnamedValuesListTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(2, result.LeadingUnnamedValues.Count);
            Assert.AreEqual("delete", result.LeadingUnnamedValues[0]);
            Assert.AreEqual("files", result.LeadingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_leading_unnamed_values_to_Enumerable()
        {
            var result = ArrangeAndAct<LeadingUnnamedValuesEnumerableTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(2, result.LeadingUnnamedValues.Count());
            Assert.AreEqual("delete", result.LeadingUnnamedValues.ElementAt(0));
            Assert.AreEqual("files", result.LeadingUnnamedValues.ElementAt(1));
        }

        [TestMethod]
        public void Assigns_single_leading_unnamed_value_to_string()
        {
            var unnamedValuesModel = new UnnamedValuesParseModel(ArgumentMultiplicity.One,
                ArgumentMultiplicity.ZeroOrMore);
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var result = ArrangeAndAct<LeadingUnnameValueStringTarget>(
                actuals: actuals, unnamedValuesModel: unnamedValuesModel);
            Assert.IsNotNull(result);
            Assert.AreEqual("delete", result.LeadingUnnamedValue);
            Assert.IsNotNull(result.LeadingUnnamedValues);
            Assert.AreEqual(1, result.LeadingUnnamedValues.Count());
            Assert.AreEqual("delete", result.LeadingUnnamedValues.Single());
        }

        [TestMethod]
        public void Assigns_single_trailing_unnamed_value_to_string()
        {
            var unnamedValuesModel = new UnnamedValuesParseModel(
                ArgumentMultiplicity.OneOrMore, ArgumentMultiplicity.One);
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .GetCommandLine();
            var result = ArrangeAndAct<TrailingUnnamedValueStringTarget>(
                actuals: actuals, unnamedValuesModel: unnamedValuesModel);
            Assert.IsNotNull(result);
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValue);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(1, result.TrailingUnnamedValues.Count());
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValues.Single());
        }




        private T ArrangeAndAct<T>(
            ImmutableArray<CommandLineNodeGroup> actuals = default,
            UnnamedValuesParseModel unnamedValuesModel = null) where T : TargetBase
        {
            var effectiveUnnamedValuesModel = unnamedValuesModel ?? UnnamedValuesParseModel.AllowAll;
            var effectiveActuals = actuals;
            if (effectiveActuals == default)
            {
                effectiveActuals =
                    new CommandLineBuilder()
                        .AddUnnamedArgument("delete")
                        .AddUnnamedArgument("files")
                        .AddArgument("r")
                        .AddUnnamedArgument("SomeFile.txt")
                        .AddUnnamedArgument("SomeOtherFile.txt")
                        .GetCommandLine();
            }
            var arguments = new Argument[]
            {
                new Argument("Recycle", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, effectiveUnnamedValuesModel, '@');
            var resolution = new ActualModelResolution(effectiveActuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var result = testee.CreateObject<T>(resolution);
            return result;
        }

        #region "  Target classes  "

        private abstract class TargetBase
        {
        }

        private class LeadingUnnamedValuesImmutableArrayTarget : TargetBase
        {
            private readonly ImmutableArray<string> _leadingUnnamedValues;
            public LeadingUnnamedValuesImmutableArrayTarget(ImmutableArray<string> leadingUnnamedValues)
            {
                _leadingUnnamedValues = leadingUnnamedValues;
            }
            public ImmutableArray<string> LeadingUnnamedValues => _leadingUnnamedValues;
        }

        private class LeadingUnnamedValuesArrayTarget : TargetBase
        {
            private readonly string[] _leadingUnnamedValues;
            public LeadingUnnamedValuesArrayTarget(string[] leadingUnnamedValues)
            {
                _leadingUnnamedValues = leadingUnnamedValues;
            }
            public string[] LeadingUnnamedValues => _leadingUnnamedValues;
        }

        private class LeadingUnnamedValuesListTarget : TargetBase
        {
            private readonly List<string> _leadingUnnamedValues;
            public LeadingUnnamedValuesListTarget(List<string> leadingUnnamedValues)
            {
                _leadingUnnamedValues = leadingUnnamedValues;
            }
            public List<string> LeadingUnnamedValues => _leadingUnnamedValues;
        }

        private class LeadingUnnamedValuesEnumerableTarget : TargetBase
        {
            private readonly IEnumerable<string> _leadingUnnamedValues;
            public LeadingUnnamedValuesEnumerableTarget(IEnumerable<string> leadingUnnamedValues)
            {
                _leadingUnnamedValues = leadingUnnamedValues;
            }
            public IEnumerable<string> LeadingUnnamedValues => _leadingUnnamedValues;
        }

        private class LeadingUnnameValueStringTarget : TargetBase
        {
            private readonly string _leadingUnnamedValue;
            private readonly IEnumerable<string> _leadingUnnamedValues;

            public LeadingUnnameValueStringTarget(string leadingUnnamedValue,
                IEnumerable<string> leadingUnnamedValues)
            {
                _leadingUnnamedValue = leadingUnnamedValue;
                _leadingUnnamedValues = leadingUnnamedValues;
            }

            public string LeadingUnnamedValue => _leadingUnnamedValue;
            public IEnumerable<string> LeadingUnnamedValues => _leadingUnnamedValues;
        }

        private class TrailingUnnamedValueStringTarget : TargetBase
        {
            private readonly string _trailingUnnamedValue;
            private readonly IEnumerable<string> _trailingUnnamedValues;

            public TrailingUnnamedValueStringTarget(string trailingUnnamedValue,
                IEnumerable<string> trailingUnnamedValues)
            {
                _trailingUnnamedValue = trailingUnnamedValue;
                _trailingUnnamedValues = trailingUnnamedValues;
            }

            public string TrailingUnnamedValue => _trailingUnnamedValue;
            public IEnumerable<string> TrailingUnnamedValues => _trailingUnnamedValues;
        }

        #endregion
    }
}
