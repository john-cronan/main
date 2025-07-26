using System.Collections.Generic;

namespace JPC.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<bool> SelectBits(this IEnumerable<byte> bytes)
        {
            byte mask = 1;
            var enumerator = bytes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                do
                {
                    var bitValue = enumerator.Current & mask;
                    yield return bitValue == 0 ? false : true;
                    mask = (byte)((mask << 1) % 0xff);
                } while (mask != 1);
            }
        }
    }
}
