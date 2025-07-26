using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common.Internal
{
    internal class CryptographyService : ICryptographyService
    {
        private string _standardSymmetricAlgorithmName;
        private string _standardHashAlgorithmName;
        private Encoding _textEncoding;

        public CryptographyService()
        {
            _standardHashAlgorithmName = HashAlgorithmName.SHA256.Name;
            _standardSymmetricAlgorithmName = "AES";
            _textEncoding = Encoding.UTF8;
        }

        string ICryptographyService.StandardSymmetricAlgorithmName
        {
            get { return _standardSymmetricAlgorithmName; }
            set { _standardSymmetricAlgorithmName = value; }
        }

        string ICryptographyService.StandardHashAlgorithmName
        {
            get { return _standardHashAlgorithmName; }
            set { _standardHashAlgorithmName = value; }
        }

        Encoding ICryptographyService.TextEncoding
        {
            get { return _textEncoding; }
            set { _textEncoding = value; }
        }


        byte[] ICryptographyService.ComputeHash(byte[] input)
            => (this as ICryptographyService).ComputeHash(_standardHashAlgorithmName, input);

        byte[] ICryptographyService.ComputeHash(string hashAlgorithmName, byte[] input)
        {
            if (string.IsNullOrWhiteSpace(hashAlgorithmName))
            {
                throw new ArgumentNullException(nameof(hashAlgorithmName));
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
            return hashAlgorithm.ComputeHash(input);
        }

        byte[] ICryptographyService.ComputeHash(Stream input)
            => (this as ICryptographyService).ComputeHash(_standardHashAlgorithmName, input);

        byte[] ICryptographyService.ComputeHash(string hashAlgorithmName, Stream input)
        {
            if (string.IsNullOrWhiteSpace(hashAlgorithmName))
            {
                throw new ArgumentNullException(nameof(hashAlgorithmName));
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (!input.CanRead)
            {
                throw new ArgumentException("Specified stream is not readable", nameof(input));
            }

            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
            return hashAlgorithm.ComputeHash(input);
        }

        Task<byte[]> ICryptographyService.ComputeHashAsync(Stream input)
            => (this as ICryptographyService).ComputeHashAsync(_standardHashAlgorithmName, input);

        Task<byte[]> ICryptographyService.ComputeHashAsync(string hashAlgorithmName, Stream input)
        {
            if (string.IsNullOrWhiteSpace(hashAlgorithmName))
            {
                throw new ArgumentNullException(nameof(hashAlgorithmName));
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (!input.CanRead)
            {
                throw new ArgumentException("Specified stream is not readable", nameof(input));
            }

            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
            return hashAlgorithm.ComputeHashAsync(input);
        }


        byte[] ICryptographyService.DeriveKey(string password, byte[] salt, int outputLength)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (outputLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputLength));
            }

            return HKDF.DeriveKey(new HashAlgorithmName(_standardHashAlgorithmName), _textEncoding.GetBytes(password),
                outputLength, salt, Array.Empty<byte>());
        }


        int ICryptographyService.Encrypt(Stream streamIn, Stream streamOut, byte[] key, byte[] iv)
            => (this as ICryptographyService).Encrypt(_standardSymmetricAlgorithmName, streamIn, streamOut, key, iv);

        int ICryptographyService.Encrypt(string symmetricAlgorithmName, Stream streamIn, Stream streamOut, byte[] key, byte[] iv)
        {
            if (string.IsNullOrWhiteSpace(symmetricAlgorithmName))
            {
                throw new ArgumentNullException(nameof(symmetricAlgorithmName));
            }
            if (streamIn == null)
            {
                throw new ArgumentNullException(nameof(streamIn));
            }
            if (streamOut == null)
            {
                throw new ArgumentNullException(nameof(streamOut));
            }
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var bufferedStreamIn = streamIn is BufferedStream ? streamIn : new BufferedStream(streamIn);
            var bufferedStreamOut = streamOut is BufferedStream ? streamOut : new BufferedStream(streamOut);
            try
            {
                return EncryptInternal(symmetricAlgorithmName, bufferedStreamIn, bufferedStreamOut, key,
                    iv, (this as ICryptographyService).GenerateRandomBytes());
            }
            finally
            {
                bufferedStreamOut.Flush();
            }
        }

        byte[] ICryptographyService.Encrypt(byte[] dataIn, byte[] key, byte[] iv)
            => (this as ICryptographyService).Encrypt(_standardSymmetricAlgorithmName, dataIn, key, iv);

        byte[] ICryptographyService.Encrypt(string symmetricAlgorithmName, byte[] dataIn, byte[] key, byte[] iv)
        {
            if (string.IsNullOrWhiteSpace(symmetricAlgorithmName))
            {
                throw new ArgumentNullException(nameof(symmetricAlgorithmName));
            }
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException(nameof(key));
            }

            using var dataInAsStream = new MemoryStream(dataIn);
            using var dataOutAsStream = new MemoryStream();
            EncryptInternal(symmetricAlgorithmName, dataInAsStream, dataOutAsStream, key, iv,
                (this as ICryptographyService).GenerateRandomBytes());
            return dataOutAsStream.ReadAllBytes();

        }


        IEnumerable<byte> ICryptographyService.GenerateRandomBytes()
        {
            using var rng = RandomNumberGenerator.Create();
            while (true)
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                foreach (var b in bytes)
                {
                    yield return b;
                }
            }
        }


        private static int EncryptInternal(string symmetricAlgorithmName, Stream streamIn, Stream streamOut, byte[] key,
            byte[] iv, IEnumerable<byte> randomGenerator)
        {
            var totalByteCount = 0;
            using var symmetricAlgorithm = SymmetricAlgorithm.Create(symmetricAlgorithmName);
            var effectiveIV =
                iv == null || iv.Length == 0
                    ? randomGenerator.Take((symmetricAlgorithm.BlockSize / 8)).ToArray()
                    : iv; var encryptor = symmetricAlgorithm.CreateEncryptor(key, effectiveIV);
            var keyStream = CounterModeKeyStream.GetEnumerable(encryptor, effectiveIV).GetEnumerator();
            var byteInAsInt = 0;
            while ((byteInAsInt = streamIn.ReadByte()) >= 0)
            {
                keyStream.MoveNext();
                var byteIn = (byte)byteInAsInt;
                var byteOut = (byte)(byteIn ^ keyStream.Current);
                streamOut.WriteByte(byteOut);
                totalByteCount++;
            }
            return totalByteCount;
        }
    }
}
