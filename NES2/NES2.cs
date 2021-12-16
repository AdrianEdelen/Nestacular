using Nestacular.NESCore;
using System;
using System.Collections.Generic;
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

        public NES2()
        {
            _bus = new BUS();
            _CPU = new CPU2(_bus);
        }


    }
}
