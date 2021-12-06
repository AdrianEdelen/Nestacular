using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    public class PPU
    {
        public byte[] VRAM = new byte[0x10000];
        public byte[] SPRRAM = new byte[0x256];
        byte StackPointer = 0xFD;
        public ushort PC = 0xC000; //skip the header for now
        

        public PPU()
        {

        }
    }

    public class NameTable
    {
        //The nametable is a 1024 byte area of memory used by the PPU to lay out backgrounds.
        //Each byte in a nametable controls one 8x8 pixel character cell
        //each name table has 30 rows of 32 tiles (960 bytes)
        //The rest is used by each tables attribute table
        //the attribute table has tiles of 8x8 pixels making a total of 256X240 pixels in one maps
        public byte[] Table = new byte[0x4000];
    }
}
