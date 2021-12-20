using Nestacular.NESCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NES2
{
    /// <summary>
    /// The NES Object represent the entire NES as a whole. 
    /// After each frame is drawn, the NES object will spit out a bitmap? that can be used to render the frame to the scrren
    /// 
    /// The NES contains a bus, each device that is part of the nes will recieve a reference to the bus. that is how the memory will be accessed
    /// as well as reads and writes to and from the bus will be performed.
    /// 
    /// The individual components of the NES do not know about each other, except maybe in certain scenarios
    /// instead they just know about the bus, and can read and write to and from the bus to communicate with the other components
    /// 
    /// </summary>
    internal class NES2
    {
        BUS _bus;
        public CPU2 _CPU;
        public CartLoader Loader { get; set; }
        public ulong masterClock = 0;
        public NES2()
        {
            _bus = new BUS();
            Loader = new CartLoader(_bus);
            _CPU = new CPU2(_bus);
            
        }

        //1.79 MHz the CPU executes 1,790,000 cyles per second.
        //Time between each cycle 0.000000558659217877095 seconds or
        //1s = 1000 ms            0.000001
        //.001s = 1ms
        //.000001 = 1 microsecond
        //.000000001 = 1 nanosecond
        //.000000000001 1 picosecond.

        //558 nanoseconds per cycle OR .558 microseconds.
        //a .NET tick is 100 nanoseconds, which can get us accuracy of within 50ish nanoseconds.
        //basically we need to execute a clock cycle every 5 ticks.
        public string MasterClockAdvance() 
        {   var sw = new Stopwatch();
            sw.Start();
            //sleep/block appropriate time for clock speed
            //this stopwatch should take into consideration the time it takes to process everything on host
            //so if the host takes 3 ticks to process, we should only NOP for 2.
            masterClock ++;
            var logString = _CPU.StepTo(masterClock);
            NOP(0.000000558659217877095, sw.ElapsedTicks);
            return logString;
        }

        private static void NOP(double durationSeconds, long alreadyElapsed)
        {
            var durationTicks = Math.Round(durationSeconds * Stopwatch.Frequency) - alreadyElapsed;;
            var sw = Stopwatch.StartNew();
            
            while (sw.ElapsedTicks < durationTicks)
            {

            }
        }
        
    }
}
