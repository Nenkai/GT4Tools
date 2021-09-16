using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

using PDISTD;
namespace GT4Tools
{
    public class GT4SaveDecryptor
    {
        public const float Mult = 0.13579f;
        public const float Mult2 = 0.65486f;

        public static void DecryptSave(string file, int offset = -1, int size = -1)
        {
            var savefile = File.ReadAllBytes(file);
            Span<byte> gamedata = offset != -1 ? savefile.AsSpan(0x1389D0, 0x3A0C0) : savefile;

            EncryptUnit_Decrypt(savefile, savefile.Length, 0, false, Mult, Mult2);
            File.WriteAllBytes("save.out", savefile);
        }

        private static bool EncryptUnit_Decrypt(Span<byte> buffer, int length, uint unk, bool useMt, float mult, float mult2)
        {
            GT4MC_swapPlace(buffer, length, buffer, 4, mult2);
            GT4MC_swapPlace(buffer.Slice(4), length - 4, buffer.Slice(4), 4, mult);

            if (useMt)
            {

            }
            else
            {

            }

            int firstVal = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            int secondVal = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(4));

            uint seed = (uint)(secondVal + firstVal);
            var rand = new MTRandom(seed);

            uint cipher = (uint)(secondVal ^ firstVal);
            Span<uint> dataInts = MemoryMarshal.Cast<byte, uint>(buffer.Slice(8));
            GT4MC_easyDecrypt(dataInts, length - 8, rand, ref cipher);

            Span<uint> uintBuf = MemoryMarshal.Cast<byte, uint>(buffer);
            uintBuf[1] ^= unk;

            if (uintBuf[1] == CRC32.crc32(buffer.Slice(8), length - 8))
            {
                Console.WriteLine($"Successfully decrypted!");
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void GT4MC_swapPlace(Span<byte> data, int size, Span<byte> data2, int count, float mult)
        {
            int index = (size - count);
            float calcOffset;
            if (count < 0)
                calcOffset = mult * ((index & 1 | (index >> 1)) + (index & 1 | (index >> 1)));
            else
                calcOffset = mult * index;

            GT4MC_swapPlace2(data, size, data2, count, (int)calcOffset);
        }

        private static void GT4MC_swapPlace2(Span<byte> data, int size, Span<byte> data2, int count, int calcOffset)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    byte swapA = data[calcOffset + i];
                    byte swapB = data2[i];

                    data[calcOffset + i] = swapB;
                    data2[i] = swapA;
                }
            }
        }

        private static void GT4MC_easyDecrypt(Span<uint> data, int len, MTRandom rand, ref uint seed)
        {
            Span<uint> remInts = data;
            int remBytesCnt = len;

            while (remBytesCnt >= 4)
            {
                int r = rand.getInt32();
                int res = Misc.RandomUpdateOld1(ref seed);
                remInts[0] = (uint)((remInts[0] + res) ^ r);

                remBytesCnt -= 4;
                remInts = remInts[1..];
            }

            var rem = MemoryMarshal.Cast<uint, byte>(remInts);
            while (remBytesCnt > 0)
            {
                int r = rand.getInt32();
                int res = Misc.RandomUpdateOld1(ref seed);
                rem[0] = (byte)((rem[0] + res) ^ r);

                remBytesCnt--;
                rem = rem[1..];
            }
        }
    }
}
