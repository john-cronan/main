using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace JC.CommandLine.UnitTests.ConstructorBinderUnitTests
{
    [TestClass]
    public class ConstructorBinderBoolUnitTests
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
                new Argument("Directory", ArgumentMultiplicity.One, true),
                new Argument("Recurse", ArgumentMultiplicity.Zero, false),
                new Argument("Force", ArgumentMultiplicity.Zero, false),
                new Argument("Recycle", ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var delimitters = "-/".ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, false,
                NameMatchingOptions.Exact, true, '@');
            var resolution = new ActualModelResolution(actuals, model);
            IObjectBinder testee = new ConstructorBinder();
            var instance = testee.CreateObject<FlagsTarget>(resolution);
            Assert.IsTrue(instance.Force);
            Assert.IsTrue(instance.Recurse);
            Assert.IsFalse(instance.Recycle);
        }

        private class FlagsTarget
        {
            private readonly bool _recurse;
            private readonly bool _force;
            private readonly bool _recycle;

            public FlagsTarget(bool recurse, bool force, bool recycle)
            {
                _recurse = recurse;
                _force = force;
                _recycle = recycle;
            }

            public bool Recurse => _recurse;

            public bool Force => _force;

            public bool Recycle => _recycle;
        }
    }
}
