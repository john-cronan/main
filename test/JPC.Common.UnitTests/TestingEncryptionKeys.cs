namespace JPC.Common.UnitTests
{
    internal class TestingEncryptionKeys
    {
        public static readonly byte[][] KeysOf128Bits = GenerateArray(128, 10);
        public static readonly byte[][] KeysOf256Bits = GenerateArray(256, 10);

        public static readonly byte[][] IVsOf128Bits = KeysOf128Bits;

        private static byte[][] GenerateArray(int keyLengthInBits, int howMany)
        {
            var keyLengthInBytes = keyLengthInBits / 8;
            return
                (from repeatedValue in Enumerable.Range(0, howMany)
                 select Repeat(repeatedValue, keyLengthInBytes)).ToArray();
        }

        private static byte[] Repeat(int value, int times)
            => Enumerable.Range(0, times).Select(i => (byte)value).ToArray();
    }
}
