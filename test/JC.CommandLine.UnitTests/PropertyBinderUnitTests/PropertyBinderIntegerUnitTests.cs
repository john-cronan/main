using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderIntegerUnitTests
    {
        [TestMethod]
        public void Binds_ints_to_immutable_array()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Length);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_array()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Length);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_list()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_IList()
        {
            var instance = ArrangeAndAct<ListInterfaceTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_ICollection()
        {
            var instance = ArrangeAndAct<CollectionInterfaceTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        [TestMethod]
        public void Binds_ints_to_IEnumerable()
        {
            var instance = ArrangeAndAct<EnumerableInterfaceTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Count());
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        [TestMethod]
        public void Binds_ints_to_IReadOnlyCollection()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionInterfaceTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("delete", instance.Command);
            Assert.AreEqual(3, instance.ObjectIDs.Count());
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        private T ArrangeAndAct<T>()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("Command", "delete")
                    .AddArgument("ObjectIDs", "1234", "4321", "9999")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(ImmutableArray<string>.Empty.Add("Command"),
                    ArgumentMultiplicity.One, true),
                new Argument(ImmutableArray<string>.Empty.Add("ObjectIDs"),
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

        private class ImmutableArrayTarget
        {
            public string Command { get; set; }
            public ImmutableArray<int> ObjectIDs { get; set; }
        }

        private class ArrayTarget
        {
            public string Command { get; set; }
            public int[] ObjectIDs { get; set; }
        }

        private class ListTarget
        {
            public string Command { get; set; }
            public List<int> ObjectIDs { get; set; }
        }

        private class ListInterfaceTarget
        {
            public string Command { get; set; }
            public IList<int> ObjectIDs { get; set; }
        }

        private class CollectionInterfaceTarget
        {
            public string Command { get; set; }
            public ICollection<int> ObjectIDs { get; set; }
        }

        private class EnumerableInterfaceTarget
        {
            public string Command { get; set; }
            public IEnumerable<int> ObjectIDs { get; set; }
        }

        private class ReadOnlyCollectionInterfaceTarget
        {
            public string Command { get; set; }
            public IReadOnlyCollection<int> ObjectIDs { get; set; }
        }
    }
}
