using System.IO;
using System.Threading.Tasks;

namespace JPC.Common
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var returnValue = new byte[stream.Length];
            stream.Read(returnValue, 0, returnValue.Length);
            return returnValue;
        }

        public async static Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var returnValue = new byte[stream.Length];
            await stream.ReadAsync(returnValue, 0, returnValue.Length);
            return returnValue;
        }
    }
}
