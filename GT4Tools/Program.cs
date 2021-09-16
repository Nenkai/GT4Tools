using System;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using PDISTD;

namespace GT4Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            GTEngineCoreDecrypter.Decrypt(File.ReadAllBytes(@"D:\Modding_Research\Gran_Turismo\Gran_Turismo_4_Online_Test_Version\CORE.GT4"));

        }

    }
}
