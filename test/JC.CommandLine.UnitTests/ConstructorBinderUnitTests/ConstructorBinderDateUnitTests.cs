using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderDateUnitTests
    {

        [TestMethod]
        public void Binds_to_scalar()
        {
            var instance = ArrangeAndAct<ScalarTarget>();
            Assert.AreEqual(DateTime.Parse("12/13/1984 12:34:00"), instance.Start);
        }

        [TestMethod]
        public void Binds_to_array_target()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.AreEqual(3, instance.Dates.Count());
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_list_target()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.AreEqual(3, instance.Dates.Count);
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_immutable_array_target()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.AreEqual(3, instance.Dates.Length);
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_Enumerable_target()
        {
            var instance = ArrangeAndAct<EnumerableTarget>();
            Assert.AreEqual(3, instance.Dates.Count());
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates.ElementAt(0));
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates.ElementAt(1));
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates.ElementAt(2));
        }

        [TestMethod]
        public void Binds_to_read_only_collection_target()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionTarget>();
            Assert.AreEqual(3, instance.Dates.Count());
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates.ElementAt(0));
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates.ElementAt(1));
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates.ElementAt(2));
        }
               

        private T ArrangeAndAct<T>() where T : ConstructorBinderTarget
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("c", "some_script.cmd")
                    .AddArgument("d", "11/1/2019", "12/1/2019", "1/1/2020")
                    .AddArgument("s", "12/13/1984 12:34:00")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("Command", ArgumentMultiplicity.One, true),
                new Argument("Dates", ArgumentMultiplicity.OneOrMore, false),
                new Argument("Start", ArgumentMultiplicity.One, false)
            }.ToImmutableArray();
            var delimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var instance = testee.CreateObject<T>(resolution);
            return (T)instance;
        }

        #region "  Target classes  "

        private abstract class ConstructorBinderTarget
        {
        }

        private class ScalarTarget : ConstructorBinderTarget
        {
            private readonly DateTime _start;

            public ScalarTarget(DateTime start)
            {
                _start = start;
            }

            public DateTime Start => _start;
        }

        private class ArrayTarget : ConstructorBinderTarget
        {
            private readonly DateTime[] _dates;

            public ArrayTarget(DateTime[] dates)
            {
                _dates = dates;
            }

            public DateTime[] Dates => _dates;
        }

        private class ListTarget : ConstructorBinderTarget
        {
            private readonly List<DateTime> _dates;

            public ListTarget(List<DateTime> dates)
            {
                _dates = dates;
            }

            public List<DateTime> Dates => _dates;
        }

        private class ImmutableArrayTarget : ConstructorBinderTarget
        {
            private readonly ImmutableArray<DateTime> _dates;

            public ImmutableArrayTarget(ImmutableArray<DateTime> dates)
            {
                _dates = dates;
            }

            public ImmutableArray<DateTime> Dates => _dates;
        }

        private class EnumerableTarget : ConstructorBinderTarget
        {
            private readonly IEnumerable<DateTime> _dates;

            public EnumerableTarget(IEnumerable<DateTime> dates)
            {
                _dates = dates;
            }

            public IEnumerable<DateTime> Dates => _dates;
        }

        private class ReadOnlyCollectionTarget : ConstructorBinderTarget
        {
            private readonly IReadOnlyCollection<DateTime> _dates;

            public ReadOnlyCollectionTarget(IReadOnlyCollection<DateTime> dates)
            {
                _dates = dates;
            }

            public IReadOnlyCollection<DateTime> Dates => _dates;
        }

        #endregion

    }
}
