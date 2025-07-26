using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace JC.CommandLine.UnitTests.PropertyBinderUnitTests
{
    [TestClass]
    public class PropertyBinderBoolUnitTests
    {
        [TestMethod]
        public void Binds_flags()
        {
            var actuals =
                new CommandLineBuilder()
                    .AddExeNode("DeleteTree.exe")
                    .AddArgument("Directory", @"C:\Windows")
                    .AddArgument("Recurse")
                    .AddArgument("Force")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(ImmutableArray<string>.Empty.Add("Directory"),
                    ArgumentMultiplicity.One, true),
                new Argument(ImmutableArray<string>.Empty.Add("Recurse"),
                    ArgumentMultiplicity.Zero, false),
                new Argument(ImmutableArray<string>.Empty.Add("Force"),
                    ArgumentMultiplicity.Zero, false),
                new Argument(ImmutableArray<string>.Empty.Add("Recycle"),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var delimitters = new char[] { '-', '/' }.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new PropertyBinder();
            var instance = testee.CreateObject<CommandLineFlags>(resolution);
            Assert.IsTrue(instance.Force);
            Assert.IsTrue(instance.ForceSet);
            Assert.IsTrue(instance.Recurse);
            Assert.IsTrue(instance.RecurseSet);
            Assert.IsFalse(instance.Recycle);
            Assert.IsFalse(instance.RecycleSet);
        }

        private class CommandLineFlags
        {
            private bool _recurse;
            private bool _recurseSet;
            private bool _force;
            private bool _forceSet;
            private bool _recycle;
            private bool _recycleSet;

            public bool Recurse
            {
                get { return _recurse; }
                set
                {
                    _recurse = value;
                    _recurseSet = true;
                }
            }

            public bool Force
            {
                get { return _force; }
                set
                {
                    _force = value;
                    _forceSet = true;
                }
            }

            public bool Recycle
            {
                get { return _recycle; }
                set
                {
                    _recycle = value;
                    _recycleSet = true;
                }
            }

            public bool RecurseSet { get => _recurseSet; }
            public bool ForceSet { get => _forceSet; }
            public bool RecycleSet { get => _recycleSet; }
        }
    }
}
