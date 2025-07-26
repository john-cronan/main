using System;
using System.Buffers.Binary;
using System.Text;

namespace JPC.Common
{
    public class ProtocolBufferReader
    {
        private readonly byte[] _bytes;
        private int _index;
        private bool _swapByteOrder;

        public ProtocolBufferReader(byte[] bytes, int startingIndex)
        {
            _bytes = bytes;
            _index = startingIndex;
            _swapByteOrder = false;
        }

        public ProtocolBufferReader(byte[] bytes, int startingIndex, ByteOrders endianess)
            : this(bytes, startingIndex)
        {
            _swapByteOrder = endianess switch
            {
                ByteOrders.System => false,
                ByteOrders.LittleEndian => !BitConverter.IsLittleEndian,
                ByteOrders.BigEndian => BitConverter.IsLittleEndian,
                _ => throw new ArgumentException("Invalid endianess value")
            };
        }

        public byte ReadByte()
        {
            return _bytes[_index++];
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (_index + count > _bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var bytesOut = new byte[count];
            Array.Copy(_bytes, _index, bytesOut, 0, count);
            _index += count;
            return bytesOut;
        }

        public string ReadNullTerminatedAsciiString()
        {
            var indexOfTerminator = _index;
            while (indexOfTerminator < _bytes.Length && _bytes[indexOfTerminator] != 0) indexOfTerminator++;
            var str = Encoding.ASCII.GetString(_bytes, _index, indexOfTerminator - _index);
            _index = indexOfTerminator;
            return str;
        }

        public ushort ReadUInt16()
        {
            if (_index + sizeof(UInt16) > _bytes.Length)
                throw new InvalidOperationException("No enough bytes remaining");

            var result = BitConverter.ToUInt16(_bytes, _index);
            _index += sizeof(UInt16);
            return _swapByteOrder ? BinaryPrimitives.ReverseEndianness(result) : result;
        }
    }
}
