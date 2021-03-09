using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace PDISTD
{
    public struct Salsa20
    {
        static readonly byte[] c_sigma = Encoding.ASCII.GetBytes("expand 32-byte k");
        static readonly byte[] c_tau = Encoding.ASCII.GetBytes("expand 16-byte k");

        private uint[] m_state;

        public Salsa20(byte[] key, int keyLength)
        {
            if (keyLength > 32)
                keyLength = 32;

            m_state = new uint[0x10];

            // memcpy(vector, key, keyLength)
            var keyUints = MemoryMarshal.Cast<byte, uint>(key);
            keyUints.CopyTo(m_state.AsSpan(1, keyLength / 4));

            byte[] constants = key.Length == 32 ? c_sigma : c_tau;
            int keyIndex = key.Length - 16;

            m_state[11] = ToUInt32(key, keyIndex + 0);
            m_state[12] = ToUInt32(key, keyIndex + 4);
            m_state[13] = ToUInt32(key, keyIndex + 8);
            m_state[14] = ToUInt32(key, keyIndex + 12);
            m_state[0] = ToUInt32(constants, 0);
            m_state[5] = ToUInt32(constants, 4);
            m_state[10] = ToUInt32(constants, 8);
            m_state[15] = ToUInt32(constants, 12);

            m_state[6] = 0;
            m_state[7] = 0;
            m_state[8] = 0;
            m_state[9] = 0;
        }

        public void SetIV(Span<byte> iv)
        {
            var ivUints = MemoryMarshal.Cast<byte, uint>(iv);
            m_state[6] = ivUints[0];
            m_state[7] = ivUints[1];
            m_state[8] = 0;
            m_state[9] = 0;
        }

        public void Decrypt(Span<byte> bytes, int length)
        {
            byte[] o = new byte[64];

            int pos = 0;
            while (length > 0x40)
            {
                Hash(o);
                Increment();

                int blockSize = Math.Min(0x40, length);
                for (int i = 0; i < blockSize; i++)
                    bytes[pos + i] ^= o[i];

                pos += 0x40;
                length -= 0x40;
            }

        }

        private void Hash(byte[] output)
        {
            uint[] state = (uint[])m_state.Clone();

            for (int round = 20; round > 0; round -= 2)
            {
                state[4] ^= Rotate(Add(state[0], state[12]), 7);
                state[8] ^= Rotate(Add(state[4], state[0]), 9);
                state[12] ^= Rotate(Add(state[8], state[4]), 13);
                state[0] ^= Rotate(Add(state[12], state[8]), 18);
                state[9] ^= Rotate(Add(state[5], state[1]), 7);
                state[13] ^= Rotate(Add(state[9], state[5]), 9);
                state[1] ^= Rotate(Add(state[13], state[9]), 13);
                state[5] ^= Rotate(Add(state[1], state[13]), 18);
                state[14] ^= Rotate(Add(state[10], state[6]), 7);
                state[2] ^= Rotate(Add(state[14], state[10]), 9);
                state[6] ^= Rotate(Add(state[2], state[14]), 13);
                state[10] ^= Rotate(Add(state[6], state[2]), 18);
                state[3] ^= Rotate(Add(state[15], state[11]), 7);
                state[7] ^= Rotate(Add(state[3], state[15]), 9);
                state[11] ^= Rotate(Add(state[7], state[3]), 13);
                state[15] ^= Rotate(Add(state[11], state[7]), 18);
                state[1] ^= Rotate(Add(state[0], state[3]), 7);
                state[2] ^= Rotate(Add(state[1], state[0]), 9);
                state[3] ^= Rotate(Add(state[2], state[1]), 13);
                state[0] ^= Rotate(Add(state[3], state[2]), 18);
                state[6] ^= Rotate(Add(state[5], state[4]), 7);
                state[7] ^= Rotate(Add(state[6], state[5]), 9);
                state[4] ^= Rotate(Add(state[7], state[6]), 13);
                state[5] ^= Rotate(Add(state[4], state[7]), 18);
                state[11] ^= Rotate(Add(state[10], state[9]), 7);
                state[8] ^= Rotate(Add(state[11], state[10]), 9);
                state[9] ^= Rotate(Add(state[8], state[11]), 13);
                state[10] ^= Rotate(Add(state[9], state[8]), 18);
                state[12] ^= Rotate(Add(state[15], state[14]), 7);
                state[13] ^= Rotate(Add(state[12], state[15]), 9);
                state[14] ^= Rotate(Add(state[13], state[12]), 13);
                state[15] ^= Rotate(Add(state[14], state[13]), 18);
            }

            for (int index = 0; index < 16; index++)
                ToBytes(Add(state[index], m_state[index]), output, 4 * index);
        }

        private void Increment()
        {
            m_state[8]++;
            if (m_state[8] == 0)
                m_state[9]++;
        }

        private static uint Add(uint v, uint w)
        {
            return unchecked(v + w);
        }

        private static uint Rotate(uint v, int c)
            => (v << c) | (v >> (32 - c));

        private static void ToBytes(uint input, byte[] output, int outputOffset)
        {
            unchecked
            {
                output[outputOffset] = (byte)input;
                output[outputOffset + 1] = (byte)(input >> 8);
                output[outputOffset + 2] = (byte)(input >> 16);
                output[outputOffset + 3] = (byte)(input >> 24);
            }
        }

        private static uint ToUInt32(byte[] input, int inputOffset)
            => unchecked((uint)(((input[inputOffset] | (input[inputOffset + 1] << 8)) | (input[inputOffset + 2] << 16)) | (input[inputOffset + 3] << 24)));


    }
}
