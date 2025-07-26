using System;
using System.Text;

namespace JPC.Common
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Remove(this StringBuilder str, Func<char, bool> predicate)
        {
            int i = 0;
            while (i < str.Length)
            {
                if (predicate(str[i]))
                {
                    str.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }
            return str;
        }

        public static StringBuilder Trim(this StringBuilder str)
        {
            return TrimEnd(TrimStart(str));
        }

        public static StringBuilder TrimEnd(this StringBuilder str)
        {
            var indexOfLastNonWhitespaceChar = str.Length - 1;
            while (indexOfLastNonWhitespaceChar >= 0 && Char.IsWhiteSpace(str[indexOfLastNonWhitespaceChar]))
            {
                indexOfLastNonWhitespaceChar--;
            }
            if (indexOfLastNonWhitespaceChar < str.Length - 1)
            {
                str.Remove(indexOfLastNonWhitespaceChar + 1, str.Length - indexOfLastNonWhitespaceChar - 1);
            }
            return str;
        }

        public static StringBuilder TrimStart(this StringBuilder str)
        {
            var indexOfFirstNonWhitespaceChar = 0;
            while (indexOfFirstNonWhitespaceChar < str.Length && Char.IsWhiteSpace(str[indexOfFirstNonWhitespaceChar]))
            {
                indexOfFirstNonWhitespaceChar++;
            }
            if (indexOfFirstNonWhitespaceChar > 0)
            {
                str.Remove(0, indexOfFirstNonWhitespaceChar);
            }
            return str;
        }
    }
}
