using System.Text;
using Nestacular.NESCore.BusCore;
using Nestacular.NESCore.CPUCore.Status;
namespace Nestacular.NESCore.CPUCore
{
    internal partial class CPU
    {
        BUS _bus;

        //Internal Flags and helper variables
        //These do not have real world equivalents.
        public bool _isHalted = false;
        private bool AccumMode = false;
        private byte _opCode;

        private byte fetchedByte = 0x00;
        private ushort fetchedAddress = 0x0000;
        public ulong Cycles { get; private set; }
        private static List<Instruction> _opCodes = new List<Instruction>();

        internal CPUStatus Status { get; private set; }
        internal InstructionStatus InstructionStatus { get; private set; }
        
        public void StepCPU()
        {
            Clock();
            UpdateStatus();
            updateInstructionStatus();

            AccumMode = false;
        }
        

        public void Clock()
        {
            _opCode = _bus.Read(PC); //get the byte of memory at the address of the PC
            Cycles += _opCodes[_opCode].Execute(); //Actually Execute the op

        }
        internal void UpdateStatus()
        {
            Status = new CPUStatus(PC, SP, A, X, Y, CreateStatusByte(), C, Z, I, D, B, V, N, _isHalted, AccumMode, fetchedByte, fetchedAddress, Cycles);
        }
        internal void updateInstructionStatus()
        {
            InstructionStatus = new InstructionStatus(PC, _opCodes[_opCode].ToString());
        }

        private void NMI() { /* nmi not implemented TODO */ }
        private void Startup() { throw new NotImplementedException(); }

        private void Shutdown() { throw new NotImplementedException(); }
        private void Reset() { throw new NotImplementedException(); }
        private byte CreateStatusByte()
        {
            var flags = new bool[8] { C, Z, I, D, false, true, V, N };

            byte range = 0;
            if (flags.Length < 8) range = 0;
            for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
            return range;
        }

        
    }
}