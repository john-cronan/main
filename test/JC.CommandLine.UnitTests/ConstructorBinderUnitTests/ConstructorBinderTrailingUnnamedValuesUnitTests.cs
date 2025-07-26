using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderTrailingUnnamedValuesUnitTests
    {
        [TestMethod]
        public void Assigns_trailing_unnamed_values_to_ImmutableArray()
        {
            var result = ArrangeAndAct<TrailingUnnamedValuesImmutableArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(2, result.TrailingUnnamedValues.Length);
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValues[0]);
            Assert.AreEqual("SomeOtherFile.txt", result.TrailingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_trailing_unnamed_values_to_Array()
        {
            var result = ArrangeAndAct<TrailingUnnamedValuesArrayTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(2, result.TrailingUnnamedValues.Length);
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValues[0]);
            Assert.AreEqual("SomeOtherFile.txt", result.TrailingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_trailing_unnamed_values_to_List()
        {
            var result = ArrangeAndAct<TrailingUnnamedValuesListTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(2, result.TrailingUnnamedValues.Count);
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValues[0]);
            Assert.AreEqual("SomeOtherFile.txt", result.TrailingUnnamedValues[1]);
        }

        [TestMethod]
        public void Assigns_trailing_unnamed_values_to_IEnumerable()
        {
            var result = ArrangeAndAct<TrailingUnnamedValuesEnumerableTarget>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(2, result.TrailingUnnamedValues.Count());
            Assert.AreEqual("SomeFile.txt", result.TrailingUnnamedValues.ElementAt(0));
            Assert.AreEqual("SomeOtherFile.txt", result.TrailingUnnamedValues.ElementAt(1));
        }

        [TestMethod]
        public void Assigns_trailing_unnambed_values_following_single_valued_argument()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddUnnamedArgument("delete")
                    .AddUnnamedArgument("files")
                    .AddArgument("matching", "somepattern")
                    .AddUnnamedArgument("SomeFile.txt")
                    .AddUnnamedArgument("SomeOtherFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Matching", ArgumentMultiplicity.One, false)
            }.ToImmutableArray();
            var argumentDelimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, argumentDelimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var result = testee.CreateObject<TrailingUnnamedValuesImmutableArrayTarget>(resolution);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TrailingUnnamedValues);
            Assert.AreEqual(2, result.TrailingUnnamedValues.Count());
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

        private class TrailingUnnamedValuesImmutableArrayTarget : TargetBase
        {
            private readonly ImmutableArray<string> _trailingUnnamedValues;
            public TrailingUnnamedValuesImmutableArrayTarget(ImmutableArray<string> trailingUnnamedValues)
            {
                _trailingUnnamedValues = trailingUnnamedValues;
            }
            public ImmutableArray<string> TrailingUnnamedValues => _trailingUnnamedValues;
        }

        private class TrailingUnnamedValuesArrayTarget : TargetBase
        {
            private readonly string[] _trailingUnnamedValues;
            public TrailingUnnamedValuesArrayTarget(string[] trailingUnnamedValues)
            {
                _trailingUnnamedValues = trailingUnnamedValues;
            }
            public string[] TrailingUnnamedValues => _trailingUnnamedValues;
        }

        private class TrailingUnnamedValuesListTarget : TargetBase
        {
            private readonly List<string> _trailingUnnamedValues;
            public TrailingUnnamedValuesListTarget(List<string> trailingUnnamedValues)
            {
                _trailingUnnamedValues = trailingUnnamedValues;
            }
            public List<string> TrailingUnnamedValues => _trailingUnnamedValues;
        }

        private class TrailingUnnamedValuesEnumerableTarget : TargetBase
        {
            private readonly IEnumerable<string> _trailingUnnamedValues;
            public TrailingUnnamedValuesEnumerableTarget(IEnumerable<string> trailingUnnamedValues)
            {
                _trailingUnnamedValues = trailingUnnamedValues;
            }
            public IEnumerable<string> TrailingUnnamedValues => _trailingUnnamedValues;
        }

        #endregion
    }
}