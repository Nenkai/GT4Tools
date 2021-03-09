using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;

using PDISTD;

namespace GT4Tools
{
    public class GarageFileHeader
    {
        // Base key located in the game data
        public static void decrypt(Memory<byte> buffer, uint baseKey)
        {
            uint ogKey = baseKey;

            int seed1 = Misc.RandomUpdateOld1(ref baseKey);
            int seed2 = Misc.RandomUpdateOld1(ref baseKey);

            var rand = new MTRandom((uint)seed2);
            Misc.r_shufflebit(buffer, 0x40, rand);

            uint ciph = (uint)(BinaryPrimitives.ReadUInt32LittleEndian(buffer.Span[0x3C..]) ^ seed1);
            rand = new MTRandom(ogKey + ciph);

            for (int i = 0; i < 0x3C; i++)
                buffer.Span[i] ^= (byte)rand.getInt32();
        }
    }
}
