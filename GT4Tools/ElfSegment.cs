using System;
using System.Collections.Generic;
using System.Text;

namespace GT4Tools
{
    public class ElfSegment
    {
        public int TargetOffset { get; set; }
        public int Size { get; set; }

        public byte[] Data { get; set; }
    }
}
