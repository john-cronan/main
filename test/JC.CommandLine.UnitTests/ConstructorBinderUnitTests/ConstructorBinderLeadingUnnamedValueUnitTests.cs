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



        private T ArrangeAndAct<T>() where T : TargetBase
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddUnnamedArgument("files")
                    .AddArgument("r")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recycle", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
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

        #endregion
    }
}
