using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    public class CPU
    {
        public byte[] Memory; //TODO: change this to private after testing, its only public to access it in main
        #region general Notes On the CPU
        //Memory Map 
        /* 0x0000 - 0x00FF
         * 0x0100 - 0x01FF STACK
         * 0x0200 - 0x07FF
         * 0x0800 - 0x1FFF
         * 0x2000 - 0x2007 IO Registers
         * 0x2008 - 0x3FFF
         * 0x4000 - 0x401F
         * 0x4020 - 0x5FFF
         * 0x6000 - 0x7FFF
         * 0x8000 - 0xBFFF
         * 0xC000 - 0x10000
         * */

        //TODO: CPU cycle count is off somehow. I think its a before and after type thing
        //TODO: REplace this stringbuilder mess with a ToString override of the state of the CPU, we can output that after each cycle.
        //TODO: Replace search for opcode with a general CYCLE CPU Method.
        //TODO: Within the cycle cpu, each op or cycle consuming thing, will report its cycles, then we will halt for n time according to the appropriate cycle time
        //TODO: abstract out some of these arbitrary bitshifts and movements, stuff like PC +=2 should be more like FetchNextInstruction();
        //TODO: write an IsBitSet method that just lets you select the index and returns a bool
        //TODO: refactor the Processor status, have a byte proper that has a getter of the bit values for all of the flags in the right position
        //TODO: Create a tostring override, for the CPU status that can be used to write and compare logs.
        //TODO: maybe make a json string also for UI 
        //TODO: Determine BIOS/FIRMWARE/startup routine.
        //TODO: I may have fucked up this whol way of doing things, see next note
        /*
         * So the CPU clock cycle timing (how much time it takes to do each CPU step) is unique for each address mode AND instruction
         * combo. this creates the problem of the way I generalized this.
         * so it may be that each instruction + address mode combo needs its own method that contains the cycle count
         * ideally, we will emulate everything on a per cycle basis (so the fetch will inc the counter, then the actual operations will inc the counter
         * 
         * need to read more about how the cycle actually work.
         */
        #endregion
        #region Registers. Independent of RAM
        public byte StackPointer = 0xFD;
        public ushort PC = 0xC000; //skip the header for now
        public byte Accumulator = 0x00;
        public byte RegisterY = 0x00;
        public byte RegisterX = 0x00;
        #endregion
        public byte Status
        {
            get
            {
                var flags = new bool[8]
                {
                    Flags.CarryFlag,
                    Flags.ZeroFlag,
                    Flags.InterruptDisableFlag,
                    Flags.DecimalModeFlag,
                    false,
                    Flags.nullFlag,
                    Flags.OverflowFlag,
                    Flags.NegativeFlag
                    };
                byte range = 0;
                if (flags == null || flags.Length < 8) range = 0;
                for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
                return range;
            }
        }
        public bool IsHalted = false;
        public ProcessorStatus Flags = new ProcessorStatus();

        public bool IsNewOP = false;//testing 
        public ulong CPUCycle = 0;
        private AddressModes _currentMode; //This is a workaround until I figure out a better way this will just be changed to an int
        private ushort curMemLocation;
        private long internalClock;
        public CPU(ref Byte[] memory)
        {
            //Startup routine
            //can probably make this more accurate
            Memory = memory;
            CPUCycle = 4; //Why is this?
            Flags.InterruptDisableFlag = true; //I think this starts on true
            Flags.NegativeFlag = false; //test
            Flags.nullFlag = true;

        }


        #region Various Functions
        void Clock() { }
        void Reset() { }
        void IRQ() { }
        void NMI() { }
        public void StepTo(long masterClock)
        {
            //NOTE: this is NOT one cycle thre are a variable amount of cycles that would happen here
            //for now I think we will advance the master clock based on the clock speed.
            if (!IsHalted && internalClock < masterClock)
                SearchForOpcode();
        }

        #endregion
        #region OPcode Calculations
        enum AddressModes
        {
            Increase2, Increase3, Implied, Absolute, AbsoluteY, AbsoluteX, Immediate, Indirect, XIndexedIndirect,
            YindexedIndirect, Relative, ZeroPage, ZeroPageX, ZeroPageY, NO_PC_CHANGE
        }
        private void SearchForOpcode()
        {
            #region Some notes on OPCODES
            /*
            There are three primary things to determine when fetching an opcode
            1. The instruction (e.g. STA)
            2. The addressing mode (e.g. Indirect)
            3. The operands

            The different instructions access their operands differently based on the addressing mode.
            This results in effectively having multiple versions of the same instruction.

            The different addressing modes for each instruction are determined by the byte for the instruction.

            for example:
            $69 is the opcode for ADC Immediate
            $65 is the opcode for ADC Zero Page

            The key is that the opcode is still performing the same operation, just the arguments are different.
            Effectively you can look at it like overloads to a method

            The good thing about this is we can drastically reduce our LOC by creating proper methods for each instruction.

            I know some of the opcode methods may be a little redundant, but I feel it is important to have a very consistent style
            for the large switch statement. This makes debugging easier, as well as seeing which byte belongs to which OP
            It also makes it easier to look at whats happening at a higher level since we are always able to refer to ops by their ASM names

            */
            #endregion

            StringBuilder sb = new StringBuilder(); //Test
            sb.Append($"{PC.ToString("X4")}  {Memory[PC].ToString("X2")} "); //Test
            IsNewOP = false; //Test

            _currentMode = AddressModes.Implied; //assume implied until is is changed
            var prevAccum = Accumulator;
            switch (Memory[PC])
            {
                //The switch here exists only to find the correct opcode.
                //It calls the appropriate opcode with the correct Addressing mode
                #region 0x0
                case 0x00: BRK(); break;
                case 0x01: ORA(XIndexedIndirectMode()); break;
                case 0x02: JamCPU(); break;
                case 0x03: SLO(XIndexedIndirectMode()); break;
                case 0x04: NOP(ZeroPageMode()); break;
                case 0x05: ORA(ZeroPageMode()); break;
                case 0x06: ASL(ZeroPageMode()); break;
                case 0x07: SLO(ZeroPageMode()); break;
                case 0x08: PHP(); break;
                case 0x09: ORA(ImmediateMode()); break;
                case 0x0A: ASL(); break;
                case 0x0B: ANC(); break;
                case 0x0C: NOP(AbsoluteMode()); break;
                case 0x0D: ORA(AbsoluteMode()); break;
                case 0x0E: ASL(AbsoluteMode()); break;
                case 0x0F: SLO(AbsoluteMode()); break;
                #endregion 
                #region 0x1

                case 0x10: BPL(); break;
                case 0x11: ORA(YIndexIndirectMode()); break;
                case 0x12: JamCPU(); break;
                case 0x13: SLO(YIndexIndirectMode()); break;
                case 0x14: NOP(ZeroPageXIndexedMode()); break;
                case 0x15: ORA(ZeroPageXIndexedMode()); break;
                case 0x16: ASL(ZeroPageXIndexedMode()); break;
                case 0x17: SLO(ZeroPageXIndexedMode()); break;
                case 0x18: CLC(); break;
                case 0x19: ORA(AbsoluteYMode()); break;
                case 0x1A: NOP(0x00); ; break;
                case 0x1B: SLO(AbsoluteYMode()); break;
                case 0x1C: NOP(AbsoluteXMode()); break;
                case 0x1D: ORA(AbsoluteXMode()); break;
                case 0x1E: ASL(AbsoluteXMode()); break;
                case 0x1F: SLO(AbsoluteXMode()); break;
                #endregion
                #region 0x2
                case 0x20: JSR(); break;
                case 0x21: AND(XIndexedIndirectMode()); break;
                case 0x22: JamCPU(); break;
                case 0x23: RLA(XIndexedIndirectMode()); break;
                case 0x24: BIT(ZeroPageMode()); break;
                case 0x25: AND(ZeroPageMode()); break;
                case 0x26: ROL(ZeroPageMode()); break;
                case 0x27: RLA(ZeroPageMode()); break;
                case 0x28: PLP(); break;
                case 0x29: AND(ImmediateMode()); break;
                case 0x2A: ROL(); break;
                case 0x2B: ANC(ImmediateMode()); break;
                case 0x2C: BIT(AbsoluteMode()); break;
                case 0x2D: AND(AbsoluteMode()); break;
                case 0x2E: ROL(AbsoluteMode()); break;
                case 0x2F: RLA(AbsoluteMode()); break;
                #endregion
                #region 0x3
                case 0x30: BMI(); break;
                case 0x31: AND(YIndexIndirectMode()); break;
                case 0x32: JamCPU(); break;
                case 0x33: RLA(YIndexIndirectMode()); break;
                case 0x34: NOP(ZeroPageXIndexedMode()); break;
                case 0x35: AND(ZeroPageXIndexedMode()); break;
                case 0x36: ROL(ZeroPageXIndexedMode()); break;
                case 0x37: RLA(ZeroPageXIndexedMode()); break;
                case 0x38: SEC(); break;
                case 0x39: AND(AbsoluteYMode()); break;
                case 0x3A: NOP(0x00); break;
                case 0x3B: RLA(AbsoluteYMode()); break;
                case 0x3C: NOP(AbsoluteXMode()); break;
                case 0x3D: AND(AbsoluteXMode()); break;
                case 0x3E: ROL(AbsoluteXMode()); break;
                case 0x3F: RLA(AbsoluteXMode()); break;
                #endregion
                #region 0x4
                case 0x40: RTI(); break;
                case 0x41: EOR(XIndexedIndirectMode()); break;
                case 0x42: JamCPU(); break;
                case 0x43: SRE(XIndexedIndirectMode()); break;
                case 0x44: NOP(ZeroPageMode()); break;
                case 0x45: EOR(ZeroPageMode()); break;
                case 0x46: LSR(ZeroPageMode()); break;
                case 0x47: SRE(ZeroPageMode()); break;
                case 0x48: PHA(); break;
                case 0x49: EOR(ImmediateMode()); break;
                case 0x4A: LSR(); break;
                case 0x4B: ALR(ImmediateMode()); break;
                case 0x4C: JMP(); break;
                case 0x4D: EOR(AbsoluteMode()); break;
                case 0x4E: LSR(AbsoluteMode()); break;
                case 0x4F: SRE(AbsoluteMode()); break; ;
                #endregion
                #region 0x5
                case 0x50: BVC(); break;
                case 0x51: EOR(YIndexIndirectMode()); break;
                case 0x52: JamCPU(); break;
                case 0x53: SRE(YIndexIndirectMode()); break;
                case 0x54: NOP(ZeroPageXIndexedMode()); break;
                case 0x55: EOR(ZeroPageXIndexedMode()); break;
                case 0x56: LSR(ZeroPageXIndexedMode()); break;
                case 0x57: SRE(ZeroPageXIndexedMode()); break;
                case 0x58: CLI(); break;
                case 0x59: EOR(AbsoluteYMode()); break;
                case 0x5A: NOP(0x00); break;
                case 0x5B: SRE(AbsoluteYMode()); break;
                case 0x5C: NOP(AbsoluteXMode()); break;
                case 0x5D: EOR(AbsoluteXMode()); break;
                case 0x5E: LSR(AbsoluteXMode()); break;
                case 0x5F: SRE(AbsoluteXMode()); break;
                #endregion
                #region 0x6
                case 0x60: RTS(); break;
                case 0x61: ADC(XIndexedIndirectMode()); break;
                case 0x62: JamCPU(); break;
                case 0x63: RRA(XIndexedIndirectMode()); break;
                case 0x64: NOP(ZeroPageMode()); break;
                case 0x65: ADC(ZeroPageMode()); break;
                case 0x66: ROR(ZeroPageMode()); break;
                case 0x67: RRA(ZeroPageMode()); break;
                case 0x68: PLA(); break;
                case 0x69: ADC(ImmediateMode()); break;
                case 0x6A: ROR(); break;
                case 0x6B: ARR(ImmediateMode()); break;
                case 0x6C: JMP(IndirectMode()); break;
                case 0x6D: ADC(AbsoluteMode()); break;
                case 0x6E: ROR(AbsoluteMode()); break;
                case 0x6F: RRA(AbsoluteMode()); break;
                #endregion
                #region 0x7
                case 0x70: BVS(); break;
                case 0x71: ADC(YIndexIndirectMode()); break;
                case 0x72: JamCPU(); break;
                case 0x73: RRA(YIndexIndirectMode()); break;
                case 0x74: NOP(ZeroPageXIndexedMode()); break;
                case 0x75: ADC(ZeroPageXIndexedMode()); break;
                case 0x76: ROR(ZeroPageXIndexedMode()); break;
                case 0x77: RRA(ZeroPageXIndexedMode()); break;
                case 0x78: SEI(); break;
                case 0x79: ADC(AbsoluteYMode()); break;
                case 0x7A: NOP(); break;
                case 0x7B: RRA(AbsoluteYMode()); break;
                case 0x7C: NOP(AbsoluteXMode()); break;
                case 0x7D: ADC(AbsoluteXMode()); break;
                case 0x7E: ROR(AbsoluteXMode()); break;
                case 0x7F: RRA(AbsoluteXMode()); break;
                #endregion
                #region 0x8
                case 0x80: NOP(ImmediateMode()); break;
                case 0x81: STA(XIndexedIndirectMode()); break;
                case 0x82: NOP(ImmediateMode()); break;
                case 0x83: SAX(XIndexedIndirectMode()); break;
                case 0x84: STY(ZeroPageMode()); break;
                case 0x85: STA(ZeroPageMode()); break;
                case 0x86: STX(ZeroPageMode()); break;
                case 0x87: SAX(ZeroPageMode()); break;
                case 0x88: DEY(); break;
                case 0x89: NOP(ImmediateMode()); break;
                case 0x8A: TXA(); break;
                case 0x8B: ANE(ImmediateMode()); break;
                case 0x8C: STY(AbsoluteMode()); break;
                case 0x8D: STA(AbsoluteMode()); break;
                case 0x8E: STX(AbsoluteMode()); break;
                case 0x8F: SAX(AbsoluteMode()); break;
                #endregion
                #region 0x9
                case 0x90: BCC(); break;
                case 0x91: STA(YIndexIndirectMode()); break;
                case 0x92: JamCPU(); break;
                case 0x93: SHA(YIndexIndirectMode()); break;
                case 0x94: STY(ZeroPageXIndexedMode()); break;
                case 0x95: STA(ZeroPageXIndexedMode()); break;
                case 0x96: STX(ZeroPageYIndexed()); break;
                case 0x97: SAX(ZeroPageYIndexed()); break;
                case 0x98: TYA(); break;
                case 0x99: STA(AbsoluteYMode()); break;
                case 0x9A: TXS(); break;
                case 0x9B: TAS(AbsoluteYMode()); break;
                case 0x9C: SHY(AbsoluteXMode()); break;
                case 0x9D: STA(AbsoluteXMode()); break;
                case 0x9E: SHX(AbsoluteYMode()); break;
                case 0x9F: SHY(AbsoluteYMode()); break;
                #endregion
                #region 0xA
                case 0xA0: LDY(ImmediateMode()); break;
                case 0xA1: LDA(XIndexedIndirectMode()); break;
                case 0xA2: LDX(ImmediateMode()); break;
                case 0xA3: LAX(XIndexedIndirectMode()); break;
                case 0xA4: LDY(ZeroPageMode()); break; ;
                case 0xA5: LDA(ZeroPageMode()); break;
                case 0xA6: LDX(ZeroPageMode()); break;
                case 0xA7: LAX(ZeroPageMode()); break;
                case 0xA8: TAY(); break;
                case 0xA9: LDA(ImmediateMode()); break;
                case 0xAA: TAX(); break;
                case 0xAB: LXA(ImmediateMode()); break;
                case 0xAC: LDY(AbsoluteMode()); break;
                case 0xAD: LDA(AbsoluteMode()); break;
                case 0xAE: LDX(AbsoluteMode()); break;
                case 0xAF: LAX(AbsoluteMode()); break;
                #endregion
                #region 0xB
                case 0xB0: BCS(); break;
                case 0xB1: LDA(YIndexIndirectMode()); break;
                case 0xB2: JamCPU(); break;
                case 0xB3: LAX(YIndexIndirectMode()); break;
                case 0xB4: LDY(ZeroPageXIndexedMode()); break;
                case 0xB5: LDA(ZeroPageXIndexedMode()); break;
                case 0xB6: LDX(ZeroPageYIndexed()); break;
                case 0xB7: LAX(ZeroPageYIndexed()); break;
                case 0xB8: CLV(); break;
                case 0xB9: LDA(AbsoluteYMode()); break;
                case 0xBA: TSX(); break;
                case 0xBB: LAS(AbsoluteYMode()); break;
                case 0xBC: LDY(AbsoluteXMode()); break;
                case 0xBD: LDA(AbsoluteXMode()); break;
                case 0xBE: LDX(AbsoluteYMode()); break;
                case 0xBF: LAX(AbsoluteYMode()); break;
                #endregion
                #region 0xC
                case 0xC0: CMP(RegisterY, ImmediateMode()); break;
                case 0xC1: CMP(Accumulator, XIndexedIndirectMode()); break;
                case 0xC2: NOP(ImmediateMode()); break;
                case 0xC3: DCP(XIndexedIndirectMode()); break;
                case 0xC4: CMP(RegisterY, ZeroPageMode()); break;
                case 0xC5: CMP(Accumulator, ZeroPageMode()); break;
                case 0xC6: DEC(ZeroPageMode()); break;
                case 0xC7: DCP(ZeroPageMode()); break;
                case 0xC8: INY(); break;
                case 0xC9: CMP(Accumulator, ImmediateMode()); break;
                case 0xCA: DEX(); break;
                case 0xCB: SBX(ImmediateMode()); break;
                case 0xCC: CMP(RegisterY, AbsoluteMode()); break;
                case 0xCD: CMP(Accumulator, AbsoluteMode()); break;
                case 0xCE: DEC(AbsoluteMode()); break;
                case 0xCF: DCP(AbsoluteMode()); break;
                #endregion
                #region 0xD
                case 0xD0: BNE(); break;
                case 0xD1: CMP(Accumulator, YIndexIndirectMode()); break;
                case 0xD2: JamCPU(); break;
                case 0xD3: DCP(YIndexIndirectMode()); break;
                case 0xD4: NOP(ZeroPageXIndexedMode()); break;
                case 0xD5: CMP(Accumulator, ZeroPageXIndexedMode()); break;
                case 0xD6: DEC(ZeroPageXIndexedMode()); break;
                case 0xD7: DCP(ZeroPageXIndexedMode()); break;
                case 0xD8: CLD(); break;
                case 0xD9: CMP(Accumulator, AbsoluteYMode()); break;
                case 0xDA: NOP(); break;
                case 0xDB: DCP(AbsoluteYMode()); break;
                case 0xDC: NOP(AbsoluteXMode()); break;
                case 0xDD: CMP(Accumulator, AbsoluteXMode()); break;
                case 0xDE: DEC(AbsoluteXMode()); break;
                case 0xDF: DCP(AbsoluteXMode()); break;
                #endregion
                #region 0xE
                case 0xE0: CMP(RegisterX, ImmediateMode()); break;
                case 0xE1: SBC(XIndexedIndirectMode()); break;
                case 0xE2: NOP(ImmediateMode()); break;
                case 0xE3: ISC(XIndexedIndirectMode()); break;
                case 0xE4: CMP(RegisterX, ZeroPageMode()); break;
                case 0xE5: SBC(ZeroPageMode()); break;
                case 0xE6: INC(ZeroPageMode()); break;
                case 0xE7: ISC(ZeroPageMode()); break;
                case 0xE8: INX(); break;
                case 0xE9: SBC(ImmediateMode()); break;
                case 0xEA: NOP(0x00); break;
                case 0xEB: USBC(ImmediateMode()); break;
                case 0xEC: CMP(RegisterX, AbsoluteMode()); break;
                case 0xED: SBC(AbsoluteMode()); break;
                case 0xEE: INC(AbsoluteMode()); break;
                case 0xEF: ISC(AbsoluteMode()); break;
                #endregion
                #region 0xF
                case 0xF0: BEQ(); break;
                case 0xF1: SBC(YIndexIndirectMode()); break;
                case 0xF2: JamCPU(); break;
                case 0xF3: ISC(YIndexIndirectMode()); break;
                case 0xF4: NOP(ZeroPageXIndexedMode()); break;
                case 0xF5: SBC(ZeroPageXIndexedMode()); break;
                case 0xF6: INC(ZeroPageXIndexedMode()); break;
                case 0xF7: ISC(ZeroPageXIndexedMode()); break;
                case 0xF8: SED(); break;
                case 0xF9: SBC(AbsoluteYMode()); break;
                case 0xFA: NOP(); break;
                case 0xFB: ISC(AbsoluteYMode()); break;
                case 0xFC: NOP(AbsoluteXMode()); break;
                case 0xFD: SBC(AbsoluteXMode()); break;
                case 0xFE: INC(AbsoluteXMode()); break;
                case 0xFF: ISC(AbsoluteXMode()); break;
                    #endregion
            }
            switch (_currentMode) //Set the Program Counter to the appropriate next position
            {
                //TODO: Change this from modes, to PC increments instead, more general. and can have some unrelated to a mode
                //e.g. 0,1,2,3,4,5,6,7
                //infact, maybe it just needs to be an int thats probably better.
                case AddressModes.Absolute: PC += 3; break;
                case AddressModes.AbsoluteX: PC += 3; break;
                case AddressModes.AbsoluteY: PC += 3; break;
                case AddressModes.Immediate: PC += 2; break;
                case AddressModes.Implied: PC++; break;
                case AddressModes.Indirect: PC += 3; break;
                case AddressModes.Relative: PC += 2; break;
                case AddressModes.XIndexedIndirect: PC += 2; break;
                case AddressModes.YindexedIndirect: PC += 2; break;
                case AddressModes.ZeroPage: PC += 2; break;
                case AddressModes.ZeroPageX: PC += 2; break;
                case AddressModes.ZeroPageY: PC += 2; break;
                case AddressModes.NO_PC_CHANGE: break;
                case AddressModes.Increase2: PC += 2; break;
                case AddressModes.Increase3: PC += 3; break;
            }
            if (prevAccum != Accumulator) AccumChanged();
            internalClock += 1;
            Trace.WriteLine(sb);
        }
        #endregion
        #region OPcode helpers 
        #region Notes On OPcode helpers
        /* These methods are for the most part pretty simple one liners to help cut down on typos
        While it is probably more efficient to just do some of these manually, they have proven to be very helpful during
        the design phase.*/
        #endregion
        private void JamCPU()
        {
            //TODO: set data bus to $FF
            IsHalted = true;
        }
        private byte PeekNextByte() { return Memory[PC + 1]; }
        private byte PeekByteAfterNext() { return Memory[PC + 2]; }
        private void SetZeroAndNegFlag(byte value)
        {
            if ((value & 128) != 0) Flags.NegativeFlag = true;
            else Flags.NegativeFlag = false;
            if (value != 0x00) Flags.ZeroFlag = false;
            else Flags.ZeroFlag = true;
        }
        private void Branch(bool DoBranch)
        {
            _currentMode = AddressModes.NO_PC_CHANGE;
            var jumpDistance = PeekNextByte() + 2;

            if (DoBranch && jumpDistance >= 0x80) PC -= (byte)(0xFF - jumpDistance + 1);
            else if (DoBranch) PC += ((byte)jumpDistance);
            else PC += 2;
        }
        private void AccumChanged()
        {
            if (Accumulator != 0x00) Flags.ZeroFlag = false; else Flags.ZeroFlag = true;
            if ((Accumulator & 128) != 0) Flags.NegativeFlag = true; else Flags.NegativeFlag = false;
        }
        private ushort SwapNextTwoBytes() { return (ushort)(PeekByteAfterNext() << 8 | PeekNextByte()); }
        private void PushToStack(byte value)
        {
            var currentStackPosition = (ushort)0x01 << 8 | StackPointer;
            Memory[currentStackPosition] = value;
            DecrementStackPointer();
        }
        private byte PopFromStack()
        {
            IncrementStackPointer();
            var currentStackPosition = (ushort)(0x01 << 8 | StackPointer);
            var retVal = Memory[currentStackPosition];

            return retVal;
        }
        private void DecrementStackPointer()
        {
            StackPointer -= 0x01;
        }
        private void IncrementStackPointer()
        {
            StackPointer += 0x01;
        }
        #endregion
        #region Addressing Modes
        #region Notes On Addressing Modes
        //NOTE: The PC needs to be modified AFTER The value has been grabbed,
        //NOTE TO ME: make sure that NONE of the Opcode methods utilize the opcode, if they do, they need to get it from the operand of the 
        //instruction and NOT from the PC.
        #endregion
        private byte AbsoluteMode()
        {
            _currentMode = AddressModes.Absolute;
            curMemLocation = SwapNextTwoBytes();
            return Memory[curMemLocation];
        }
        private byte AbsoluteXMode()
        {
            _currentMode = AddressModes.AbsoluteX;
            var tmp = SwapNextTwoBytes();
            var tmp1 = RegisterX;
            var tmp2 = (ushort)(tmp + tmp1);
            curMemLocation = tmp2;
            return Memory[tmp2];
        }
        private byte AbsoluteYMode()
        {
            _currentMode = AddressModes.AbsoluteY;
            var tmp = SwapNextTwoBytes();
            var tmp1 = RegisterY;
            var tmp2 = (ushort)(tmp + tmp1);
            curMemLocation = tmp2;
            return Memory[tmp2];
        }
        private byte ImmediateMode()
        {
            _currentMode = AddressModes.Immediate;
            return PeekNextByte();
        }
        private void ImpliedMode()
        {
            _currentMode = AddressModes.Implied;
        }
        private ushort IndirectMode()
        {
            _currentMode = AddressModes.Indirect;
            curMemLocation = SwapNextTwoBytes();
            if ((curMemLocation & 0x00FF) == 0xFF)
            {
                return curMemLocation += 1;
            }
            var b1 = Memory[curMemLocation];
            var b2 = Memory[curMemLocation + 1];
            var calcedLocation = (ushort)(b2 << 8 | b1);

            return calcedLocation;
        }
        private byte XIndexedIndirectMode()
        {
            _currentMode = AddressModes.XIndexedIndirect;
            var indexByte = PeekNextByte();
            var newPos = (byte)(indexByte + RegisterX);
            var calcedPos = Memory[newPos];
            var calcedPos2 = Memory[(byte)(newPos + 1)];
            ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
            curMemLocation = addr;
            return Memory[curMemLocation];
        }
        private byte YIndexIndirectMode()
        {
            _currentMode = AddressModes.YindexedIndirect;
            byte indexByte = PeekNextByte();
            byte b1 = Memory[indexByte];
            byte b2 = Memory[indexByte + 1];
            if (indexByte == 0xFF) b2++;
            ushort tempShort = 0x00;
            tempShort = (ushort)(b2 << 8 | b1);
            tempShort += RegisterY;
            curMemLocation = tempShort;
            return Memory[curMemLocation];
        }
        private void RelativeMode()
        {
            _currentMode = AddressModes.Relative;
        }
        private byte ZeroPageMode()
        {
            _currentMode = AddressModes.ZeroPage;
            curMemLocation = PeekNextByte();
            return Memory[curMemLocation];
        }
        private byte ZeroPageXIndexedMode()
        {
            _currentMode = AddressModes.ZeroPageX;
            byte tmp = PeekNextByte();
            tmp += RegisterX;
            curMemLocation = tmp;
            return Memory[tmp];
        }
        private byte ZeroPageYIndexed()
        {
            _currentMode = AddressModes.ZeroPageY;
            byte tmp = PeekNextByte();
            tmp += RegisterY;
            curMemLocation = tmp;
            return Memory[tmp];
        }
        #endregion
        #region Official Instructions
        #region Notes On CPU Instructions
        /* The following region contains all of the official opcodes for the 6502
        The Next Section will contain the 'Illegal' opcodes
        The next question becomes, How do WE want to determine the Address Mode
        We can make the methods more generic with parameters and pass in the correct operands for the addressing mode
        OR we can pass the addressing mode as an argument and let the method do the sorting out of where to get the operand
        I like option 1

        Once complete, each instruction will have a thorough explanation of what the instruction does and why it does.
        Part of the challenge with this is that since this is old school tech, some of the way it works is obtuse.
        In order to accurately emulate it we often have to be obtuse as well, doing bitwise stuff etc.

        So to combat later confusion, I may write painstakingly detailed play by play comments on some stuff. 

         */
        #endregion
        private void ADC(byte op)
        {
            // As The Prodigy once said: This is dangerous.
            //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
            var carry = Flags.CarryFlag ? 1 : 0; //is the carry flag set
            var sum = Accumulator + op + carry; //sum the Accum+operand+carry(if set)
            Flags.CarryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
            Flags.OverflowFlag = (~(Accumulator ^ op) & (Accumulator ^ sum) & 0x80) != 0 ? true : false;
            Accumulator = (byte)sum;

            
            

        }
        private void AND(byte op)
        {
            Accumulator = (byte)(Accumulator & op);
            SetZeroAndNegFlag(Accumulator);

            
            
        }
        private void ASL(byte op)
        {
            //the byte that is passed is the calculated value from the position of the next byte/s in memory 
            //e.g. PeekNextByte(PC).. The problem is we have moved the PC already in the address mode method, so now we cannot
            //be sure where the actual POSITION that this value needs to be stored in.
            //we are calculating the correct value, we just lost where it belongs.

            //I think the only solution is for ASL to have a second argument.
            //I hate this since it breaks the pattern.

            var temp = op;
            if ((temp & 128) != 0) Flags.CarryFlag = true;
            else Flags.CarryFlag = false;
            temp = (byte)(temp << 1);
            Memory[curMemLocation] = temp;
            SetZeroAndNegFlag(Memory[curMemLocation]);

            
            
        }
        private void ASL() //Implied Overload
        {
            if ((Accumulator & 128) != 0) Flags.CarryFlag = true;
            else Flags.CarryFlag = false;
            Accumulator = (byte)(Accumulator << 1);

            
            
        }
        private void BCC()
        {
            //jump as many addresses as the next address says plus one i think
            Branch(!Flags.CarryFlag);

            
            
        }
        private void BCS()
        {
            //jump as many addresses as the next address says plus one i think
            Branch(Flags.CarryFlag);

            
            
        }
        private void BEQ()
        {
            Branch(Flags.ZeroFlag);

            
            
        }
        private void BIT(byte op)
        {
            //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
            //value store at the tested address
            var pos = op;
            if ((Accumulator & pos) == 0x00) Flags.ZeroFlag = true;
            else Flags.ZeroFlag = false;
            if ((pos & 128) != 0) Flags.NegativeFlag = true;
            else Flags.NegativeFlag = false;
            if ((pos & 64) != 0) Flags.OverflowFlag = true;
            else Flags.OverflowFlag = false;

            
            
        }
        private void BMI()
        {
            Branch(Flags.NegativeFlag);

            
            
        }
        private void BNE()
        {
            Branch(!Flags.ZeroFlag);

            
            
        }
        private void BPL()
        {
            Branch(!Flags.NegativeFlag);

            
            
        }
        private void BRK()
        {
            PC++;
            NMI();

            
            
        }
        private void BVC()
        {
            Branch(!Flags.OverflowFlag);

            
            
        }
        private void BVS()
        {
            Branch(Flags.OverflowFlag);

            
            
        }
        private void CLC()
        {
            Flags.CarryFlag = false;

            
            
        }
        private void CLD()
        {
            Flags.DecimalModeFlag = false;

            
            
        }
        private void CLI()
        {
            Flags.InterruptDisableFlag = false;

            
            
        }
        private void CLV()
        {
            Flags.OverflowFlag = false;

            
            
        }
        private void CMP(byte register, byte operand)
        {
            var aa = operand;
            var bb = register;
            if (bb > aa)
            {
                byte cc = (byte)(bb - aa);
                Flags.CarryFlag = true;
                Flags.ZeroFlag = false;
                Flags.NegativeFlag = (cc & 128) != 0;
            }
            else if (bb < aa)
            {
                byte cc = (byte)(bb - aa);
                Flags.CarryFlag = false;
                Flags.ZeroFlag = false;
                Flags.NegativeFlag = (cc & 128) != 0;
            }
            else
            {
                Flags.NegativeFlag = false;
                Flags.ZeroFlag = true;
                Flags.CarryFlag = true;
            }

            
            

        }
        private void CPX() { throw new NotImplementedException(); }
        private void CPY() { throw new NotImplementedException(); }
        private void DEC(byte op)
        {
            Memory[curMemLocation]--;
            SetZeroAndNegFlag(Memory[curMemLocation]);

            
            
        }
        private void DEX()
        {
            RegisterX--;
            SetZeroAndNegFlag(RegisterX);

            
            
        }
        private void DEY()
        {
            RegisterY--;
            SetZeroAndNegFlag(RegisterY);

            
            
        }
        private void EOR(byte op)
        {
            Accumulator ^= op;

            
            
        }
        private void INC(byte op)
        {
            Memory[curMemLocation]++;
            SetZeroAndNegFlag(Memory[curMemLocation]);

            
            
        }
        private void INX()
        {
            RegisterX++;
            SetZeroAndNegFlag(RegisterX);

            
            
        }
        private void INY()
        {
            RegisterY++;
            SetZeroAndNegFlag(RegisterY);

            
            
        }
        private void JMP(ushort jmpLocation) //Indirect
        {
            PC = jmpLocation;
            _currentMode = AddressModes.NO_PC_CHANGE;

            
            
        }
        private void JMP()
        {
            //Move the PC to a specific address and skip the next to addresses       
            //skip because they are the location to jump too
            PC = SwapNextTwoBytes();
            _currentMode = AddressModes.NO_PC_CHANGE;

            
            
        }
        private void JSR()
        {
            //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address
            var b = BitConverter.GetBytes((ushort)PC + 2);
            PushToStack(b[1]);
            PushToStack(b[0]);


            PC = SwapNextTwoBytes();
            _currentMode = AddressModes.NO_PC_CHANGE;

            
            
        }
        private void LDA(byte op)
        {
            Accumulator = op;
            AccumChanged();
            //HACK: the accum changed doesn't actually trigger, it didn't change (but was accessed)

            
            
        }
        private void LDX(byte op)
        {
            RegisterX = op;
            SetZeroAndNegFlag(RegisterX);

            
            
        }
        private void LDY(byte op)
        {
            RegisterY = op;
            SetZeroAndNegFlag(RegisterY);

            
            
        }
        private void LSR(byte op)
        {
            if ((op & 1) != 0) Flags.CarryFlag = true;
            else Flags.CarryFlag = false;
            op = (byte)(op >> 1);
            SetZeroAndNegFlag(op);
            Memory[curMemLocation] = op;

            
            
        }
        private void LSR()
        {
            //Accum Logical shift right, bit 0 Sets carry, bit 7 = 0
            //I did so much fucking work for this before i realized i misunderstood
            //the instruction.
            if ((Accumulator & 1) != 0) Flags.CarryFlag = true;
            else Flags.CarryFlag = false;
            Accumulator = (byte)(Accumulator >> 1);

            
            
        }
        private void NOP(byte op)
        {

            
            
        }
        private void NOP()
        {

            
            
        }
        private void ORA(byte index)
        {
            Accumulator |= index;
            SetZeroAndNegFlag(Accumulator);

            
            
        }
        private void PHA()
        {
            PushToStack(Accumulator);

            
            
        }
        private void PHP()
        {
            var b0 = Flags.CarryFlag;
            var b1 = Flags.ZeroFlag;
            var b2 = Flags.InterruptDisableFlag;
            var b3 = Flags.DecimalModeFlag;
            var b4 = true;
            var b5 = true;
            var b6 = Flags.OverflowFlag;
            var b7 = Flags.NegativeFlag;
            var flags = new bool[8]
            {
                b0, b1, b2, b3, b4, b5, b6, b7
            };
            byte range = 0;
            if (flags == null || flags.Length < 8)
            {
                range = 0;
            }

            for (int i = 0; i < 8; i++)
            {
                if (flags[i])
                {
                    range |= (byte)(1 << i);
                }
            }
            PushToStack(range);

            
            
        }
        private void PLA()
        {
            Accumulator = PopFromStack();
            SetZeroAndNegFlag(Accumulator);

            
            
        }
        private void PLP()
        {
            var status = PopFromStack();
            Flags.CarryFlag = (status & 1) != 0;
            Flags.ZeroFlag = (status & 2) != 0;
            Flags.InterruptDisableFlag = (status & 4) != 0;
            Flags.DecimalModeFlag = (status & 8) != 0;
            //Flags.BreakCommandFlag = (status & 16) != 0;
            //Flags.nullFlag = (status & 32) != 0;
            Flags.OverflowFlag = (status & 64) != 0;
            Flags.NegativeFlag = (status & 128) != 0;

            
            
        }
        private void ROL(byte op)
        {
            byte bit0;
            byte shiftedAccum;
            if (Flags.CarryFlag) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            Flags.CarryFlag = (op & 128) != 0 ? true : false;
            shiftedAccum = (byte)(op << 1); //shift the accum left 1
            Memory[curMemLocation] = (byte)(shiftedAccum | bit0);
            SetZeroAndNegFlag(Memory[curMemLocation]);

            
            
        }
        private void ROL() //Acumulator Overload
        {
            byte bit0;
            byte shiftedAccum;
            if (Flags.CarryFlag) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            Flags.CarryFlag = (Accumulator & 128) != 0 ? true : false;
            shiftedAccum = (byte)(Accumulator << 1); //shift the accum left 1
            Accumulator = (byte)(shiftedAccum | bit0);

            
            
        }
        private void ROR(byte op)
        {
            byte bit7;
            if (Flags.CarryFlag) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            Flags.CarryFlag = (op & 1) != 0 ? true : false;
            var shiftedAccum = (byte)(op >> 1); //shift the accum right 1
            Memory[curMemLocation] = (byte)(shiftedAccum | bit7);
            SetZeroAndNegFlag(Memory[curMemLocation]);

            
            
        }
        private void ROR()
        {
            byte bit7;
            // carry slots into bit7 and bit 0 is shifted into the carry
            if (Flags.CarryFlag) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            Flags.CarryFlag = (Accumulator & 1) != 0 ? true : false;
            var shiftedAccum = (byte)(Accumulator >> 1); //shift the accum right 1
            Accumulator = (byte)(shiftedAccum | bit7);

            
            
        }
        private void RTI()
        {
            var status = PopFromStack();
            Flags.CarryFlag = (status & 1) != 0;
            Flags.ZeroFlag = (status & 2) != 0;
            Flags.InterruptDisableFlag = (status & 4) != 0;
            Flags.DecimalModeFlag = (status & 8) != 0;
            Flags.OverflowFlag = (status & 64) != 0;
            Flags.NegativeFlag = (status & 128) != 0;
            var PC1 = PopFromStack();
            var PC2 = PopFromStack();
            PC = (ushort)(PC2 << 8 | PC1);
            _currentMode = AddressModes.NO_PC_CHANGE;

            
            
        }
        private void RTS()
        {
            var addr2 = PopFromStack();
            var addr1 = PopFromStack();
            PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
            _currentMode = AddressModes.NO_PC_CHANGE;

            
            
        }
        private void SBC(byte op)
        {
            ADC((byte)~op);

            
            
        }
        private void SEC()
        {
            Flags.CarryFlag = true;

            
            
        }
        private void SED()
        {
            Flags.DecimalModeFlag = true;

            
            
        }
        private void SEI()
        {
            Flags.InterruptDisableFlag = true;

            
            
        }
        private void STA(byte op)
        {

            Memory[curMemLocation] = Accumulator;

            
            
        }
        private void STX(byte op)
        {
            Memory[curMemLocation] = RegisterX;

            
            
        }
        private void STY(byte op)
        {
            Memory[curMemLocation] = RegisterY;

            
            
        }
        private void TAX()
        {
            RegisterX = Accumulator;
            SetZeroAndNegFlag(RegisterX);
        }
        private void TAY()
        {
            RegisterY = Accumulator;
            SetZeroAndNegFlag(RegisterY);
        }
        private void TSX()
        {
            RegisterX = StackPointer;
            SetZeroAndNegFlag(RegisterX); 
        }
        private void TXA()
        {
            Accumulator = RegisterX;
            SetZeroAndNegFlag(Accumulator);
        }
        private void TXS()
        {
            StackPointer = RegisterX;
        }
        private void TYA()
        {
            Accumulator = RegisterY;
            SetZeroAndNegFlag(Accumulator); 
        }
        #endregion
        #region Illegal Instructions
        #region Notes on Illegal Instructions
        /* These instructions are not official instructions for the 6502, but they are possible nonetheless.
        Since some games and software actually uses these instructions, and since they would work on real hardware, we must
        implement them. at some point atleast*/
        #endregion
        private void SLO(byte op)
        {
            ASL(op);
            Accumulator |= Memory[curMemLocation];   
        }
        private void ISC(byte op)
        {
            Memory[curMemLocation]++;
            SBC(Memory[curMemLocation]);
        }
        private void SHA(byte op) { throw new NotImplementedException(); }
        private void TAS(byte op) { throw new NotImplementedException(); }
        private void SHY(byte op) { throw new NotImplementedException(); }
        private void SHX(byte op) { throw new NotImplementedException(); }
        private void ANC(byte op) { throw new NotImplementedException(); }
        private void ANC() { throw new NotImplementedException(); }
        private void RLA(byte op)
        {
            ROL(Memory[curMemLocation]);
            AND(Memory[curMemLocation]);  
        }
        private void SRE(byte op)
        {
            ROR(Memory[curMemLocation]);
            EOR(Memory[curMemLocation]);
            SetZeroAndNegFlag(Memory[curMemLocation]);
        }
        private void ALR(byte op) { throw new NotImplementedException(); }
        private void RRA(byte op) { ROR(op); ADC(op); }
        private void ARR(byte op) { throw new NotImplementedException(); }
        private void LAX(byte op)
        {
            LDA(op);
            LDX(op); 
        }
        private void SAX(byte op)
        {
            Memory[curMemLocation] = ((byte)(Accumulator & RegisterX));
        }
        private void USBC(byte op)
        {
            SBC(op);
        }
        private void DCP(byte op)
        {
            DEC(op);
            CMP(Accumulator, Memory[curMemLocation]);
        }
        private void LXA(byte op) { throw new NotImplementedException(); }
        private void ANE(byte op) { throw new NotImplementedException(); }
        private void LAS(byte op) { throw new NotImplementedException(); }
        private void SBX(byte op) { throw new NotImplementedException(); }
        #endregion
    }
    public struct ProcessorStatus
    {
        //TODO: I hate this. just make it properties of the CPU.
        public bool CarryFlag { get; set; }
        public bool ZeroFlag { get; set; }
        public bool InterruptDisableFlag { get; set; }
        public bool DecimalModeFlag { get; set; }
        public bool BreakCommandFlag { get; set; }
        public bool nullFlag { get; set; } // Not used
        public bool OverflowFlag { get; set; }
        public bool NegativeFlag { get; set; }




    }
}
