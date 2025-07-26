using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    public enum ByteOrders
    {
        LittleEndian,
        BigEndian,
        System
    }

    public class ProtocolReader
    {
        public static readonly Encoding DefaultTextEncoding = Encoding.UTF8;

        private readonly Stream _stream;
        private readonly bool _swapByteOrder;
        private readonly Encoding _textEncoding;

        public ProtocolReader(Stream stream)
            : this(stream, Encoding.UTF8, ByteOrders.System)
        {
        }

        public ProtocolReader(Stream stream, Encoding textEncoding, ByteOrders endianess)
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

        public bool TryReadByte(out byte result)
        {
            result = 0;
            var byteAsInt = _stream.ReadByte();
            if (byteAsInt == -1)
            {
                return false;
            }
            result = (byte)byteAsInt;
            return true;
        }

        public async Task<TryAsyncResult<byte[]>> TryReadBytesAsync(int length)
        {
            var value = new byte[length];
            var valueIndex = 0;
            while (true)
            {
                var bytesToRead = length - valueIndex;
                var bytesRead = await _stream.ReadAsync(value, valueIndex, bytesToRead);
                valueIndex += bytesRead;
                if (bytesRead == 0)
                {
                    var bytesToReturn = value.AsSpan(0, valueIndex).ToArray();
                    return new TryAsyncResult<byte[]>(false, bytesToReturn);
                }
                else if (bytesRead == bytesToRead)
                {
                    return new TryAsyncResult<byte[]>(true, value);
                }
            }
        }

        public async Task<TryAsyncResult<int>> TryReadUntilAsync(byte[] stopIndicator, Stream writeTo,
            bool writeStopIndicator)
        {
            var currentIndex = -1;
            var patternStartsAt = -1;
            var currentPatternIndex = 0;
            while (true)
            {
                currentIndex++;
                if (!TryReadByte(out var byteIn))
                {
                    //  EOF
                    await writeTo.FlushAsync();
                    return new TryAsyncResult<int>(false, -1);
                }
                if (byteIn == stopIndicator[currentPatternIndex])
                {
                    //  This byte is part of the stopIndicator
                    patternStartsAt = patternStartsAt == -1 ? currentIndex : patternStartsAt;
                    currentPatternIndex++;
                    if (currentPatternIndex == stopIndicator.Length)
                    {
                        //  The whole indicator has been read
                        if (writeStopIndicator)
                        {
                            await writeTo.WriteAsync(stopIndicator);
                        }
                        return new TryAsyncResult<int>(true, patternStartsAt);
                    }
                }
                else
                {
                    //  This byte is not part of the stopIndicator. 
                    if (currentPatternIndex != 0)
                    {
                        //  There is a partial stopIndicator "buffered up", so we need to
                        //  write it out before resetting.
                        await writeTo.WriteAsync(stopIndicator, 0, currentPatternIndex + 1);
                        patternStartsAt = -1;
                        currentPatternIndex = 0;
                    }
                    writeTo.WriteByte(byteIn);
                }
            }
        }

        public async Task<TryAsyncResult<ushort>> TryReadUInt16Async()
        {
            var valueAsBytes = new byte[2];
            var valueAsBytesIndex = 0;
            while (true)
            {
                var bytesToRead = sizeof(ushort) - valueAsBytesIndex;
                var bytesRead = await _stream.ReadAsync(valueAsBytes, valueAsBytesIndex, bytesToRead);
                valueAsBytesIndex += bytesRead;
                if (bytesRead == 0)
                {
                    return new TryAsyncResult<ushort>(false, ushort.MinValue);
                }
                else if (bytesRead == bytesToRead)
                {
                    var result = BitConverter.ToUInt16(valueAsBytes);
                    result = _swapByteOrder ? BinaryPrimitives.ReverseEndianness(result) : result;
                    return new TryAsyncResult<ushort>(true, result);
                }
            }
        }


        private int SeekPatternInBuffer(byte[] buffer, byte[] pattern)
        {
            var patternStartsAt = -1;
            var currentPatternIndex = 0;
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == pattern[currentPatternIndex])
                {
                    patternStartsAt = patternStartsAt == -1 ? i : patternStartsAt;
                    currentPatternIndex++;
                    if (currentPatternIndex == pattern.Length)
                    {
                        return patternStartsAt;
                    }
                }
                else
                {
                    patternStartsAt = -1;
                    currentPatternIndex = 0;
                }
            }
            return -1;
        }
    }
}
