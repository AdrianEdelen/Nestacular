using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore
{
    public class NES
    {
        private byte[] memory;
        //public CPU cpu;
        public PPU ppu;
        private CU cu;
        public CartridgeLoader loader;
        public ulong MasterClock = 0;


        private int clockSpeed = 1; //the time in milliseconds to advance the master clock.
        public NES()
        {
            memory = new byte[0x10000];
            //cpu = new CPU(ref memory);
            ppu = new PPU(ref memory);
            cu = new CU();
            loader = new CartridgeLoader(ref memory);

        }

        public void Step()
        {
            //Thread.Sleep(clockSpeed);
            MasterClock++;
            //cpu.StepTo(MasterClock);
        }

    }
}