using System.Diagnostics;
using JPC.Common;
using JPC.Common.Internal;
using JPC.Common.Testing;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class PathCanonicalizerTests
    {
        private IFilesystem _windowsFilesystem;
        private IFilesystem _linuxFilesystem;

        [TestInitialize]
        public void TestInitialize()
        {
            var windowsRuntime = new MockRuntime();
            windowsRuntime.Environment.OperatingSystem = OperatingSystem.Windows;
            _windowsFilesystem = new Filesystem(windowsRuntime.Environment.Object);
        
            var linuxRuntime = new MockRuntime();
            linuxRuntime.Environment.OperatingSystem = OperatingSystem.Linux;
            _linuxFilesystem = new Filesystem(linuxRuntime.Environment.Object);
        }

        [TestMethod]
        public void Canonicalize_does_nothing_Windows()
        {
            var input = @"C:\Windows\System32\drivers\etc\hosts";
            var testee = CreateTestee(cfg => 
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Windows;
                cfg.Filesystem.IsPathRootedDelegates(_windowsFilesystem);
                cfg.Filesystem.SplitPathDelegates(_windowsFilesystem);
                cfg.Filesystem.CombinePathReturns(
                    new string[] {"C:\\", "Windows", "System32", "drivers", "etc", "hosts"},
                    input);
            });
            var actual = testee.MakeCanonical(input);
            Assert.AreEqual(input, actual);
        }

        [TestMethod]
        public void Canonicalize_does_nothing_Linux()
        {
            var input = "/dev/disk/by-id";
            var testee = CreateTestee(cfg =>
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Linux;
                cfg.Filesystem.IsPathRootedDelegates(_linuxFilesystem);
                cfg.Filesystem.SplitPathDelegates(_linuxFilesystem);
                cfg.Filesystem.CombinePathReturns(new string[] { "/", "dev", "disk", "by-id" }, input);
            });
            var actual = testee.MakeCanonical(input);
            Assert.AreEqual(input, actual);
        }

        [TestMethod]
        public void Canonicalize_absolute_paths_on_windows()
        {
            var input = @"C:\Windows\.\System32\drivers\etc\..\..\..\..\Windows\System32\drivers\.\.\etc\hosts";
            var expected = @"C:\Windows\System32\drivers\etc\hosts";
            var testee = CreateTestee(cfg => 
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Windows;
                cfg.Filesystem.IsPathRootedDelegates(_windowsFilesystem);
                cfg.Filesystem.SplitPathDelegates(_windowsFilesystem);
                cfg.Filesystem.CombinePathReturns(
                    new string[] { "C:\\", "Windows", "System32", "drivers", "etc", "hosts" },
                    expected);
            });
            var actual = testee.MakeCanonical(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Throws_on_attempt_to_navigate_above_root_Windows()
        {
            var input = @"C:\Windows\..\..";
            var testee = CreateTestee();
            try
            {
                testee.MakeCanonical(input);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [TestMethod]
        public void Canonicalizes_relative_path_Windows()
        {
            var input = @"Program Files\Acme Rockets\Road-Runner Killer\..\Cartoon Bomb";
            var testee = CreateTestee(cfg =>
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Windows;
                cfg.Filesystem.IsPathRootedDelegates(_windowsFilesystem);
                cfg.Filesystem.SplitPathDelegates(_windowsFilesystem);
                cfg.Filesystem.CombinePathDelegates((arg) => string.Join('\\', arg));
            });
            var actual = testee.MakeCanonical(input);
            var expected = @"Program Files\Acme Rockets\Cartoon Bomb";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Canonicalizes_root_directory_Windows()
        {
            var input = @"C:\";
            var testee = CreateTestee(cfg =>
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Windows;
                cfg.Filesystem.IsPathRootedDelegates(_windowsFilesystem);
                cfg.Filesystem.SplitPathDelegates(_windowsFilesystem);
                cfg.Filesystem.CombinePathReturns(new string[] { "C:\\" }, input);
            });
            var actual = testee.MakeCanonical(input);
            Assert.AreEqual(input, actual);
        }

        private PathCanonicalizer CreateTestee()
        {
            var testee = CreateTestee(cfg =>
            {
                cfg.Environment.OperatingSystem = OperatingSystem.Windows;
            });
            return testee;
        }

        private PathCanonicalizer CreateTestee(Action<MockRuntime> configureRuntime)
        {
            var runtime = new MockRuntime();
            configureRuntime(runtime);            
            return new PathCanonicalizer(runtime.Filesystem.Object);
        }
    }
}
