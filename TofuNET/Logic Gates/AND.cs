using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TofuNET.Logic_Gates
{
    internal class AND
    {
        public bool Input1 { private get; set; }
        public bool Input2 { private get; set; }
        public bool Output { get => Input1 & Input2; }
    }
}
