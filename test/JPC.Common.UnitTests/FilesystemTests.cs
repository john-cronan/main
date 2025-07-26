using JPC.Common;
using JPC.Common.Internal;
using JPC.Common.Testing;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class FilesystemTests
    {
        [TestMethod]
        public void Split_supports_absolute_windows_paths()
        {
            // var linux = System.Runtime.InteropServices.OSPlatform.Linux;
            // if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            // {
            //     return;
            // }

            var environment = new MockEnvironment();
            environment.OperatingSystem = OperatingSystem.Windows;
            IFilesystem testee = new Filesystem(environment.Object);
            var actual = testee.SplitPath("C:\\boot.ini");
            var expected = new string[] { "C:\\", "boot.ini" };
            Assert.IsTrue(expected.SequenceEqual(actual));

            actual = testee.SplitPath(@"C:\Windows\System32\drivers\etc");
            expected = new string[] { "C:\\", "Windows", "System32", "drivers", "etc" };
            Assert.IsTrue(expected.SequenceEqual(actual));

            actual = testee.SplitPath(@"C:Program Files\SomeCompany\File:WithStream.txt");
            expected = new string[] { "C:", "Program Files", "SomeCompany", "File:WithStream.txt" };
            Assert.IsTrue(expected.SequenceEqual(actual));

            actual = testee.SplitPath(@"Program Files\SomeCompany\File.txt");
            expected = new string[] { "Program Files", "SomeCompany", "File.txt" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Split_supports_absolute_linux_paths()
        {
            var environment = new MockEnvironment();
            environment.OperatingSystem = OperatingSystem.Linux;
            IFilesystem testee = new Filesystem(environment.Object);
            var actual = testee.SplitPath("/dev/hd0");
            var expected = new string[] { "/", "dev", "hd0" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Split_paths_can_be_rejoined_by_join_method()
        {
            IFilesystem testee = new Filesystem(new EnvironmentWrapper());
            var path = "C:\\Program Files\\SomeCompany\\File:WithStream.txt";
            var aplit = testee.SplitPath(path);
            var rejoined = testee.CombinePath(aplit);
            Assert.AreEqual(path, rejoined);
        }
    }
}
