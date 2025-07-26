using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace JPC.Common.Internal
{
    internal static class CounterModeKeyStream
    {
        public static IEnumerable<byte> GetEnumerable(ICryptoTransform cryptoTransform, byte[] initialCounterValue)
        {
            var input = initialCounterValue.ToArray();
            var output = new byte[input.Length];
            while (true)
            {
                cryptoTransform.TransformBlock(input, 0, input.Length, output, 0);
                for (int i = 0; i < output.Length; i++)
                    yield return output[i];
                Increment(input);
            }
        }

        internal static void Increment(byte[] bytes)
        {
            var index = 0;
            var shouldContinue = true;
            do
            {
                var byteAsInt = (int)bytes[index];
                bytes[index] = (byte)((byteAsInt + 1) % 0x100);
                shouldContinue = bytes[index] == 0 && index + 1 < bytes.Length;
                index++;
            } while (shouldContinue);
        }
    }
}
