using System;
using System.Collections.Generic;
using System.Text;

namespace PDISTD
{
    public static class Misc
    {
        public static int RandomUpdateOld1(ref uint value)
        {
            uint v1 = 17 * value + 17;
            value = v1;

            uint low = (v1 << 16);
            uint high = (v1 & 0xFFFF0000) >> 16;

            uint swapped = low + high;

            return (int)(v1 ^ swapped);
        }

        public static void r_shufflebit(Memory<byte> buffer, int size, MTRandom randomizer)
            => Shuffle(buffer, 8 * size, randomizer, swapbit);

        public static void Shuffle(Memory<byte> buffer, int size, MTRandom randomizer, 
            Action<Memory<byte>, int, int> shuffler)
        {
            int max = size - 1;

            short[] temp;
            if (size == 1)
                temp = new short[0];
            else
                temp = new short[size];

            if (size != 1)
            {
                for (int i = max; i > 0; i--)
                {
                    float randVal = randomizer.getFloat();

                    int h = i + 1;
                    float index;
                    if (h < 0)
                        index = randVal * ((h & 1 | (h >> 1)) + (h & 1 | (h >> 1)));
                    else
                        index = randVal * h;

                    temp[i - 1] = (short)index;
                }
            }

            if (size != 1)
            {
                int cPos = 0;
                for (int i = 1; i < size; i++)
                {
                    int pos = temp[cPos++];
                    shuffler(buffer, i, pos);
                }
            }


        }

        public static void swapbit(Memory<byte> data, int oldIndex, int newIndex)
        {
            int indexA = oldIndex >> 3;
            int posA = newIndex >> 3;
            int indexB = oldIndex & 7;
            int posB = newIndex & 7;

            if (oldIndex != newIndex)
            {
                byte old = data.Span[posA];

                uint v9 = 1u << posB;
                uint v10 = 1u << indexB;

                int v11 = ~(1 << posB);
                int v12 = ~(1 << indexB);

                bool unkBool = (old & v9) != 0;

                byte unk;
                if ((data.Span[indexA] & v10) != 0)
                    unk = (byte)(data.Span[posA] | v9);
                else
                    unk = (byte)(data.Span[posA] & v11);

                data.Span[posA] = unk;

                if (unkBool)
                    data.Span[indexA] |= (byte)v10;
                else
                    data.Span[indexA] &= (byte)v12;
            }
        }
    }
}
