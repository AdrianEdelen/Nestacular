using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TofuNET.Gates;

namespace TofuNET.Latches
{
    public class SRLatch
    {
        
        
        public bool Q => _rGate.Output;
        public bool NotQ => _sGate.Output;


        public bool R
        {
            set
            {
                _rGate.Input1 = value;
                _rGate.Input2 = NotQ;
            }
        }
        public bool S
        {
            set
            {
                _sGate.Input1 = Q;
                _sGate.Input2 = value;
            }
        }

        private NOR _rGate = new NOR();
        private NOR _sGate = new NOR();



    }
}
