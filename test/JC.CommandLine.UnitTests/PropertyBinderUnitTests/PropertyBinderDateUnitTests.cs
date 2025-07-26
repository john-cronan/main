using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderDateUnitTests
    {
        [TestMethod]
        public void Binds_to_array_target()
        {
            var instance = ArrangeAndAct<ArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("some_script.cmd", instance.Command);
            Assert.AreEqual(3, instance.Dates.Length);
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_list_target()
        {
            var instance = ArrangeAndAct<ListTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("some_script.cmd", instance.Command);
            Assert.AreEqual(3, instance.Dates.Count);
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_immutable_array_target()
        {
            var instance = ArrangeAndAct<ImmutableArrayTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("some_script.cmd", instance.Command);
            Assert.AreEqual(3, instance.Dates.Length);
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates[0]);
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates[1]);
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates[2]);
        }

        [TestMethod]
        public void Binds_to_Enumerable_target()
        {
            var instance = ArrangeAndAct<EnumerableTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("some_script.cmd", instance.Command);
            Assert.AreEqual(3, instance.Dates.Count());
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates.ElementAt(0));
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates.ElementAt(1));
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates.ElementAt(2));
        }

        [TestMethod]
        public void Binds_to_read_only_collection_target()
        {
            var instance = ArrangeAndAct<ReadOnlyCollectionTarget>();
            Assert.IsNotNull(instance.Command);
            Assert.AreEqual("some_script.cmd", instance.Command);
            Assert.AreEqual(3, instance.Dates.Count());
            Assert.AreEqual(DateTime.Parse("11/1/2019"), instance.Dates.ElementAt(0));
            Assert.AreEqual(DateTime.Parse("12/1/2019"), instance.Dates.ElementAt(1));
            Assert.AreEqual(DateTime.Parse("1/1/2020"), instance.Dates.ElementAt(2));
        }

        private T ArrangeAndAct<T>()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("Program.exe")
                    .AddArgument("c", "some_script.cmd")
                    .AddArgument("d", "11/1/2019", "12/1/2019", "1/1/2020")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(ImmutableArray<string>.Empty.Add("Command"),
                    ArgumentMultiplicity.One, true),
                new Argument(ImmutableArray<string>.Empty.Add("Dates"),
                    ArgumentMultiplicity.OneOrMore, false)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Stem, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var instance = testee.CreateObject<T>(resolution);
            return instance;
        }

        private class ArrayTarget
        {
            public string Command { get; set; }
            public DateTime[] Dates { get; set; }
        }

        private class ListTarget
        {
            public string Command { get; set; }
            public List<DateTime> Dates { get; set; }
        }

        private class ImmutableArrayTarget
        {
            public string Command { get; set; }
            public ImmutableArray<DateTime> Dates { get; set; }
        }

        private class EnumerableTarget
        {
            public string Command { get; set; }
            public IEnumerable<DateTime> Dates { get; set; }
        }

        private class ReadOnlyCollectionTarget
        {
            public string Command { get; set; }
            public IReadOnlyCollection<DateTime> Dates { get; set; }
        }
    }
}
