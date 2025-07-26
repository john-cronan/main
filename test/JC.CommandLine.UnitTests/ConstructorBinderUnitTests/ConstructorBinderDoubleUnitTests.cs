using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderDoubleUnitTests
    {
        [TestMethod]
        public void Binds_to_scalar_target()
        {
            var instance = ArrangeAndAct<ScalarTarget>();
            Assert.AreEqual(.001, instance.Precision);
        }

        [TestMethod]
        public void Binds_to_array_target()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.AreEqual(3, instance.Constants.Length);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_list_target()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.AreEqual(3, instance.Constants.Count);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_immutable_array_target()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.AreEqual(3, instance.Constants.Length);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_Enumerable_target()
        {
            var instance = ArrangeAndAct<EnumerableTarget>();
            Assert.AreEqual(3, instance.Constants.Count());
            Assert.AreEqual(3.14159, instance.Constants.ElementAt(0), .0001);
            Assert.AreEqual(2.718, instance.Constants.ElementAt(1), .0001);
            Assert.AreEqual(1.414, instance.Constants.ElementAt(2), .0001);
        }

        [TestMethod]
        public void Binds_to_read_only_collection_target()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionTarget>();
            Assert.AreEqual(3, instance.Constants.Count());
            Assert.AreEqual(3.14159, instance.Constants.ElementAt(0), .0001);
            Assert.AreEqual(2.718, instance.Constants.ElementAt(1), .0001);
            Assert.AreEqual(1.414, instance.Constants.ElementAt(2), .0001);
        }



        private T ArrangeAndAct<T>() where T : DoubleUnitTestTarget
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Command", "print")
                    .AddArgument("Constants", "3.14159", "2.718", "1.414")
                    .AddArgument("Precision", ".001")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Command", ArgumentMultiplicity.One, true),
                new Argument("Constants", ArgumentMultiplicity.OneOrMore, false),
                new Argument("Precision", ArgumentMultiplicity.One, false)
            }.ToImmutableArray();
            var delimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var instance = testee.CreateObject<T>(resolution);
            return (T)instance;
        }

        #region "  Target classes  "

        private abstract class DoubleUnitTestTarget
        {
        }

        private class ScalarTarget : DoubleUnitTestTarget
        {
            private readonly double _precision;

            public ScalarTarget(double precision)
            {
                _precision = precision;
            }

            public double Precision => _precision;
        }

        private class ArrayTarget : DoubleUnitTestTarget
        {
            private readonly double[] _constants;

            public ArrayTarget(double[] constants)
            {
                _constants = constants;
            }

            public double[] Constants => _constants;
        }

        private class ListTarget : DoubleUnitTestTarget
        {
            private readonly List<double> _constants;

            public ListTarget(List<double> constants)
            {
                _constants = constants;
            }

            public List<double> Constants => _constants;
        }

        private class ImmutableArrayTarget : DoubleUnitTestTarget
        {
            private readonly ImmutableArray<double> _constants;

            public ImmutableArrayTarget(ImmutableArray<double> constants)
            {
                _constants = constants;
            }

            public ImmutableArray<double> Constants => _constants;
        }

        private class EnumerableTarget : DoubleUnitTestTarget
        {
            private readonly IEnumerable<double> _constants;

            public EnumerableTarget(IEnumerable<double> constants)
            {
                _constants = constants;
            }
            public IEnumerable<double> Constants => _constants;
        }

        private class ReadOnlyCollectionTarget : DoubleUnitTestTarget
        {
            private readonly IReadOnlyCollection<double> _constants;

            public ReadOnlyCollectionTarget(IReadOnlyCollection<double> constants)
            {
                _constants = constants;
            }

            public IReadOnlyCollection<double> Constants => _constants;
        }

        #endregion
    }
}
