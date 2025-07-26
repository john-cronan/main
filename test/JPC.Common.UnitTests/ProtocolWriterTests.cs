using System.Threading.Tasks;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class ProtocolWriterTests
    {
        [TestMethod]
        public void WriteByte_is_byte_order_invariant()
        {
            var expectedBytes = new byte[] { 0x01, 0x02, 0x03 };
            var littleEndianStream = new MemoryStream();
            var bigEndianStream = new MemoryStream();
            var testeeLittleEndian = new ProtocolWriter(littleEndianStream, ProtocolWriter.DefaultTextEncoding, ByteOrders.LittleEndian);
            var testeeBigEndian = new ProtocolWriter(bigEndianStream, ProtocolWriter.DefaultTextEncoding, ByteOrders.BigEndian);
            foreach (var expectedByte in expectedBytes)
            {
                testeeLittleEndian.WriteByte(expectedByte);
                testeeBigEndian.WriteByte(expectedByte);
            }
            littleEndianStream.Seek(0, SeekOrigin.Begin);
            var actualBytesLittleEndian = new byte[littleEndianStream.Length];
            littleEndianStream.Read(actualBytesLittleEndian, 0, actualBytesLittleEndian.Length);
            bigEndianStream.Seek(0, SeekOrigin.Begin);
            var actualBytesBigEndian = new byte[bigEndianStream.Length];
            bigEndianStream.Read(actualBytesBigEndian, 0, actualBytesBigEndian.Length);

            Assert.IsTrue(actualBytesLittleEndian.SequenceEqual(actualBytesBigEndian));
            Assert.IsTrue(actualBytesLittleEndian.SequenceEqual(expectedBytes));
        }

        [TestMethod]
        public async Task WriteBytes_is_byte_order_invariant()
        {
            var expectedBytes = new byte[] { 0x01, 0x02, 0x03 };
            var testee = new ProtocolWriter(new MemoryStream(), ProtocolWriter.DefaultTextEncoding, ByteOrders.LittleEndian);
            await testee.WriteBytesAsync(expectedBytes);
            var actualBytesLittleEndian = ReadAllBytesFromMemoryStream((MemoryStream)testee.Stream);

            testee = new ProtocolWriter(new MemoryStream(), ProtocolWriter.DefaultTextEncoding, ByteOrders.BigEndian);
            await testee.WriteBytesAsync(expectedBytes);
            var actualBytesBigEndian = ReadAllBytesFromMemoryStream((MemoryStream)testee.Stream);

            Assert.IsTrue(actualBytesLittleEndian.SequenceEqual(actualBytesBigEndian));
            Assert.IsTrue(actualBytesLittleEndian.SequenceEqual(expectedBytes));
        }

        [TestMethod]
        public async Task WriteBytes_writes_subset_of_byte_array()
        {
            var expectedBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var expectedSubset = expectedBytes.Skip(2).ToArray();
            var testee = new ProtocolWriter(new MemoryStream());
            await testee.WriteBytesAsync(expectedBytes, expectedSubset.Length, expectedBytes.Length - expectedSubset.Length);
            var actualBytes = ReadAllBytesFromMemoryStream((MemoryStream)testee.Stream);

            Assert.IsTrue(actualBytes.SequenceEqual(expectedSubset));
        }

        [TestMethod]
        public async Task WriteUInt16Async_is_byte_order_dependent()
        {
            var expected = (ushort)0x0102;
            var testee = new ProtocolWriter(new MemoryStream(), ProtocolWriter.DefaultTextEncoding, ByteOrders.LittleEndian);
            await testee.WriteUInt16Async(expected);
            var bytesLittleEndian = ReadAllBytesFromMemoryStream((MemoryStream)testee.Stream);

            testee = new ProtocolWriter(new MemoryStream(), ProtocolWriter.DefaultTextEncoding, ByteOrders.BigEndian);
            await testee.WriteUInt16Async(expected);
            var bytesBigEndian = ReadAllBytesFromMemoryStream((MemoryStream)testee.Stream);

            Assert.IsFalse(bytesLittleEndian.SequenceEqual(bytesBigEndian));
        }


        private byte[] ReadAllBytesFromMemoryStream(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
