using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace JPC.Common.Internal
{
    internal class CompressionService : ICompressionService
    {
        async Task ICompressionService.CompressAsync(Stream inputData, Stream writeCompressedDataTo)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException(nameof(inputData));
            }
            if (writeCompressedDataTo == null)
            {
                throw new ArgumentNullException(nameof(writeCompressedDataTo));
            }
            if (!inputData.CanRead)
            {
                throw new ArgumentException("Cannot read from input stream");
            }
            if (!writeCompressedDataTo.CanWrite)
            {
                throw new ArgumentException("Cannot write to output stream");
            }

            var effectiveInputData =
                inputData is BufferedStream || inputData is MemoryStream
                    ? inputData
                    : new BufferedStream(inputData);
            var effectiveOutputData =
                writeCompressedDataTo is BufferedStream || writeCompressedDataTo is MemoryStream
                    ? writeCompressedDataTo
                    : new BufferedStream(writeCompressedDataTo);
            using var compressionStream = new GZipStream(effectiveOutputData, CompressionMode.Compress, true);
            var byteAsInt = 0;
            while ((byteAsInt = effectiveInputData.ReadByte()) != -1)
            {
                compressionStream.WriteByte((byte)byteAsInt);
            }
            await effectiveOutputData.FlushAsync();
            await compressionStream.FlushAsync();
        }

        async Task<byte[]> ICompressionService.CompressAsync(byte[] sourceData)
        {
            if (sourceData == null || sourceData.Length == 0)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            using var sourceDataAsStream = new MemoryStream(sourceData);
            using var outputDataAsStream = new MemoryStream();
            await (this as ICompressionService).CompressAsync(sourceDataAsStream, outputDataAsStream);
            var outputDataAsBytes = new byte[outputDataAsStream.Length];
            outputDataAsStream.Seek(0, SeekOrigin.Begin);
            await outputDataAsStream.ReadAsync(outputDataAsBytes, 0, outputDataAsBytes.Length);
            return outputDataAsBytes;
        }

        async Task<byte[]> ICompressionService.ExpandAsync(byte[] sourceData)
        {
            if (sourceData == null || sourceData.Length == 0)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            using var sourceDataAsStream = new MemoryStream(sourceData);
            using var destinationDataAsStream = new MemoryStream();
            await (this as ICompressionService).ExpandAsync(sourceDataAsStream, destinationDataAsStream);
            destinationDataAsStream.Seek(0, SeekOrigin.Begin);
            var destinationDataAsBytes = new byte[destinationDataAsStream.Length];
            await destinationDataAsStream.ReadAsync(destinationDataAsBytes, 0, destinationDataAsBytes.Length);
            return destinationDataAsBytes;
        }

        async Task ICompressionService.ExpandAsync(Stream compressedData, Stream writeExpandedDataTo)
        {
            if (compressedData == null)
            {
                throw new ArgumentNullException(nameof(compressedData));
            }
            if (writeExpandedDataTo == null)
            {
                throw new ArgumentNullException(nameof(writeExpandedDataTo));
            }
            if (!compressedData.CanRead)
            {
                throw new ArgumentException("Cannot read from input stream");
            }
            if (!writeExpandedDataTo.CanWrite)
            {
                throw new ArgumentException("Cannot write to output stream");
            }

            using var decompressionStream = new GZipStream(compressedData, CompressionMode.Decompress);
            decompressionStream.CopyTo(writeExpandedDataTo);
            await writeExpandedDataTo.FlushAsync();
        }
    }
}
