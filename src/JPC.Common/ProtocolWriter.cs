using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    public class ProtocolWriter
    {
        public static readonly Encoding DefaultTextEncoding = Encoding.UTF8;

        private readonly Stream _stream;
        private readonly bool _swapByteOrder;
        private readonly Encoding _textEncoding;

        public ProtocolWriter(Stream stream)
            : this(stream, DefaultTextEncoding, ByteOrders.System)
        {
        }

        public ProtocolWriter(Stream stream, Encoding textEncoding, ByteOrders endianess)
        {
            _stream = stream;
            _textEncoding = textEncoding;
            _swapByteOrder = endianess switch
            {
                ByteOrders.System => false,
                ByteOrders.LittleEndian => !BitConverter.IsLittleEndian,
                ByteOrders.BigEndian => BitConverter.IsLittleEndian,
                _ => throw new ArgumentException("Invalid endianess value")
            };
        }

        
        public Stream Stream => _stream;
        public Encoding TextEncoding => _textEncoding;


        public void WriteByte(byte value)
        {
            Stream.WriteByte(value);
        }

        public async Task WriteBytesAsync(byte[] bytes)
        {
            await Stream.WriteAsync(bytes);
        }

        public Task WriteBytesAsync(byte[] bytes, int index, int length)
        {
            return Stream.WriteAsync(bytes,index, length);
        }

        public Task WriteUInt16Async(ushort i)
        {
            var asBytes = BitConverter.GetBytes(_swapByteOrder ? BinaryPrimitives.ReverseEndianness(i) : i);
            return WriteBytesAsync(asBytes, 0, asBytes.Length);
        }

        public Task WriteUInt32Async(uint i)
        {
            var asBytes = BitConverter.GetBytes(_swapByteOrder ? BinaryPrimitives.ReverseEndianness(i) : i);
            return WriteBytesAsync(asBytes, 0, asBytes.Length);
        }

        public async Task WritePrefixedStringAsync<T>(string value) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var valueAsBytes = TextEncoding.GetBytes(value);
            if (typeof(T) == typeof(byte))
            {
                if (value.Length > byte.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be greater than {byte.MaxValue}");
                }
                WriteByte((byte)value.Length);
            }
            else if (typeof(T) == typeof(ushort))
            {
                if (value.Length > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be greater than {ushort.MaxValue}");
                }
                await WriteUInt16Async((ushort)value.Length);
            }
            else if (typeof(T) == typeof(uint))
            {
                await WriteUInt32Async((uint)value.Length);
            }
            else
            {
                throw new ArgumentException($"Invalid type: {typeof(T).Name}");
            }
            await WriteBytesAsync(valueAsBytes, 0, valueAsBytes.Length);
        }
    }
}
