using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore.CPUCore
{
    public struct CPUStatus
    {
        internal CPUStatus(ushort pc, byte sp, byte a, byte x, byte y, byte p, bool c, bool z, bool i, bool d, bool b, bool v, bool n,
            bool isHalted, bool accumMode, byte fetchedByte, ushort fetchedAddress, UInt64 internalClock)
        {
            ProgramCounter = pc;
            StackPointer = sp;
            Accumulator = a;
            XRegister = x;
            YRegister = y;
            StatusByte = p;
            CarryFlag = c;
            ZeroFlag = z;
            InterruptDisableFlag = i;
            DecimalModeFlag = d;
            BreakCommandFlag = b;
            OverflowFlag = v;
            NegativeFlag = n;
            IsHalted = isHalted;
            AccumMode = accumMode;
            FetchedByte = fetchedByte;
            FetchedAddress = fetchedAddress;
            InternalClock = internalClock;
        }

        internal readonly ushort ProgramCounter;
        internal readonly byte StackPointer;
        internal readonly byte Accumulator;
        internal readonly byte XRegister;
        internal readonly byte YRegister;
        internal readonly byte StatusByte;
        internal readonly bool CarryFlag;
        internal readonly bool ZeroFlag;
        internal readonly bool InterruptDisableFlag;
        internal readonly bool DecimalModeFlag;
        internal readonly bool BreakCommandFlag;
        internal readonly bool OverflowFlag;
        internal readonly bool NegativeFlag;
        internal readonly bool IsHalted;
        internal readonly bool AccumMode;
        internal readonly byte FetchedByte;
        internal readonly ushort FetchedAddress;
        internal readonly UInt64 InternalClock;

        public override string ToString()
        {
            return @$"Program Counter:        {ProgramCounter:X4}
Stack Pointer:          {StackPointer:X2}
Accumulator:            {Accumulator:X2}
X Register:             {XRegister:X2}
Y Register:             {YRegister:X2}
Status Byte:            {Convert.ToString(StatusByte, 2).PadLeft(8, '0')}
Carry Flag:             {CarryFlag}
Zero Flag:              {ZeroFlag}
Interrupt Disable Flag: {InterruptDisableFlag}
Decimal Mode Flag:      {DecimalModeFlag}
Break Command Flag:     {BreakCommandFlag}
Overflow Flag:          {OverflowFlag}
Negative Flag:          {NegativeFlag}
Is Halted:              {IsHalted}
Accumulator Mode:       {AccumMode}
Current Fetched Byte:   {FetchedByte:X2}
Current Fetched Address:{FetchedAddress:X4}
Current Clock Count:    {InternalClock}";
        }
    }
}
