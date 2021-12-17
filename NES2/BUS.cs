using Nestacular.NESCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NES2
{
    public class BUS
    {
        byte[] RAM = new byte[64 * 1024];

        //TODO: consider putting a lock or something on these, so that they have to be accessed in order, not sure
        /// <summary>
        /// Write a byte of data to the memory on the bus
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        public void Write(ushort addr, byte data)
        {
            if (addr >= 0x000 && addr <= 0xFFFF)
                RAM[addr] = data;
        }

        /// <summary>
        /// Read a byte of data from the memory on the bus, this data can be markes as readonly 
        /// </summary>
        public byte Read(ushort addr, bool readOnly = false)
        {
            if (addr >= 0x0000 && addr <= 0xFFFF)
                return RAM[addr];
            else return 0x00;
        }

    }
}
