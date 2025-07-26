using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderDoubleUnitTests
    {
        [TestMethod]
        public void Binds_to_array_target()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("print", instance.Command);
            Assert.AreEqual(3, instance.Constants.Length);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_list_target()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("print", instance.Command);
            Assert.AreEqual(3, instance.Constants.Count);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_immutable_array_target()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("print", instance.Command);
            Assert.AreEqual(3, instance.Constants.Length);
            Assert.AreEqual(3.14159, instance.Constants[0], .0001);
            Assert.AreEqual(2.718, instance.Constants[1], .0001);
            Assert.AreEqual(1.414, instance.Constants[2], .0001);
        }

        [TestMethod]
        public void Binds_to_Enumerable_target()
        {
            var instance = ArrangeAndAct<EnumerableTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("print", instance.Command);
            Assert.AreEqual(3, instance.Constants.Count());
            Assert.AreEqual(3.14159, instance.Constants.ElementAt(0), .0001);
            Assert.AreEqual(2.718, instance.Constants.ElementAt(1), .0001);
            Assert.AreEqual(1.414, instance.Constants.ElementAt(2), .0001);
        }

        [TestMethod]
        public void Binds_to_read_only_collection_target()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("print", instance.Command);
            Assert.AreEqual(3, instance.Constants.Count());
            Assert.AreEqual(3.14159, instance.Constants.ElementAt(0), .0001);
            Assert.AreEqual(2.718, instance.Constants.ElementAt(1), .0001);
            Assert.AreEqual(1.414, instance.Constants.ElementAt(2), .0001);
        }


        private T ArrangeAndAct<T>()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Command", "print")
                    .AddArgument("Constants", "3.14159", "2.718", "1.414")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(ImmutableArray<string>.Empty.Add("Command"),
                    ArgumentMultiplicity.One, true),
                new Argument(ImmutableArray<string>.Empty.Add("Constants"),
                    ArgumentMultiplicity.OneOrMore, false)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var instance = testee.CreateObject<T>(resolution);
            return instance;
        }

        private class ArrayTarget
        {
            public string Command { get; set; }
            public double[] Constants { get; set; }
        }

        private class ListTarget
        {
            public string Command { get; set; }
            public List<double> Constants { get; set; }
        }

        private class ImmutableArrayTarget
        {
            public string Command { get; set; }
            public ImmutableArray<double> Constants { get; set; }
        }

        private class EnumerableTarget
        {
            public string Command { get; set; }
            public IEnumerable<double> Constants { get; set; }
        }

        private class ReadOnlyCollectionTarget
        {
            public string Command { get; set; }
            public IReadOnlyCollection<double> Constants { get; set; }
        }
    }
}
