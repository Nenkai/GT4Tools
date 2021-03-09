using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Buffers.Binary;

using PDISTD;
namespace GT4Tools
{
    class GTEngineCoreDecrypter
    {
        // Mainly intended/implemented for GT4 Online's CORE.GT4 as it has some extra encryption
        public void Decrypt(byte[] file)
        {
            // Crc of the file at the end
            Console.WriteLine($"CRC: {CRC32_CoreGT4.crc32(file.AsSpan(0), file.Length - 4):X8}");

            byte[] key = GetKey();
            var s = new Salsa20(key, key.Length);
            s.SetIV(file.AsSpan(0, 8));
            s.Decrypt(file.AsSpan(8), file.Length - 8);

            int rawSize = BinaryPrimitives.ReadInt32LittleEndian(file.AsSpan(10, 4));

            /* Deflate starts right after
            int[] test = new int[0x20];
            var decryptedFile = File.ReadAllBytes(@"");
            var encryptedHeader = decryptedFile.AsSpan(0x84, 0x80);
            */

            /* Flip header stuff
            int storagePos = 0;
            for (int i = encryptedHeader.Length - 1; i >= 0; i--)
                test[storagePos >> 2] |= encryptedHeader[i] << (8 * (storagePos++ & 3));
            */

            // 0x80 blobs might be related to SHA512, seen code for it
        }

        public static readonly byte[] k = new byte[16]
        {
            // "PolyphonyDigital"
            0x05, 0x3A, 0x39, 0x2C, 0x25, 0x3D, 0x3A, 0x3B,
            0x2C, 0x11, 0x3C, 0x32, 0x3C, 0x21, 0x34, 0x39,
        };

        public static byte[] GetKey()
        {
            byte[] key = new byte[16];
            for (int i = 0; i < 16; i++)
                key[i] = (byte)(k[i] ^ 0x55);
            return key;
        }
    }
}
