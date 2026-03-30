using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandDispatcherUnitTests
    {
        [TestMethod]
        public Task Matches_correct_command()
        {
            var testee = new CommandDispatcher();
            testee.Register("Import", (args) =>
            {
                return Task.FromResult(0);
            });
            testee.Register("Export", (args) =>
            {
                Assert.Fail("Expected: Import command matched");
                return Task.FromResult(0);
            });
            testee.Register("Notify", (args) =>
            {
                Assert.Fail("Expected: Import command matched");
                return Task.FromResult(0);
            });
            var args = new string[] { "Utility.exe", "Import", "-file", "authors.csv" };
            return testee.ExecuteAsync(args);
        }

        [TestMethod]
        public Task Matches_most_specific_command()
        {
            var testee = new CommandDispatcher();
            testee.Register("Import", (args) =>
            {
                Assert.Fail("Expected: Import Authors File command matched");
                return Task.FromResult(0);
            });
            testee.Register(new string[] { "Import", "Authors" }, (args) =>
            {
                Assert.Fail("Expected: Import Authors File command matched");
                return Task.FromResult(0);
            });
            testee.Register(new string[] { "Import", "Authors", "File" }, (args) =>
            {
                return Task.FromResult(0);
            });
            testee.Register("Notify", (args) =>
            {
                Assert.Fail("Expected: Import Authors File command matched");
                return Task.FromResult(0);
            });
            var args = new string[] { "Utility.exe", "Import", "Authors", "File", "-file", "authors.csv" };
            return testee.ExecuteAsync(args);
        }

        [TestMethod]
        public Task Matches_fallback_command()
        {
            var testee = new CommandDispatcher();
            testee.Register("Import", (args) =>
            {
                return Task.FromResult(0);
            });
            testee.Register(new string[] { "Import", "Authors" }, (args) =>
            {
                Assert.Fail("Expected: Import command matched");
                return Task.FromResult(0);
            });
            testee.Register(new string[] { "Import", "Authors", "File" }, (args) =>
            {
                Assert.Fail("Expected: Import command matched");
                return Task.FromResult(0);
            });
            testee.Register("Notify", (args) =>
            {
                Assert.Fail("Expected: Import command matched");
                return Task.FromResult(0);
            });
            var args = new string[] { "Utility.exe", "Import", "-file", "authors.csv" };
            return testee.ExecuteAsync(args);
        }

        [TestMethod]
        public Task Matches_default_command()
        {
            var testee = new CommandDispatcher();
            testee.RegisterDefault((args) =>
            {
                return Task.FromResult(0);
            });
            testee.Register("Import", (args) =>
            {
                Assert.Fail("Expected: Default command matched");
                return Task.FromResult(0);
            });
            testee.Register("Export", (args) =>
            {
                Assert.Fail("Expected: Default command matched");
                return Task.FromResult(0);
            });
            var args = new string[] { "Utility.exe", "Notify", "-msg", "Failed" };
            return testee.ExecuteAsync(args);
        }

        [TestMethod]
        public Task Respects_case_sensitivity()
        {
            var testee = new CommandDispatcher();
            testee.CaseSenitive = true;
            testee.Register("Import", (args) =>
            {
                Assert.Fail("Expected: 'import' command matched");
                return Task.FromResult(0);
            });
            testee.Register("import", (args) =>
            {
                return Task.FromResult(0);
            });
            var args = new string[] { "Utility.exe", "import", "-File", "Titles.csv" };
            return testee.ExecuteAsync(args);
        }
    }
}
