namespace JPC.Common.UnitTests
{
    [TestClass]
    public class EnumerableExtensionsSelectBitsTests
    {
        [TestMethod]
        public void All_zero_bytes_selects_all_false_bits()
        {
            var bytes = Enumerable.Range(0, 1000).Select(i => (byte)0x0);
            Assert.IsTrue(bytes.SelectBits().All(bit => !bit));
        }

        [TestMethod]
        public void All_ff_bytes_selects_all_true_bits()
        {
            var bytes = Enumerable.Range(0, 1000).Select(i => (byte)0xff);
            Assert.IsTrue(bytes.SelectBits().All(bit => bit));
        }
    }
}
