namespace JPC.Backup.UnitTests.BackupAPI
{
    [TestClass]
    public class ExcludeIfSourcePathMatchesRegexTests
    {
        [TestMethod]
        public void Excludes_on_case_sensitive_match()
        {
            var sourcePath = @"C:\Users\You\Documents\Finance\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\You\Documents\Finance\Budget.xls";
            var expression = @"\\Finance\\";

            IExcludeRule testee = new ExcludeIfSourcePathMatchesRegex(expression);

            Assert.IsTrue(testee.ExcludeObject(sourcePath, destinationPath));
        }

        [TestMethod]
        public void Excludes_on_case_insensitive_match()
        {
            var sourcePath = @"C:\Users\You\Documents\Finance\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\You\Documents\Finance\Budget.xls";

            //
            //  (?i) makes the expression case-insensitive.
            var expression = @"(?i)\\finance\\";

            IExcludeRule testee = new ExcludeIfSourcePathMatchesRegex(expression);

            Assert.IsTrue(testee.ExcludeObject(sourcePath, destinationPath));
        }

        [TestMethod]
        public void Does_not_exclude_on_case_sensitive_non_match()
        {
            var sourcePath = @"C:\Users\You\Documents\Finance\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\You\Documents\Finance\Budget.xls";
            var expression = @"\\finance\\";

            IExcludeRule testee = new ExcludeIfSourcePathMatchesRegex(expression);

            Assert.IsFalse(testee.ExcludeObject(sourcePath, destinationPath));
        }

        [TestMethod]
        public void Does_not_exclude_on_case_insensitive_non_match()
        {
            var sourcePath = @"C:\Users\You\Documents\Finance\Budget.xls";
            var destinationPath = @"D:\Backup\C\Users\You\Documents\Finance\Budget.xls";
            var expression = @"(?i)\\Finnance\\";

            IExcludeRule testee = new ExcludeIfSourcePathMatchesRegex(expression);

            Assert.IsFalse(testee.ExcludeObject(sourcePath, destinationPath));
        }

    }
}
