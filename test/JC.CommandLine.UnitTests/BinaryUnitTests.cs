using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class BinaryUnitTests
    {
        [TestMethod]
        public void All_zeros_even_number_of_chars_no_preamble()
        {
            var hex = "000000";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(3, bits.Length);
            Assert.IsTrue(bits.All(b => b == 0));
        }

        [TestMethod]
        public void All_zeros_odd_number_of_chars_no_preamble()
        {
            var hex = "00000";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(3, bits.Length);
            Assert.IsTrue(bits.All(b => b == 0));
        }

        [TestMethod]
        public void All_zeros_odd_chars_preamble()
        {
            var hex = "0x00000";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(3, bits.Length);
            Assert.IsTrue(bits.All(b => b == 0));
        }

        [TestMethod]
        public void All_hex_chars_no_preamble()
        {
            var hex = "000102030405060708090a0b0c0d0e0f";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(16, bits.Length);
            Assert.IsTrue(bits.Select((b, i) => bits[i] == b).All(b => b));
        }

        [TestMethod]
        public void All_hex_chars_preamble()
        {
            var hex = "0x000102030405060708090a0b0c0d0e0f";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(16, bits.Length);
            Assert.IsTrue(bits.Select((b, i) => bits[i] == b).All(b => b));
        }

        [TestMethod]
        public void All_hex_chars_compacted_lower_case()
        {
            var hex = "0x0123456789abcdef";
            var bits = Binary.FromHex(hex).ToArray();
            Assert.IsNotNull(bits);
            Assert.AreEqual(8, bits.Length);
            Assert.AreEqual(0x01, bits[0]);
            Assert.AreEqual(0x23, bits[1]);
            Assert.AreEqual(0x45, bits[2]);
            Assert.AreEqual(0x67, bits[3]);
            Assert.AreEqual(0x89, bits[4]);
            Assert.AreEqual(0xab, bits[5]);
            Assert.AreEqual(0xcd, bits[6]);
            Assert.AreEqual(0xef, bits[7]);
        }

        [TestMethod]
        public void Long_hex_string_from_character_string()
        {
            var encoding = Encoding.UTF8;
            var expectedString = "There is no dark side on the moon, really. As a matter of fact it's all dark.";
            var expectedBytes = encoding.GetBytes(expectedString);
            var asHex = Binary.ToHex(expectedBytes, true);
            var actualBytes = Binary.FromHex(asHex).ToArray();
            var actualString = encoding.GetString(actualBytes);
            Assert.AreEqual(expectedString, actualString);
        }

        [TestMethod]
        public void Throws_on_base_64()
        {
            var encoding = Encoding.Unicode;
            var expectedString = "There is no dark side on the moon, really. As a matter of fact, it's all dark.";
            var expectedBytes = encoding.GetBytes(expectedString);
            var asBase64 = Convert.ToBase64String(expectedBytes);
            try
            {
                Binary.FromHex(asBase64).ToArray();
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
