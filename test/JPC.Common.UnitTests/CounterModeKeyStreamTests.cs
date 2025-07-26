using System.Security.Cryptography;
using JPC.Common.Internal;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class CounterModeKeyStreamTests
    {
        [TestMethod]
        public void Increment_only_increments_one_byte()
        {
            var startingValue = new byte[] { 1, 0, 0 };
            var incrementedValue = startingValue.ToArray();
            CounterModeKeyStream.Increment(incrementedValue);
            var expected = new byte[] { 2, 0, 0 };
            Assert.IsTrue(expected.SequenceEqual(incrementedValue));
        }

        [TestMethod]
        public void Increments_all_bits_set()
        {
            var actual = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            CounterModeKeyStream.Increment(actual);
            var expected = new byte[] { 0, 0, 0, 0 };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Increment_rolls_over_one_byte()
        {
            var actual = new byte[] { 0xFF, 0, 0, 0 };
            CounterModeKeyStream.Increment(actual);
            var expected = new byte[] { 0, 1, 0, 0 };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void KeyStream_is_deterministic()
        {
            var key = TestingEncryptionKeys.KeysOf128Bits[0];
            var iv = TestingEncryptionKeys.IVsOf128Bits[0];

            using var aes = SymmetricAlgorithm.Create("AES");
            var one = EnumerateKeyStream(aes, key, 32);
            var two = EnumerateKeyStream(aes, key, 32);

            Assert.IsTrue(one.Any(b => b != 0));
            Assert.IsTrue(two.Any(b => b != 0));
            Assert.IsTrue(one.SequenceEqual(two));
        }

        [TestMethod]
        public void KeyStream_is_unique()
        {
            using var aes = SymmetricAlgorithm.Create("AES");
            var keyStreams =
                (from key in TestingEncryptionKeys.KeysOf256Bits
                 select EnumerateKeyStream(aes, key, 32)).ToArray();
            using var sha = HashAlgorithm.Create(HashAlgorithmName.SHA256.Name);
            var hashesAsStrings =
                keyStreams
                    .Select(ks => sha.ComputeHash(ks))
                    .Select(h => Convert.ToBase64String(h))
                    .ToArray();
            var hashCount = hashesAsStrings.Count();
            var distinctCount = hashesAsStrings.Distinct().Count();

            Assert.IsTrue(hashCount == distinctCount);
        }

        private byte[] EnumerateKeyStream(SymmetricAlgorithm algorithm, byte[] key, int lengthToOutput)
        {
            var blockSizeInBytes = algorithm.BlockSize / 8;
            var singleBlock = key.Take(blockSizeInBytes).ToArray();
            var encryptor = algorithm.CreateEncryptor(key, singleBlock);
            return CounterModeKeyStream.GetEnumerable(encryptor, singleBlock)
                .Take(lengthToOutput)
                .ToArray();
        }
    }
}
