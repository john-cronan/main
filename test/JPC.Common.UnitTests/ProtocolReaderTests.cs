using System.Buffers.Binary;
using System.Threading;
using Moq;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class ProtocolReaderTests
    {
        [TestMethod]
        public void TryReadByte_is_byte_order_invariant()
        {
            var message = new byte[] { 0x03 };
            using var messageStream = new MemoryStream(message);
            var testee = new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.LittleEndian);
            messageStream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(testee.TryReadByte(out var actualLittle));
            testee = new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.BigEndian);
            messageStream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(testee.TryReadByte(out var actualBig));
            testee = new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.System);
            messageStream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(testee.TryReadByte(out var actualSystem));

            Assert.AreEqual(0x03, actualLittle);
            Assert.IsTrue(actualLittle == actualBig);
            Assert.IsTrue(actualBig == actualSystem);
        }

        [TestMethod]
        public async Task TryReadBytesAsync_returns_true_on_success()
        {
            var actualMessage = new byte[] { 0x01, 0x23, 0x45, 0x67 };
            using var messageStream = new MemoryStream(actualMessage);
            var testee = new ProtocolReader(messageStream);
            var actualResponse = await testee.TryReadBytesAsync(actualMessage.Length);

            Assert.IsTrue(actualResponse.IsSuccessful);
        }

        [TestMethod]
        public async Task TryReadBytesAsync_returns_correct_bytes_on_success()
        {
            var actualMessage = new byte[] { 0x01, 0x23, 0x45, 0x67 };
            using var messageStream = new MemoryStream(actualMessage);
            var testee = new ProtocolReader(messageStream);
            var actualResponse = await testee.TryReadBytesAsync(actualMessage.Length);

            Assert.IsTrue(actualResponse.Result.SequenceEqual(actualMessage));
        }

        [TestMethod]
        public async Task TryReadBytesAsync_returns_false_on_not_enough_bytes()
        {
            var actualMessage = new byte[] { 0x01, 0x23, 0x45, 0x67 };
            using var messageStream = new MemoryStream(actualMessage);
            var testee = new ProtocolReader(messageStream);
            var actualResponse = await testee.TryReadBytesAsync(actualMessage.Length + 1);

            Assert.IsFalse(actualResponse.IsSuccessful);
        }

        [TestMethod]
        public async Task TryReadBytesAsync_returns_false_and_no_bytes_if_none_available()
        {
            var actualMessage = new byte[] { 0x01, 0x23, 0x45, 0x67 };
            using var messageStream = new MemoryStream(actualMessage);
            var testee = new ProtocolReader(messageStream);
            var actualResponse = await testee.TryReadBytesAsync(actualMessage.Length + 1);

            Assert.IsTrue(actualResponse.Result.SequenceEqual(actualMessage));
        }

        [TestMethod]
        public async Task TryReadBytesAsync_reads_twice_if_necessary()
        {
            var messageStream = new Mock<Stream>();
            var readCount = 0;
            messageStream.Setup(m => m.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((Delegate)(Func<byte[], int, int, CancellationToken, Task<int>>)(
                (bytes, index, count, ct) =>
                {
                    readCount++;
                    var returnTask = Task.FromResult(0);
                    if (readCount == 1)
                    {
                        new byte[] { 0x01, 0x02, 0x03 }.CopyTo(bytes, index);
                        returnTask = Task.FromResult(3);
                    }
                    else if (readCount == 2)
                    {
                        new byte[] { 0x04, 0x05 }.CopyTo(bytes, index);
                        returnTask = Task.FromResult(2);
                    }
                    return returnTask;
                }));
            var testee = new ProtocolReader(messageStream.Object);
            var actualBytes = await testee.TryReadBytesAsync(5);

            Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }.SequenceEqual(actualBytes.Result));
            Assert.AreEqual(2, readCount);
        }


        [TestMethod]
        public async Task TryReadUntilAsync_reads_until_pattern()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var pattern = new byte[] { 0x04, 0x05, 0x06 };
            var testee = new ProtocolReader(new MemoryStream(bytes));
            var outputTo = new MemoryStream();
            await testee.TryReadUntilAsync(pattern, outputTo, false);
            outputTo.Seek(0, SeekOrigin.Begin);
            var outputBytes = new byte[outputTo.Length];
            outputTo.Read(outputBytes, 0, outputBytes.Length);

            Assert.AreEqual(3, outputBytes.Length);
            Assert.IsTrue(outputBytes.SequenceEqual(new byte[] { 0x01, 0x02, 0x03 }));
        }

        [TestMethod]
        public async Task TryReadUntilAsync_outputs_pattern()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var pattern = new byte[] { 0x04, 0x05, 0x06 };
            var testee = new ProtocolReader(new MemoryStream(bytes));
            var outputTo = new MemoryStream();
            await testee.TryReadUntilAsync(pattern, outputTo, true);
            outputTo.Seek(0, SeekOrigin.Begin);
            var outputBytes = new byte[outputTo.Length];
            outputTo.Read(outputBytes, 0, outputBytes.Length);

            Assert.AreEqual(6, outputBytes.Length);
            Assert.IsTrue(outputBytes.SequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
        }

        [TestMethod]
        public async Task TryReadUntilAsync_pattern_in_beginning()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var pattern = new byte[] { 0x01, 0x02, 0x03 };
            var testee = new ProtocolReader(new MemoryStream(bytes));
            var outputTo = new MemoryStream();
            await testee.TryReadUntilAsync(pattern, outputTo, true);
            outputTo.Seek(0, SeekOrigin.Begin);
            var outputBytes = new byte[outputTo.Length];
            outputTo.Read(outputBytes, 0, outputBytes.Length);

            Assert.AreEqual(3, outputBytes.Length);
            Assert.IsTrue(outputBytes.SequenceEqual(new byte[] { 0x01, 0x02, 0x03 }));
        }

        [TestMethod]
        public async Task TryReadUntilAsync_outputs_nothing()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var pattern = new byte[] { 0x01, 0x02, 0x03 };
            var testee = new ProtocolReader(new MemoryStream(bytes));
            var outputTo = new MemoryStream();
            await testee.TryReadUntilAsync(pattern, outputTo, false);
            outputTo.Seek(0, SeekOrigin.Begin);

            Assert.AreEqual(0, outputTo.Length);
        }


        [TestMethod]
        public async Task TryReadUInt16Async_is_byte_order_dependent()
        {
            var message = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var messageStream = new MemoryStream(message);
            var actualLittle = await new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.LittleEndian).TryReadUInt16Async();
            messageStream.Seek(0, SeekOrigin.Begin);
            var actualBig = await new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.BigEndian).TryReadUInt16Async();

            Assert.IsFalse(actualLittle.Result == actualBig.Result);
        }

        [TestMethod]
        public async Task TryReadUInt16Async_returns_little_endian_value()
        {
            var message = new byte[] { 0x02, 0x00 };
            var messageStream = new MemoryStream(message);
            var actual = await new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.LittleEndian).TryReadUInt16Async();

            Assert.AreEqual(0x02, actual.Result);
        }

        [TestMethod]
        public async Task TryReadUInt16Async_returns_big_endian_value()
        {
            var message = new byte[] { 0x02, 0x00 };
            var messageStream = new MemoryStream(message);
            var actual = await new ProtocolReader(messageStream, ProtocolReader.DefaultTextEncoding, ByteOrders.BigEndian).TryReadUInt16Async();

            Assert.AreEqual(0x200, actual.Result);
        }
    }
}
