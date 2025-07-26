using System.Text;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class ProtocolBufferReaderTests
    {
        [TestMethod]
        public void ReadBytes_reads_requested_number_of_bytes()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 1);
            var actual = testee.ReadBytes(3);

            Assert.IsTrue(new byte[] { 0x02, 0x03, 0x04 }.SequenceEqual(actual));
        }

        [TestMethod]
        public void ReadBytes_throws()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 2);

            try
            {
                var actual = testee.ReadBytes(3);
                Assert.Fail();
            } catch (ArgumentOutOfRangeException) 
            { 
            }

        }

        [TestMethod]
        public void ReadNullTerminatedAsciiString_reads_string()
        {
            var bytes = new byte[] { 0x01, 0x61, 0x62, 0x63, 0x0, 0x02, 0x03};
            var testee = new ProtocolBufferReader (bytes, 1);
            var str = testee.ReadNullTerminatedAsciiString();

            Assert.AreEqual("abc", str);
        }

        [TestMethod]
        public void ReadNullTerminatedAsciiString_reads_to_end_of_string()
        {
            var bytes = new byte[] { 0x01, 0x61, 0x62, 0x63 };
            var testee = new ProtocolBufferReader(bytes, 1);
            var str = testee.ReadNullTerminatedAsciiString();

            Assert.AreEqual("abc", str);
        }

        [TestMethod]
        public void ReadUInt16_reads_from_middle_of_byte_array()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 1);
            var actual = testee.ReadUInt16();

            Assert.AreEqual(0x0302, actual);
        }

        [TestMethod]
        public void ReadUInt16_reads_from_end_of_byte_array()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 2);
            var actual = testee.ReadUInt16();

            Assert.AreEqual(0x0403, actual);
        }

        [TestMethod]
        public void ReadUInt16_throws()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 3);

            try
            {
                var actual = testee.ReadUInt16();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [TestMethod]
        public void ReadUInt16_swaps_byte_order()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var testee = new ProtocolBufferReader(bytes, 1, BitConverter.IsLittleEndian ? ByteOrders.BigEndian : ByteOrders.LittleEndian);
            var actual = testee.ReadUInt16();

            Assert.AreEqual(0x0203, actual);
        }
    }
}
