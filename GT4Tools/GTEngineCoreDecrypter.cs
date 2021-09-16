using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Linq;

using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;

using ICSharpCodeInflater = ICSharpCode.SharpZipLib.Zip.Compression.Inflater;

using PDISTD;
namespace GT4Tools
{
    class GTEngineCoreDecrypter
    {
        /* 0x00 - 8 bytes IV
         * 0x08 - Unk byte
         * 0x09 - Unk byte
         * 0x10 - Raw Size 
         */

        // Mainly intended/implemented for GT4 Online's CORE.GT4 as it has some extra encryption
        public static void Decrypt(byte[] file)
        {
            // Crc of the file at the end
            Console.WriteLine($"CRC: {CRC32_CoreGT4.crc32(file.AsSpan(0), file.Length - 4):X8}");

            SpanReader sr = new SpanReader(file);
            byte[] iv = sr.ReadBytes(8);

            byte[] key = GetKey();
            var s = new Salsa20(key, key.Length);
            s.SetIV(iv);
            s.Decrypt(file.AsSpan(8), file.Length - 12);

            // Decompress part - Entering compression header 
            sr.ReadByte();
            sr.ReadByte();
            int rawSize = sr.ReadInt32();

            int deflatedSize = file.Length - (8 + 12); // IV + Header + CRC at the bottom
            byte[] deflateData = sr.ReadBytes(deflatedSize);
            byte[] inflatedData = new byte[rawSize];

            ICSharpCodeInflater d = new ICSharpCodeInflater(true);
            d.SetInput(deflateData);
            d.Inflate(inflatedData);

            SpanReader sr2 = new SpanReader(inflatedData);
            short header1Size = sr2.ReadInt16();
            byte[] header1 = sr2.ReadBytes(header1Size);

            short header2Size = sr2.ReadInt16();
            byte[] header2 = sr2.ReadBytes(header2Size);

            int nSection = sr2.ReadInt32();
            int entrypoint = sr2.ReadInt32();

            var unk = new BufferReverser();
            unk.InitReverse(header1);

            byte[] elfData = inflatedData.Skip(0x104).ToArray();
            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(elfData);
            }
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
