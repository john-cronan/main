using JC.CommandLine.TargetTypeConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Text;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class BinaryValueConverterUnitTests
    {
        private readonly string _stringValue = "And in the end, the love you make is equal to the love you take.";
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly byte[] _byteArrayValue;
        private readonly string _base64Value;
        private readonly string _hexValueWithPreamble;
        private readonly string _hexValueWithoutPreamble;

        public BinaryValueConverterUnitTests()
        {
            _byteArrayValue = _encoding.GetBytes(_stringValue);
            _base64Value = Binary.ToBase64(_byteArrayValue);
            _hexValueWithPreamble = Binary.ToHex(_byteArrayValue, true);
            _hexValueWithoutPreamble = Binary.ToHex(_byteArrayValue, false);    
        }

        [TestMethod]
        public void Returns_unsuccessful_if_scalar_type_is_not_byte()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_hexValueWithPreamble, 
                typeof(int[]), ArgumentFlags.None);
            Assert.IsNotNull(conversionResult);
            Assert.IsFalse(conversionResult.Success);
        }

        [TestMethod]
        public void Decodes_hex_with_preamble_if_AssumeHex_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_hexValueWithPreamble,
                typeof(byte[]), ArgumentFlags.AssumeHexadecimal);
            Assert.IsNotNull(conversionResult);
            Assert.IsNotNull(conversionResult.Result);
            Assert.IsTrue(conversionResult.Result.Any());
            var resultAsBytes = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(_byteArrayValue.SequenceEqual(resultAsBytes));
        }

        [TestMethod]
        public void Decodes_hex_without_preamble_if_AssumeHex_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_hexValueWithoutPreamble,
                typeof(byte[]), ArgumentFlags.AssumeHexadecimal);
            Assert.IsNotNull(conversionResult);
            Assert.IsNotNull(conversionResult.Result);
            Assert.IsTrue(conversionResult.Result.Any());
            var resultAsBytes = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(_byteArrayValue.SequenceEqual(resultAsBytes));
        }

        [TestMethod]
        public void Decodes_base64_if_AssumeBase64_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_base64Value,
                typeof(byte[]), ArgumentFlags.AssumeBase64);
            Assert.IsNotNull(conversionResult);
            Assert.IsNotNull(conversionResult.Result);
            Assert.IsTrue(conversionResult.Result.Any());
            var resultAsBytes = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(_byteArrayValue.SequenceEqual(resultAsBytes));
        }

        [TestMethod]
        public void Rejects_base64_if_AssumeHex_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_base64Value,
                typeof(byte[]), ArgumentFlags.AssumeHexadecimal);
            Assert.IsNotNull(conversionResult);
            Assert.IsFalse(conversionResult.Success);
        }

        [TestMethod]
        public void Decodes_hex_if_no_flags_and_preamble_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_hexValueWithPreamble,
                typeof(byte[]), ArgumentFlags.None);
            Assert.IsNotNull(conversionResult);
            Assert.IsTrue(conversionResult.Success);
            var resultAsByteArray = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(_byteArrayValue.SequenceEqual(resultAsByteArray));
        }

        [TestMethod]
        public void Falls_back_to_base64_if_no_flags_and_no_preamble_is_specified()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert(_base64Value,
                typeof(byte[]), ArgumentFlags.None);
            Assert.IsNotNull(conversionResult);
            Assert.IsTrue(conversionResult.Success);
            var resultAsByteArray = conversionResult.Result.Cast<byte>();
            Assert.IsTrue(_byteArrayValue.SequenceEqual(resultAsByteArray));
        }

        [TestMethod]
        public void Fails_on_garbage_instead_of_hex()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert("I've been over the edge for yonks",
                typeof(byte[]), ArgumentFlags.AssumeHexadecimal);
            Assert.IsNotNull(conversionResult);
            Assert.IsFalse(conversionResult.Success);
        }

        [TestMethod]
        public void Fails_on_garbage_instead_of_base64()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert("I've been over the edge for yonks",
                typeof(byte[]), ArgumentFlags.AssumeBase64);
            Assert.IsNotNull(conversionResult);
            Assert.IsFalse(conversionResult.Success);
        }

        [TestMethod]
        public void Converts_single_byte_to_byte()
        {
            var otherConverters = new Mock<ITargetTypeConverterInstances>();
            var filesystem = new Mock<IFilesystem>();
            var testee = new BinaryValueConverter(otherConverters.Object, filesystem.Object);
            var conversionResult = testee.TryConvert("0x20", typeof(byte), 
                ArgumentFlags.AssumeHexadecimal);
            Assert.IsNotNull(conversionResult);
            Assert.IsTrue(conversionResult.Success);
            Assert.AreEqual(0x20, (byte)conversionResult.Result.Single());
        }
    }
}
