using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    public class NES
    {
        public CPU CPU { get; set; }
        public NES()
        {
            CPU = new CPU();
        }
    }
}
