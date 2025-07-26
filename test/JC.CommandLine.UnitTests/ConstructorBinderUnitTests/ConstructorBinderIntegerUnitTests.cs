using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderIntegerUnitTests
    {
        [TestMethod]
        public void Binds_ints_to_immutable_array()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Length);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_array()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Length);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_list()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_IList()
        {
            var instance = ArrangeAndAct<ListInterfaceTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs[0]);
            Assert.AreEqual(4321, instance.ObjectIDs[1]);
            Assert.AreEqual(9999, instance.ObjectIDs[2]);
        }

        [TestMethod]
        public void Binds_ints_to_ICollection()
        {
            var instance = ArrangeAndAct<CollectionInterfaceTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Count);
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        [TestMethod]
        public void Binds_ints_to_IEnumerable()
        {
            var instance = ArrangeAndAct<EnumerableInterfaceTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Count());
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        [TestMethod]
        public void Binds_ints_to_IReadOnlyCollection()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionInterfaceTarget>();
            Assert.AreEqual(3, instance.ObjectIDs.Count());
            Assert.AreEqual(1234, instance.ObjectIDs.ElementAt(0));
            Assert.AreEqual(4321, instance.ObjectIDs.ElementAt(1));
            Assert.AreEqual(9999, instance.ObjectIDs.ElementAt(2));
        }

        private T ArrangeAndAct<T>() where T : IntegerUnitTestTarget
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
            IObjectBinder testee = new ConstructorBinder();
            var instance = testee.CreateObject<T>(resolution);
            return instance;
        }

        #region Target classes

        private abstract class IntegerUnitTestTarget
        {
        }

        private class ImmutableArrayTarget : IntegerUnitTestTarget
        {
            private readonly ImmutableArray<int> _objectIds;

            public ImmutableArrayTarget(ImmutableArray<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public ImmutableArray<int> ObjectIDs => _objectIds;
        }

        private class ArrayTarget : IntegerUnitTestTarget
        {
            private readonly int[] _objectIds;

            public ArrayTarget(int[] objectIds)
            {
                _objectIds = objectIds;
            }

            public int[] ObjectIDs => _objectIds;
        }

        private class ListTarget : IntegerUnitTestTarget
        {
            private readonly List<int> _objectIds;

            public ListTarget(List<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public List<int> ObjectIDs => _objectIds;
        }

        private class ListInterfaceTarget : IntegerUnitTestTarget
        {
            private readonly IList<int> _objectIds;

            public ListInterfaceTarget(IList<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public IList<int> ObjectIDs => _objectIds;
        }

        private class CollectionInterfaceTarget : IntegerUnitTestTarget
        {
            private readonly ICollection<int> _objectIds;

            public CollectionInterfaceTarget(ICollection<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public ICollection<int> ObjectIDs => _objectIds;
        }

        private class EnumerableInterfaceTarget : IntegerUnitTestTarget
        {
            private readonly IEnumerable<int> _objectIds;

            public EnumerableInterfaceTarget(IEnumerable<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public IEnumerable<int> ObjectIDs => _objectIds;
        }

        private class ReadOnlyCollectionInterfaceTarget : IntegerUnitTestTarget
        {
            private readonly IReadOnlyCollection<int> _objectIds;

            public ReadOnlyCollectionInterfaceTarget(IReadOnlyCollection<int> objectIds)
            {
                _objectIds = objectIds;
            }

            public IReadOnlyCollection<int> ObjectIDs => _objectIds;
        }

        #endregion

    }
}
