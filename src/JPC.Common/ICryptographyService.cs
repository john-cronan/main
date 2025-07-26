using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    public interface ICryptographyService
    {
        string StandardSymmetricAlgorithmName { get; set; }
        string StandardHashAlgorithmName { get; set; }
        Encoding TextEncoding { get; set; }

        byte[] ComputeHash(byte[] input);
        byte[] ComputeHash(string hashAlgorithmName, byte[] input);
        byte[] ComputeHash(Stream input);
        byte[] ComputeHash(string hashAlgorithmName, Stream input);
        Task<byte[]> ComputeHashAsync(Stream input);
        Task<byte[]> ComputeHashAsync(string hashAlgorithmName, Stream input);

        byte[] DeriveKey(string password, byte[] salt, int outputLength);

        int Encrypt(Stream streamIn, Stream streamOut, byte[] key, byte[] iv);
        int Encrypt(string symmetricAlgorithmName, Stream streamIn, Stream streamOut, byte[] key, byte[] iv);
        byte[] Encrypt(byte[] dataIn, byte[] key, byte[] iv);
        byte[] Encrypt(string symmetricAlgorithmName, byte[] dataIn, byte[] key, byte[] iv);

        IEnumerable<byte> GenerateRandomBytes();
    }
}
