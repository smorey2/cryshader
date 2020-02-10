using System;
using System.Collections.Generic;
using ColorF = UnityEngine.Color;

namespace CryShader.Core
{
    public static class Extensions
    {
        public static void Memset(this List<byte> array, int offset, byte value, int num)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            const int blockSize = 4096; // bigger may be better to a certain extent
            int index = offset;
            int length = Math.Min(blockSize, num);
            while (index < length)
                array[index++] = value;
            length = num;
            while (index < length)
            {
                //Buffer.BlockCopy(array, offset, array, offset + index, Math.Min(blockSize, length - index));
                index += blockSize;
            }
        }

        public static void Memset(this byte[] array, int offset, byte value, int num)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            const int blockSize = 4096; // bigger may be better to a certain extent
            int index = offset;
            int length = Math.Min(blockSize, num);
            while (index < length)
                array[index++] = value;
            length = num;
            while (index < length)
            {
                Buffer.BlockCopy(array, offset, array, offset + index, Math.Min(blockSize, length - index));
                index += blockSize;
            }
        }

        public static void ScaleCol(this ColorF source, float scale)
        {
            source = new ColorF();
        }
    }
}
