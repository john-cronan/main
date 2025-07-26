using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    internal static class Binary
    {
        private static readonly IDictionary<char, byte> _hexMap
            = new Dictionary<char, byte>
            {
                {'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 },
                {'5', 5 }, {'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 },
                {'a', 10 }, {'A', 10 }, {'b', 11 }, {'B', 11 }, {'c', 12 },
                {'C', 12 }, {'d', 13 }, {'D', 13 }, {'e', 14 }, {'E', 14 },
                {'f', 15 }, {'F', 15 }
            };

        private static readonly IDictionary<byte, char> _reverseMap
            = new Dictionary<byte, char>
            {
                { 0,'0' }, { 1,'1' }, { 2,'2' }, { 3,'3' },
                { 4,'4' }, { 5,'5' }, { 6,'6' }, { 7,'7' },
                { 8,'8' }, { 9,'9' }, { 10,'a' }, { 11,'b' },
                {12,'c' }, { 13,'d' }, { 14,'e' }, { 15,'f' }
            };

        public static IEnumerable<byte> FromBase64(string base64)
        {
            Guard.IsNotNullOrWhitespace(base64, nameof(base64));

            return Convert.FromBase64String(base64);
        }

        public static IEnumerable<byte> FromHex(string hex)
        {
            Guard.IsNotNullOrWhitespace(hex, nameof(hex));

            var startIndex = 0;
            if (hex.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                startIndex = 2;
            }
            var modulus = 0;
            if ((hex.Length - startIndex) % 2 == 0)
            {
                modulus = 1;
            }
            byte currentByte = 0;
            for (int i = startIndex; i < hex.Length; i++)
            {
                if (!_hexMap.ContainsKey(hex[i]))
                {
                    throw new ArgumentException($"The character '{hex[i]}' is not a valid hexadecimal character");
                }
                currentByte = (byte)(currentByte | _hexMap[hex[i]]);
                if (i % 2 == modulus)
                {
                    yield return currentByte;
                    currentByte = 0;
                }
                else
                {
                    currentByte = (byte)(currentByte << 4);
                }
            }
        }

        public static IEnumerable<char> ToHexChars(IEnumerable<byte> bytes, bool includePreamble)
        {
            Guard.IsNotNullOrEmpty(bytes, nameof(bytes));

            if (includePreamble)
            {
                yield return '0';
                yield return 'x';
            }
            foreach (var b in bytes)
            {
                yield return _reverseMap[(byte)(b >> 4)];
                yield return _reverseMap[(byte)(b & 0xf)];
            }
        }

        public static string ToBase64(IEnumerable<byte> bytes)
        {
            return Convert.ToBase64String(bytes.ToArray());
        }

        public static string ToHex(IEnumerable<byte> bytes, bool includePreamble)
        {
            Guard.IsNotNullOrEmpty(bytes, nameof(bytes));

            var chars = ToHexChars(bytes, includePreamble).ToArray();
            return new string(chars);
        }
    }
}
