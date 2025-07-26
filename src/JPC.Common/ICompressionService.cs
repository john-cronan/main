using System.IO;
using System.Threading.Tasks;

namespace JPC.Common
{
    public interface ICompressionService
    {
        Task CompressAsync(Stream inputData, Stream writeCompressedDataTo);
        Task<byte[]> CompressAsync(byte[] sourceData);
        Task ExpandAsync(Stream compressedData, Stream writeExpandedDataTo);
        Task<byte[]> ExpandAsync(byte[] sourceData);
    }
}
