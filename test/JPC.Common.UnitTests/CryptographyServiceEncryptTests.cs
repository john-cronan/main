using System.Reflection;
using System.Security.Cryptography;
using JPC.Common.Internal;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class CryptographyServiceEncryptTests
    {
        private ICryptographyService _testee = new CryptographyService();

        [TestInitialize]
        public void TestInitialize()
        {
            _testee = new CryptographyService();
        }

        [TestMethod]
        public void Encrypt_bytes_is_deterministic()
        {
            var plaintextOne = ReadResourceAsBytes("FileList.gz");
            var plaintextTwo = plaintextOne.ToArray();
            var key = TestingEncryptionKeys.KeysOf256Bits[0];
            var iv = TestingEncryptionKeys.IVsOf128Bits[0];
            var aes = SymmetricAlgorithm.Create("AES");
            var ciphertextOne = _testee.Encrypt(plaintextOne, key, iv);
            var ciphertextTwo = _testee.Encrypt(plaintextTwo, key, iv);

            Assert.IsTrue(ciphertextOne.SequenceEqual(ciphertextTwo));
        }

        [TestMethod]
        public void Encrypt_stream_is_deterministic()
        {
            var plaintextOne = ReadResourceAsStream("FileList.gz");
            var plaintextTwo = ReadResourceAsStream("FileList.gz");
            var key = TestingEncryptionKeys.KeysOf128Bits[0];
            var iv = TestingEncryptionKeys.IVsOf128Bits[0];
            var aes = SymmetricAlgorithm.Create("AES");
            using var ciphertextOne = new MemoryStream();
            using var ciphertextTwo = new MemoryStream();

            _testee.Encrypt(plaintextOne, ciphertextOne, key, iv);
            _testee.Encrypt(plaintextTwo, ciphertextTwo, key, iv);

            var ciphertextOneAsBytes = ciphertextOne.ReadAllBytes();
            var ciphertextTwoAsBytes = ciphertextTwo.ReadAllBytes();
            Assert.IsTrue(ciphertextOneAsBytes.SequenceEqual(ciphertextTwoAsBytes));

        }

        [TestMethod]
        public void Encrypt_bytes_is_function_of_IV()
        {
            var plaintextOne = ReadResourceAsBytes("FileList.gz");
            var plaintextTwo = plaintextOne.ToArray();
            var key = TestingEncryptionKeys.KeysOf256Bits[0];
            var ivOne = TestingEncryptionKeys.IVsOf128Bits[0];
            var ivTwo = TestingEncryptionKeys.IVsOf128Bits[1];
            var aes = SymmetricAlgorithm.Create("AES");
            var ciphertextOne = _testee.Encrypt(plaintextOne, key, ivOne);
            var ciphertextTwo = _testee.Encrypt(plaintextTwo, key, ivTwo);

            Assert.IsFalse(ciphertextOne.SequenceEqual(ciphertextTwo));
        }

        [TestMethod]
        public void Encrypt_bytes_is_function_of_key()
        {
            var plaintextOne = ReadResourceAsBytes("FileList.gz");
            var plaintextTwo = plaintextOne.ToArray();
            var keyOne = TestingEncryptionKeys.KeysOf256Bits[0];
            var keyTwo = TestingEncryptionKeys.KeysOf256Bits[1];
            var iv = TestingEncryptionKeys.IVsOf128Bits[0];
            var aes = SymmetricAlgorithm.Create("AES");
            var ciphertextOne = _testee.Encrypt(plaintextOne, keyOne, iv);
            var ciphertextTwo = _testee.Encrypt(plaintextTwo, keyTwo, iv);

            Assert.IsFalse(ciphertextOne.SequenceEqual(ciphertextTwo));
        }

        [TestMethod]
        public void Encrypt_bytes_is_round_trippable()
        {
            var plaintext = ReadResourceAsBytes("FileList.gz");
            using var plaintextAsStream = new MemoryStream(plaintext);
            var key = TestingEncryptionKeys.KeysOf128Bits[0];
            var iv = TestingEncryptionKeys.IVsOf128Bits[0];
            var aes = SymmetricAlgorithm.Create("AES");
            using var ciphertext = new MemoryStream();
            _testee.Encrypt(plaintextAsStream, ciphertext, key, iv);
            var ciphertextAsBytes = ReadAllBytes(ciphertext);
            Assert.IsFalse(plaintext.SequenceEqual(ciphertextAsBytes));
            using var recoveredPlaintext = new MemoryStream();
            _testee.Encrypt(ciphertext, recoveredPlaintext, key, iv);
            var recoveredPlaintextAsBytes = ReadAllBytes(recoveredPlaintext);
            Assert.IsTrue(plaintext.SequenceEqual(recoveredPlaintextAsBytes));
        }


        private byte[] ReadAllBytes(MemoryStream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var allBytes = new byte[memoryStream.Length];
            memoryStream.Read(allBytes, 0, allBytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return allBytes;
        }

        private Stream ReadResourceAsStream(string partialName)
        {
            var resourceName = GetResourceNameFromPartialName(partialName);
            return GetType().Assembly.GetManifestResourceStream(resourceName);
        }

        private byte[] ReadResourceAsBytes(string partialName)
        {
            using var resourceStream = ReadResourceAsStream(partialName);
            var bytes = new byte[resourceStream.Length];
            resourceStream.Read(bytes, 0, bytes.Length);
            return bytes;
        }


        private string GetResourceNameFromPartialName(string partialName)
            => (from asm in new Assembly[] { GetType().Assembly }
               from resourceName in asm.GetManifestResourceNames()
               where resourceName.Contains(partialName)
               select resourceName).Single();

    }
}
