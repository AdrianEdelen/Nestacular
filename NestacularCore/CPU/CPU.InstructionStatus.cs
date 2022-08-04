using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore.CPUCore.Status
{
    public struct InstructionStatus
    {
        internal readonly ushort Address;
        internal readonly string Instruction;

        internal InstructionStatus(ushort address, string instruction)
        {
            Address = address;
            Instruction = instruction;
        }
        public override string ToString()
        {
            return $"{Address:X4} | {Instruction}";
        }
    }
}
