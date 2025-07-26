using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderAllUnnamedValueUnitTests
    {
        [TestMethod]
        public void Assigns_all_unnamed_values_to_ImmutableArray()
        {
            var result = ArrangeAndAct<AllUnnamedValuesImmutableArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Length);
            Assert.AreEqual("delete", result.UnnamedValues[0]);
            Assert.AreEqual("SomeFile.txt", result.UnnamedValues[1]);
            Assert.AreEqual("SomeOtherFile.txt", result.UnnamedValues[2]);
        }

        [TestMethod]
        public void Assigns_all_unnamed_values_to_array()
        {
            var result = ArrangeAndAct<AllUnnamedValuesArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Length);
            Assert.AreEqual("delete", result.UnnamedValues[0]);
            Assert.AreEqual("SomeFile.txt", result.UnnamedValues[1]);
            Assert.AreEqual("SomeOtherFile.txt", result.UnnamedValues[2]);
        }

        [TestMethod]
        public void Assigns_all_unnamed_values_to_list()
        {
            var result = ArrangeAndAct<AllUnnamedValuesListTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Count);
            Assert.AreEqual("delete", result.UnnamedValues[0]);
            Assert.AreEqual("SomeFile.txt", result.UnnamedValues[1]);
            Assert.AreEqual("SomeOtherFile.txt", result.UnnamedValues[2]);
        }

        [TestMethod]
        public void Assigns_all_unnamed_values_to_enumerable_interface()
        {
            var result = ArrangeAndAct<AllUnnamedValuesEnumerableTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UnnamedValues);
            Assert.AreEqual(3, result.UnnamedValues.Count());
            Assert.AreEqual("delete", result.UnnamedValues.ElementAt(0));
            Assert.AreEqual("SomeFile.txt", result.UnnamedValues.ElementAt(1));
            Assert.AreEqual("SomeOtherFile.txt", result.UnnamedValues.ElementAt(2));
        }

        [TestMethod]
        public void Unnamed_values_cannot_be_integers()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("1")
                    .AddArgument("r")
                    .AddUnnamedArgument("2")
                    .AddArgument("f")
                    .AddUnnamedArgument("3")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var argumentDelimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var result = testee.CreateObject<AllUnnamedValuesIntegerTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNull(result.UnnamedValues);
        }


        private T ArrangeAndAct<T>() where T : TargetBase
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

        private class AllUnnamedValuesImmutableArrayTarget : TargetBase
        {
            private readonly ImmutableArray<string> _unnamedValues;
            public AllUnnamedValuesImmutableArrayTarget(ImmutableArray<string> unnamedValues)
            {
                _unnamedValues = unnamedValues;
            }
            public ImmutableArray<string> UnnamedValues => _unnamedValues;
        }

        private class AllUnnamedValuesArrayTarget : TargetBase
        {
            private readonly string[] _unnamedValues;
            public AllUnnamedValuesArrayTarget(string[] unnamedValues)
            {
                _unnamedValues = unnamedValues;
            }
            public string[] UnnamedValues => _unnamedValues;
        }

        private class AllUnnamedValuesListTarget : TargetBase
        {
            private readonly List<string> _unnamedValues;
            public AllUnnamedValuesListTarget(List<string> unnamedValues)
            {
                _unnamedValues = unnamedValues;
            }
            public List<string> UnnamedValues => _unnamedValues;
        }

        private class AllUnnamedValuesEnumerableTarget : TargetBase
        {
            private readonly IEnumerable<string> _unnamedValues;
            public AllUnnamedValuesEnumerableTarget(IEnumerable<string> unnamedValues)
            {
                _unnamedValues = unnamedValues;
            }
            public IEnumerable<string> UnnamedValues => _unnamedValues;
        }

        private class AllUnnamedValuesIntegerTarget
        {
            private readonly int[] _unnamedValues;
            public AllUnnamedValuesIntegerTarget(int[] unnamedValues)
            {
                _unnamedValues = unnamedValues;
            }
            public int[] UnnamedValues => _unnamedValues;
        }

        #endregion
    }
}
