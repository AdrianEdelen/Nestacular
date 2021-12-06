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
        public byte[] Memory = new byte[0x10000];
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
        //TODO: Fix Illegal NOPS that have operands
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
                    Flags.BreakCommandFlag,
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
        public byte PPUControlOne
        {
            get => Memory[0x2000];
            set => WriteToPPUControl1(value);
        }
        public byte PPUControlTwo
        {
            get => Memory[0x2001];
            set => WriteToPPUControl2(value);
        }
        public ProcessorStatus Flags = new ProcessorStatus();
        public PPU ppu = new PPU();
        public ALU alu = new ALU();
        public CU cu = new CU();

        public bool IsNewOP = false;//testing 
        public ulong CPUCycle = 0;
        private AddressModes _currentMode; //This is a workaround until I figure out a better way this will just be changed to an int
        public CPU()
        {
            //Startup routine
            //can probably make this more accurate
            CPUCycle = 4; //Why is this?
            Flags.InterruptDisableFlag = true; //I think this starts on true
            Flags.NegativeFlag = false; //test
            Flags.nullFlag = true;

        }
        public void CycleCPU()
        {
            if (!IsHalted)
                SearchForOpcode();
        }
        public void LoadRomIntoMemory(List<byte> LoadedRom)
        {
            //16384 bytes for PRG-ROM
            //PRG rom lower
            for (var i = 0; i < 0x4000; i++)
            {
                var addr = LoadedRom[0x10 + i];
                Memory[0x8000 + i] = addr; //add PRG to lower PRG-rom section
                Memory[0xC000 + i] = addr; //add PRF to upper PRG-rom section
            }
        }
        #region OPcode Calculations
        enum AddressModes
        {
            Implied, Absolute, AbsoluteY, AbsoluteX, Immediate, Indirect, XIndexedIndirect,
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
                case 0x06: ASL(ZeroPageMode(), PC); break;
                case 0x07: SLO(ZeroPageMode()); break;
                case 0x08: PHP(); break;
                case 0x09: ORA(ImmediateMode()); break;
                case 0x0A: ASL(); break;
                case 0x0B: ANC(); break;
                case 0x0C: NOP(AbsoluteMode()); break;
                case 0x0D: ORA(AbsoluteMode()); break;
                case 0x0E: ASL(AbsoluteMode(), PC); break;
                case 0x0F: SLO(AbsoluteMode()); break;
                #endregion
                #region 0x1
                case 0x10: BPL(); break;
                case 0x11: ORA(YIndexIndirectMode()); break;
                case 0x12: JamCPU(); break;
                case 0x13: SLO(YIndexIndirectMode()); break;
                case 0x14: BIT(ZeroPageMode()); break;
                case 0x15: ORA(ZeroPageXIndexedMode()); break;
                case 0x16: ASL(ZeroPageXIndexedMode(), PC); break;
                case 0x17: SLO(ZeroPageXIndexedMode()); break;
                case 0x18: CLC(); break;
                case 0x19: ORA(AbsoluteYMode()); break;
                case 0x1A: NOP(); break;
                case 0x1B: SLO(AbsoluteYMode()); break;
                case 0x1C: NOP(); break;
                case 0x1D: ORA(AbsoluteXMode()); break;
                case 0x1E: ASL(AbsoluteXMode(), PC); break;
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
                case 0x27: PLP(); break;
                case 0x28: RLA(ZeroPageMode()); break;
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
                case 0x34: NOP(); break;
                case 0x35: AND(ZeroPageXIndexedMode()); break;
                case 0x36: ROL(ZeroPageXIndexedMode()); break;
                case 0x37: RLA(ZeroPageXIndexedMode()); break;
                case 0x38: SEC(); break;
                case 0x39: AND(AbsoluteYMode()); break;
                case 0x3A: NOP(); break;
                case 0x3B: RLA(AbsoluteYMode()); break;
                case 0x3C: NOP(); break;
                case 0x3D: AND(AbsoluteXMode()); break;
                case 0x3E: ROL(AbsoluteXMode()); break;
                case 0x3F: RLA(AbsoluteXMode()); break;
                #endregion
                #region 0x4
                case 0x40: RTI(); break;
                case 0x41: EOR(XIndexedIndirectMode()); break;
                case 0x42: JamCPU(); break;
                case 0x43: SRE(XIndexedIndirectMode()); break;
                case 0x44: NOP(); break; //ZeroPage
                case 0x45: EOR(ZeroPageMode()); break;
                case 0x46: LSR(ZeroPageMode(), PC); break;
                case 0x47: throw new NotImplementedException();
                case 0x48: PHA(); break;
                case 0x49: EOR(ImmediateMode()); break;
                case 0x4A: LSR(); break;
                case 0x4B: throw new NotImplementedException();
                case 0x4C: JMP(); break;
                case 0x4D: EOR(AbsoluteMode()); break;
                case 0x4E: LSR(AbsoluteMode(), PC); break;
                case 0x4F: throw new NotImplementedException();
                #endregion
                #region 0x5
                case 0x50: BVC(); break;
                case 0x51: EOR(XIndexedIndirectMode()); break;
                case 0x52: JamCPU(); break;
                case 0x53: throw new NotImplementedException();
                case 0x54: throw new NotImplementedException();
                case 0x55: throw new NotImplementedException();
                case 0x56: throw new NotImplementedException();
                case 0x57: throw new NotImplementedException();
                case 0x58: throw new NotImplementedException();
                case 0x59: throw new NotImplementedException();
                case 0x5A: throw new NotImplementedException();
                case 0x5B: throw new NotImplementedException();
                case 0x5C: throw new NotImplementedException();
                case 0x5D: throw new NotImplementedException();
                case 0x5E: throw new NotImplementedException();
                case 0x5F: throw new NotImplementedException();
                #endregion
                #region 0x6
                case 0x60: RTS(); break;
                case 0x61: ADC(XIndexedIndirectMode()); break;
                case 0x62: JamCPU(); break;
                case 0x63: throw new NotImplementedException();
                case 0x64: throw new NotImplementedException();
                case 0x65: ADC(ZeroPageMode()); break;
                case 0x66: //ROR Zero Page
                    byte bit7;
                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[PeekNextByte()] & 1) != 0 ? true : false;
                    var shiftedAccum = (byte)(Memory[PeekNextByte()] >> 1); //shift the accum right 1
                    Memory[PeekNextByte()] = (byte)(shiftedAccum | bit7);
                    break;
                case 0x67: throw new NotImplementedException();
                case 0x68: //PLA set accumulator from the stack
                    Accumulator = PopFromStack();
                    SetZeroAndNegFlag(Accumulator);
                    break;
                case 0x69: ADC(PeekNextByte()); break;
                case 0x6A: //ROR Rotate Right
                           // carry slots into bit7 and bit 0 is shifted into the carry
                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Accumulator & 1) != 0 ? true : false;
                    shiftedAccum = (byte)(Accumulator >> 1); //shift the accum right 1
                    Accumulator = (byte)(shiftedAccum | bit7);
                    break;
                case 0x6B: throw new NotImplementedException();
                case 0x6C: //JMP Indirect
                    var ind = SwapNextTwoBytes();
                    if ((ind & 0x00FF) != 0)
                        PC = (ushort)(ind + 1);
                    else
                    {
                        var tempInd = Memory[ind];
                        var tempInd2 = Memory[(ind + 1)];
                        var newInd = (ushort)(tempInd2 << 8 | tempInd);
                        PC = newInd;
                    }
                    break;
                case 0x6D: ADC(AbsoluteMode()); break;
                case 0x6E: //ROR Absolute
                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[SwapNextTwoBytes()] & 1) != 0 ? true : false;
                    shiftedAccum = (byte)(Accumulator >> 1); //shift the accum right 1
                    Memory[SwapNextTwoBytes()] = (byte)(shiftedAccum | bit7);
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    break;
                case 0x6F: throw new NotImplementedException();
                #endregion
                #region 0x7
                case 0x70: BVS(); break;
                case 0x71: ADC(YIndexIndirectMode()); break;
                case 0x72: JamCPU(); break;
                case 0x73: throw new NotImplementedException();
                case 0x74: throw new NotImplementedException();
                case 0x75: throw new NotImplementedException();
                case 0x76: throw new NotImplementedException();
                case 0x77: throw new NotImplementedException();
                case 0x78: SEI(); break;
                case 0x79: throw new NotImplementedException();
                case 0x7A: throw new NotImplementedException();
                case 0x7B: throw new NotImplementedException();
                case 0x7C: throw new NotImplementedException();
                case 0x7D: throw new NotImplementedException();
                case 0x7E: throw new NotImplementedException();
                case 0x7F: throw new NotImplementedException();
                #endregion
                #region 0x8
                case 0x80: NOP(); break;
                case 0x81: // STA: indirect x
                    Memory[CalcIndirectX()] = Accumulator;
                    break;
                case 0x82: NOP(); break;
                case 0x83: throw new NotImplementedException();
                case 0x84: //STY Zero Page
                    Memory[PeekNextByte()] = RegisterY;
                    break;
                case 0x85: //STA Store accum into Zero page
                    //THIS MAY BE A PROBLEM
                    var pos = PeekNextByte();
                    sb.Append($"{PeekNextByte().ToString("X2")}    STA ${PeekNextByte().ToString("X2")} = {Memory[pos].ToString("X2")}");
                    Memory[pos] = Accumulator;
                    break;
                case 0x86: //STX Store X Register STX (val) Zero Page
                    pos = PeekNextByte();
                    Memory[pos] = RegisterX;
                    break;
                case 0x87: throw new NotImplementedException();
                case 0x88: ///DEY Decrement Register Y
                    RegisterY--;
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0x89: throw new NotImplementedException();
                case 0x8A: //TXA Transfer X to Accumulator
                    Accumulator = RegisterX;
                    SetZeroAndNegFlag(Accumulator);
                    break;
                case 0x8B: throw new NotImplementedException();
                case 0x8C: //STY Absolute
                    Memory[SwapNextTwoBytes()] = RegisterY;
                    break;
                case 0x8D: //STA Absolute
                    var memPos = SwapNextTwoBytes();
                    Memory[memPos] = Accumulator;
                    break;
                case 0x8E: //STX: Absolute
                    Memory[SwapNextTwoBytes()] = RegisterX;
                    break;
                case 0x8F: throw new NotImplementedException();
                #endregion
                #region 0x9
                case 0x90: BCC(); break;
                case 0x91: //STA Indirect Y
                    Memory[CalcIndirectY()] = Accumulator;
                    break;
                case 0x92: JamCPU(); break;
                case 0x93: throw new NotImplementedException();
                case 0x94: throw new NotImplementedException();
                case 0x95: throw new NotImplementedException();
                case 0x96: throw new NotImplementedException();
                case 0x97: throw new NotImplementedException();
                case 0x98: //TYA Transfer Y to Accumulator
                    Accumulator = RegisterY;
                    SetZeroAndNegFlag(Accumulator);
                    break;
                case 0x99: throw new NotImplementedException();
                case 0x9A: //TSX Transfer register x TO stack pointer
                    StackPointer = RegisterX;
                    break;
                case 0x9B: throw new NotImplementedException();
                case 0x9C: throw new NotImplementedException();
                case 0x9D: throw new NotImplementedException();
                case 0x9E: throw new NotImplementedException();
                case 0x9F: throw new NotImplementedException();
                #endregion
                #region 0xA
                case 0xA0: //LDY Load Y register Immediate
                    RegisterY = PeekNextByte();
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0xA1: //LDA: Indirect X
                    Accumulator = Memory[CalcIndirectX()];
                    break;
                case 0xA2://LDX LoadX Register LDX (val)
                    //store the value in the next address in Register x
                    RegisterX = PeekNextByte();
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xA3: throw new NotImplementedException();
                case 0xA4: // LDY zero page
                    RegisterY = Memory[PeekNextByte()];
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0xA5: LDA(ZeroPageMode()); break;
                case 0xA6: //LDX Zero Page
                    RegisterX = Memory[PeekNextByte()];
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xA7: throw new NotImplementedException();
                case 0xA8: //TAY Transfer Accum into Register Y
                    RegisterY = Accumulator;
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0xA9: //LDA Load Accumulator LDA (val)
                    Accumulator = PeekNextByte();
                    break;
                case 0xAA: //TAX Transfer Accum into register X
                    RegisterX = Accumulator;
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xAB: throw new NotImplementedException();
                case 0xAC: //LDY Absolute
                    RegisterY = Memory[SwapNextTwoBytes()];
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0xAD: //LDA: Absolute
                    Accumulator = Memory[SwapNextTwoBytes()];
                    break;
                case 0xAE: //LDX: Absolute
                    RegisterX = Memory[SwapNextTwoBytes()];
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xAF: throw new NotImplementedException();
                #endregion
                #region 0xB
                case 0xB0: BCS(); break;
                case 0xB1: //LDA Indirect Y
                    Accumulator = Memory[CalcIndirectY()];
                    SetZeroAndNegFlag(Accumulator);
                    break;
                case 0xB2: JamCPU(); break;
                case 0xB3: throw new NotImplementedException();
                case 0xB4: throw new NotImplementedException();
                case 0xB5: throw new NotImplementedException();
                case 0xB6: throw new NotImplementedException();
                case 0xB7: throw new NotImplementedException();
                case 0xB8: //CLV Clear overflow 
                    Flags.OverflowFlag = false;
                    break;
                case 0xB9: //LDA Absolute Y
                    {
                        var a = SwapNextTwoBytes();
                        var ba = Memory[a];
                        byte ca = (byte)(ba + RegisterY);
                        Accumulator = ca;
                        break;
                    }
                case 0xBA: //TSX Transfer Stack Pointer to Register X
                    RegisterX = StackPointer;
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xBB: throw new NotImplementedException();
                case 0xBC: throw new NotImplementedException();
                case 0xBD: throw new NotImplementedException();
                case 0xBE: throw new NotImplementedException();
                case 0xBF: throw new NotImplementedException();
                #endregion
                #region 0xC
                case 0xC0: //CPY CMP Y Immediate
                    CMP(RegisterY, PeekNextByte());
                    break;
                case 0xC1: //CMP Indirect X
                    CMP(Accumulator, Memory[CalcIndirectX()]);
                    break;
                case 0xC2: NOP(); break;
                case 0xC3: throw new NotImplementedException();
                case 0xC4: //CPY Zero Page
                    CMP(RegisterY, Memory[PeekNextByte()]);
                    break;
                case 0xC5: //CMP ZeroPage
                    CMP(Accumulator, Memory[PeekNextByte()]);
                    break;
                case 0xC6: //DEC Zero Page
                    Memory[PeekNextByte()]--;
                    SetZeroAndNegFlag(Memory[PeekNextByte()]);
                    break;
                case 0xC7: throw new NotImplementedException();
                case 0xC8: // INY Increment Register Y
                    RegisterY++;
                    SetZeroAndNegFlag(RegisterY);
                    break;
                case 0xC9: //CMP compare operand and Accum immediate value
                    CMP(Accumulator, PeekNextByte());
                    break;
                case 0xCA: // DEX Decrement Register X
                    RegisterX--;
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xCB: throw new NotImplementedException();
                case 0xCC: // CPY Absolute
                    CMP(RegisterY, Memory[SwapNextTwoBytes()]);
                    break;
                case 0xCD: //CMP Absolute
                    CMP(Accumulator, Memory[SwapNextTwoBytes()]);
                    break;
                case 0xCE: //DEC Absolute
                    Memory[SwapNextTwoBytes()]--;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    break;
                case 0xCF: throw new NotImplementedException();
                #endregion
                #region 0xD
                case 0xD0: BNE(); break;
                case 0xD1: //CMP Indirect Y
                    CMP(Accumulator, Memory[CalcIndirectY()]);
                    break;
                case 0xD2: JamCPU(); break;
                case 0xD3: throw new NotImplementedException();
                case 0xD4: throw new NotImplementedException();
                case 0xD5: throw new NotImplementedException();
                case 0xD6: throw new NotImplementedException();
                case 0xD7: throw new NotImplementedException();
                case 0xD8: //CLD Clear Dec Flag
                    Flags.DecimalModeFlag = false;
                    break;
                case 0xD9: throw new NotImplementedException();
                case 0xDA: throw new NotImplementedException();
                case 0xDB: throw new NotImplementedException();
                case 0xDC: throw new NotImplementedException();
                case 0xDD: throw new NotImplementedException();
                case 0xDE: throw new NotImplementedException();
                case 0xDF: throw new NotImplementedException();
                #endregion
                #region 0xE
                //0xE 5 Left
                case 0xE0: //CPX CMP X Immediate
                    CMP(RegisterX, PeekNextByte());
                    break;
                case 0xE1: // SBC Inidrect X;
                    ADC((byte)~Memory[CalcIndirectX()]);
                    break;
                case 0xE2: NOP(); break;
                case 0xE3: throw new NotImplementedException();
                case 0xE4: //CPX Zero Page
                    CMP(RegisterX, Memory[PeekNextByte()]);
                    break;
                case 0xE5: //SBC Zero Page
                    ADC((byte)~Memory[PeekNextByte()]);
                    break;
                case 0xE6: //INC ZeroPage
                    Memory[PeekNextByte()]++;
                    SetZeroAndNegFlag(Memory[PeekNextByte()]);
                    break;
                case 0xE7: throw new NotImplementedException();
                case 0xE8: //INX Increment Register x
                    RegisterX++;
                    SetZeroAndNegFlag(RegisterX);
                    break;
                case 0xE9: //SBC Subtract with carry                
                    //we will see if this shit works.               
                    ADC((byte)~PeekNextByte());
                    break;
                case 0xEA: NOP(); break;
                case 0xEB: throw new NotImplementedException();
                case 0xEC: //CPX Absolute
                    CMP(RegisterX, AbsoluteMode());
                    break;
                case 0xED: //SBC Absolute
                    ADC((byte)~AbsoluteMode());
                    break;
                case 0xEE: // INC Absolute
                    Memory[SwapNextTwoBytes()]++;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    break;
                case 0xEF: throw new NotImplementedException();
                #endregion
                #region 0xF
                case 0xF0: BEQ(); break;
                case 0xF1: SBC(YIndexIndirectMode()); break;
                case 0xF2: JamCPU(); break;
                case 0xF3: throw new NotImplementedException();
                case 0xF4: throw new NotImplementedException();
                case 0xF5: throw new NotImplementedException();
                case 0xF6: throw new NotImplementedException();
                case 0xF7: throw new NotImplementedException();
                case 0xF8: SED(); break;
                case 0xF9: throw new NotImplementedException();
                case 0xFA: throw new NotImplementedException();
                case 0xFB: throw new NotImplementedException();
                case 0xFC: throw new NotImplementedException();
                case 0xFD: throw new NotImplementedException();
                case 0xFE: throw new NotImplementedException();
                case 0xFF: throw new NotImplementedException();
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
            }
            if (prevAccum != Accumulator) AccumChanged();
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
        private ushort CalcIndirectX()
        {
            var indexByte = PeekNextByte();
            var newPos = (byte)(indexByte + RegisterX);
            var calcedPos = Memory[newPos];
            var calcedPos2 = Memory[(byte)(newPos + 1)];
            ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
            return addr;
        }
        private ushort CalcIndirectY()
        {
            byte indexByte = PeekNextByte();
            byte b1 = Memory[indexByte];
            byte b2 = Memory[indexByte + 1];
            if (indexByte == 0xFF) b2++;
            ushort tempShort = 0x00;
            tempShort = (ushort)(b2 << 8 | b1);
            tempShort += RegisterY;
            return tempShort;
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
            if (DoBranch) PC += (ushort)jumpDistance;
            else PC += 2;
        }
        private void AccumChanged()
        {
            if (Accumulator != 0x00) Flags.ZeroFlag = false;
            else Flags.ZeroFlag = true;
            if ((Accumulator & 128) != 0) Flags.NegativeFlag = true;
            else Flags.NegativeFlag = false;
        }
        private ushort SwapNextTwoBytes() { return (ushort)(PeekByteAfterNext() << 8 | PeekNextByte()); }
        private void CycleCPU(int cycles)
        {
            //TODO, Change this to something that will actualyl delay the whole deal by whatevr the time needs to be.
            //then this method would be called to actually 'cycle' the cpu
            for (var i = 0; i < cycles; i++) CPUCycle++;
        }
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
            return Memory[SwapNextTwoBytes()];
        }
        private byte AbsoluteXMode()
        {
            _currentMode = AddressModes.AbsoluteX;
            return (byte)(Memory[SwapNextTwoBytes()] + RegisterX);
        }
        private byte AbsoluteYMode()
        {
            _currentMode = AddressModes.AbsoluteY;
            return (byte)(Memory[SwapNextTwoBytes()] + RegisterY);

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
        private void IndirectMode()
        {
            _currentMode = AddressModes.Indirect;
        }
        private byte XIndexedIndirectMode()
        {
            _currentMode = AddressModes.XIndexedIndirect;
            return Memory[CalcIndirectX()];
        }
        private byte YIndexIndirectMode()
        {
            _currentMode = AddressModes.YindexedIndirect;
            return Memory[CalcIndirectY()];
        }
        private void RelativeMode()
        {
            _currentMode = AddressModes.Relative;
        }
        private byte ZeroPageMode()
        {
            _currentMode = AddressModes.ZeroPage;
            return Memory[PeekNextByte()];
        }
        private byte ZeroPageXIndexedMode() { throw new NotImplementedException(); }
        private void ZeroPageYIndexed() { throw new NotImplementedException(); }
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
        private void ASL(byte op, ushort originalPC)
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
            Memory[originalPC] = temp;
            SetZeroAndNegFlag(Memory[originalPC]);
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
            if ((Accumulator & pos) == 0x00)
            {
                Flags.ZeroFlag = true;
            }
            if ((pos & 128) != 0)
                Flags.NegativeFlag = true;
            else Flags.NegativeFlag = false;
            if ((pos & 64) != 0)
                Flags.OverflowFlag = true;
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
        private void BRK() { throw new NotImplementedException(); }
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
        private void CLD() { throw new NotImplementedException(); }
        private void CLI() { throw new NotImplementedException(); }
        private void CLV() { throw new NotImplementedException(); }
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
        private void DEC() { throw new NotImplementedException(); }
        private void DEX() { throw new NotImplementedException(); }
        private void DEY() { throw new NotImplementedException(); }
        private void EOR(byte op)
        {
            Accumulator ^= op;
        }

        private void INC() { throw new NotImplementedException(); }
        private void INX() { throw new NotImplementedException(); }
        private void INY() { throw new NotImplementedException(); }
        private void JMP()
        {
            //Move the PC to a specific address and skip the next to addresses       
            //skip because they are the location to jump too
            PC = SwapNextTwoBytes();
        }
        private void JSR()
        {
            //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address
            var b = BitConverter.GetBytes((ushort)PC + 2);
            PushToStack(b[1]);
            PushToStack(b[0]);


            PC = SwapNextTwoBytes();
        }
        private void LDA(byte op)
        {
            Accumulator = op;

        }
        private void LDX() { throw new NotImplementedException(); }
        private void LDY() { throw new NotImplementedException(); }
        private void LSR(byte op, ushort originalPC)
        {
            var temp = op;
            if ((temp & 1) != 0)
                Flags.CarryFlag = true;
            else
                Flags.CarryFlag = false;
            temp = (byte)(temp >> 1);
            SetZeroAndNegFlag(temp);
            Memory[originalPC] = temp;

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
        private void PLA() { throw new NotImplementedException(); }
        private void PLP()
        {
            var status = PopFromStack();
            Flags.CarryFlag = (status & 1) != 0;
            Flags.ZeroFlag = (status & 2) != 0;
            Flags.InterruptDisableFlag = (status & 4) != 0;
            Flags.DecimalModeFlag = (status & 8) != 0;
            Flags.BreakCommandFlag = (status & 16) != 0;
            Flags.nullFlag = (status & 32) != 0;
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
            Flags.CarryFlag = (Memory[op] & 128) != 0 ? true : false;
            shiftedAccum = (byte)(Memory[op] << 1); //shift the accum left 1
            Memory[op] = (byte)(shiftedAccum | bit0);
            SetZeroAndNegFlag(Memory[op]);
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
        private void ROR() { throw new NotImplementedException(); }
        private void RTI()
        {
            var status = PopFromStack();
            Flags.CarryFlag = (status & 1) != 0;
            Flags.ZeroFlag = (status & 2) != 0;
            Flags.InterruptDisableFlag = (status & 4) != 0;
            Flags.DecimalModeFlag = (status & 8) != 0;
            Flags.BreakCommandFlag = (status & 16) != 0;
            Flags.nullFlag = (status & 32) != 0;
            Flags.OverflowFlag = (status & 64) != 0;
            Flags.NegativeFlag = (status & 128) != 0;
            var PC1 = PopFromStack();
            var PC2 = PopFromStack();
            PC = (ushort)(PC2 << 8 | PC1);
        }
        private void RTS()
        {
            var addr2 = PopFromStack();
            var addr1 = PopFromStack();
            PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
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
        private void STA() { throw new NotImplementedException(); }
        private void STX() { throw new NotImplementedException(); }
        private void STY() { throw new NotImplementedException(); }
        private void TAX() { throw new NotImplementedException(); }
        private void TAY() { throw new NotImplementedException(); }
        private void TSX() { throw new NotImplementedException(); }
        private void TXA() { throw new NotImplementedException(); }
        private void TXS() { throw new NotImplementedException(); }
        private void TYA() { throw new NotImplementedException(); }
        #endregion
        #region Illegal Instructions
        #region Notes on Illegal Instructions
        /* These instructions are not official instructions for the 6502, but they are possible nonetheless.
        Since some games and software actually uses these instructions, and since they would work on real hardware, we must
        implement them. at some point atleast*/
        #endregion
        private void SLO(byte op) { throw new NotImplementedException(); }
        private void ANC(byte op) { throw new NotImplementedException(); }
        private void ANC() { throw new NotImplementedException(); }
        private void RLA(byte op) { throw new NotImplementedException(); }
        private void SRE(byte op) {throw new NotImplementedException(); }
        #endregion
        #region PPU Operations
        #region Notes On PPU
        /* Pretty far away from this at the moment.*/
        #endregion
        public void WriteToPPUControl1(byte dataToWrite)
        {
            //need to include Mirror writes in here as well
            //2000-2007 are mirrored every 8 bytes from 2008 to 3FFF, so 2008 8 equals whatever is in 2000 for example
            Memory[0x2000] = dataToWrite;
        }
        public void WriteToPPUControl2(byte dataToWrite)
        {
            //need to do the mirroring here as well
            Memory[0x2001] = dataToWrite;
        }
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
