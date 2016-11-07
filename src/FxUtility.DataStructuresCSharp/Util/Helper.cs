using System.Collections;
using System.Collections.Generic;

namespace DataStructuresCSharp.Util
{
    public static class Helper
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        public static byte ToByte(this IEnumerable<bool> bits)
        {
            var item = 0;
            var bitIndex = 0;
            foreach (var bit in bits)
            {
                if (bit) item |= 1 << (7 - bitIndex);
                ++bitIndex;
                if (bitIndex == 8) break;
            }
            return (byte)item;
        }

        private static byte[] ToByteArray(IEnumerable<bool> bits, int count)
        {
            var numBytes = count / 8;
            if (count % 8 != 0) numBytes++;

            var bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            foreach (var bit in bits)
            {
                if (bit) bytes[byteIndex] |= (byte)(1 << bitIndex);
                ++bitIndex;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    ++byteIndex;
                }

            }
            return bytes;
        }

        public static byte[] ToByteArray(this bool[] bits) => ToByteArray(bits, bits.Length);

        public static byte[] ToByteArray(this List<bool> bits) => ToByteArray(bits, bits.Count);


    }
}
