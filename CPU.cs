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
        //TODO abstract out some of these arbitrary bitshifts and movements, stuff like PC +=2 should be more like FetchNextInstruction();
        //TODO write an IsBitSet method that just lets you select the index and returns a bool
        byte StackPointer = 0xFD;
        public ushort PC = 0xC000; //skip the header for now
        public byte Accumulator = 0x00;
        public byte RegisterY = 0x00;
        public byte RegisterX = 0x00;

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

        //testing 
        public bool IsNewOP = false;

        public ulong CPUCycle = 0;
        public CPU()
        {
            //Startup routine
            //can probably make this more accurate
            CPUCycle = 4; //Why is this?
            Flags.InterruptDisableFlag = false;
            Flags.NegativeFlag = false; //test

        }

        public void CycleCPU()
        {
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
        private void SearchForOpcode()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append($"{PC.ToString("X4")}  {Memory[PC].ToString("X2")} ");
            var prevAccum = Accumulator;
            IsNewOP = false;

            switch (Memory[PC])
            {

                //151 OPS Left WOWOWOW
                #region 0x0
                //0x0 8 Left
                case 0x00:
                    throw new NotImplementedException();
                case 0x01://ORA indirect X

                    Accumulator |= Memory[CalcIndirectX()];
                    SetZeroAndNegFlag(Accumulator);
                    PC += 2;
                    break;
                case 0x02:
                    throw new NotImplementedException();
                case 0x03:
                    throw new NotImplementedException();
                case 0x04:
                    throw new NotImplementedException();
                case 0x05: //ORA Zero Page

                    Accumulator |= Memory[PeekNextByte()];
                    PC += 2;
                    break;
                case 0x06: //ASL Zero Page

                    var temp = Memory[PeekNextByte()];
                    if ((temp & 128) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    temp = (byte)(temp << 1);
                    Memory[PeekNextByte()] = temp;

                    PC += 2;
                    break;
                case 0x07:
                    throw new NotImplementedException();
                case 0x08: //PHP Push Processor Status

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
                    PC++;
                    sb.Append("      PHP");
                    break;
                case 0x09: //ORA or the accum and operand

                    Accumulator |= PeekNextByte();

                    PC += 2;
                    break;
                case 0x0A: //ASL Shift Accum Left, similar to LSR
                    if ((Accumulator & 128) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    Accumulator = (byte)(Accumulator << 1);
                    PC++;
                    break;
                case 0x0B:
                    throw new NotImplementedException();
                case 0x0C:
                    throw new NotImplementedException();
                case 0x0D: //ORA Absolute

                    Accumulator |= Memory[SwapNextTwoBytes()];

                    PC += 3;
                    break;
                case 0x0E: //ASL absolute

                    temp = Memory[SwapNextTwoBytes()];
                    if ((temp & 128) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    temp = (byte)(temp << 1);
                    Memory[SwapNextTwoBytes()] = temp;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0x0F:
                    break;
                #endregion
                #region 0x1
                //0x1 13 Left
                case 0x10: //BPL Branch on plus

                    Branch(!Flags.NegativeFlag, sb, "BPL");
                    break;
                case 0x11: //ORA  Indirect Y
                    Accumulator |= Memory[CalcIndirectY()];
                    SetZeroAndNegFlag(Accumulator);
                    PC += 2;
                    break;
                case 0x12:
                    throw new NotImplementedException();
                case 0x13:
                    throw new NotImplementedException();
                case 0x14:
                    throw new NotImplementedException();
                case 0x15:
                    throw new NotImplementedException();
                case 0x16:
                    throw new NotImplementedException();
                case 0x17:
                    throw new NotImplementedException();
                case 0x18: //CEC clear carry

                    sb.Append("      CLC");
                    Flags.CarryFlag = false;
                    PC++;
                    break;
                case 0x19: //ORA  Absolute Y
                    {
                        var a = SwapNextTwoBytes();
                        var ba = Memory[a];
                        byte ca = (byte)(ba + RegisterY);

                        Accumulator |= ca;
                        PC += 3;
                        break;
                    }
                case 0x1A:
                    throw new NotImplementedException();
                case 0x1B:
                    throw new NotImplementedException();
                case 0x1C:
                    throw new NotImplementedException();
                case 0x1D:
                    throw new NotImplementedException();
                case 0x1E:
                    throw new NotImplementedException();
                case 0x1F:
                    throw new NotImplementedException();
                #endregion
                #region 0x2
                //0x2 6 Left
                case 0x20: //JSR Jump SubRoutine JSR (addr addr)
                    //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address
                    var b = BitConverter.GetBytes((ushort)PC + 2);
                    PushToStack(b[1]);
                    PushToStack(b[0]);
                    PC = SwapNextTwoBytes();
                    break;
                case 0x21: //AND Indirect X

                    Accumulator = (byte)(Accumulator & Memory[CalcIndirectX()]);
                    SetZeroAndNegFlag(Accumulator);
                    PC += 2;
                    break;
                case 0x22:
                    throw new NotImplementedException();
                case 0x23:
                    throw new NotImplementedException();
                case 0x24: //BIT 

                    //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
                    //value store iat the tested address
                    var pos = PeekNextByte();
                    if ((Accumulator & Memory[pos]) == 0x00)
                    {
                        Flags.ZeroFlag = true;
                    }
                    if ((Memory[pos] & 128) != 0)
                        Flags.NegativeFlag = true;
                    else Flags.NegativeFlag = false;
                    if ((Memory[pos] & 64) != 0)
                        Flags.OverflowFlag = true;
                    else Flags.OverflowFlag = false;
                    sb.Append($"{PeekNextByte().ToString("X2")}    BIT ${PeekNextByte().ToString("X2")} = {Accumulator.ToString("X2")}");
                    PC += 2;
                    break;
                case 0x25: //AND zero page



                    Accumulator = (byte)(Accumulator & Memory[PeekNextByte()]);
                    SetZeroAndNegFlag(Accumulator);
                    sb.Append("      AND");
                    PC += 2;
                    break;
                case 0x26: //ROL Zero Page

                    byte bit0;
                    byte shiftedAccum;
                    if (Flags.CarryFlag) bit0 = 1;
                    else bit0 = 0;

                    //check bit7 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[PeekNextByte()] & 128) != 0 ? true : false;
                    shiftedAccum = (byte)(Memory[PeekNextByte()] << 1); //shift the accum left 1
                    Memory[PeekNextByte()] = (byte)(shiftedAccum | bit0);

                    PC += 2;
                    break;
                case 0x27:
                    throw new NotImplementedException();
                case 0x28: //PLP pull stack to processor status

                    var status = PopFromStack();
                    Flags.CarryFlag = (status & 1) != 0;
                    Flags.ZeroFlag = (status & 2) != 0;
                    Flags.InterruptDisableFlag = (status & 4) != 0;
                    Flags.DecimalModeFlag = (status & 8) != 0;
                    Flags.BreakCommandFlag = (status & 16) != 0;
                    Flags.nullFlag = (status & 32) != 0;
                    Flags.OverflowFlag = (status & 64) != 0;
                    Flags.NegativeFlag = (status & 128) != 0;

                    PC++;
                    break;
                case 0x29: //AND Bitwise and with operand and accum
                    var aa = PeekNextByte();
                    var bb = Accumulator;
                    byte c = (byte)(aa & bb);
                    Accumulator = (byte)c;
                    SetZeroAndNegFlag(Accumulator);
                    sb.Append("      AND");
                    PC += 2;
                    break;
                case 0x2A: //ROL Rotate Left
                    if (Flags.CarryFlag) bit0 = 1;
                    else bit0 = 0;

                    //check bit7 to see what the new carry flag chould be
                    Flags.CarryFlag = (Accumulator & 128) != 0 ? true : false;
                    shiftedAccum = (byte)(Accumulator << 1); //shift the accum left 1
                    Accumulator = (byte)(shiftedAccum | bit0);

                    PC++;
                    break;
                case 0x2B:
                    throw new NotImplementedException();
                case 0x2C: //BIT Absolute
                    //pos2 because pos is used, this weill be fixed in the refactor
                    var pos2 = SwapNextTwoBytes();
                    if ((Accumulator & Memory[pos2]) == 0x00)
                    {
                        Flags.ZeroFlag = true;
                    }
                    if ((Memory[pos2] & 128) != 0)
                        Flags.NegativeFlag = true;
                    else Flags.NegativeFlag = false;
                    if ((Memory[pos2] & 64) != 0)
                        Flags.OverflowFlag = true;
                    else Flags.OverflowFlag = false;
                    sb.Append($"{PeekNextByte().ToString("X2")}    BIT ${PeekNextByte().ToString("X2")} = {Accumulator.ToString("X2")}");
                    PC += 3;
                    break;
                case 0x2D: //AND Absolute

                    Accumulator = (byte)(Accumulator & Memory[SwapNextTwoBytes()]);

                    sb.Append("      AND");
                    PC += 3;
                    break;
                case 0x2E: // ROL Absolute


                    if (Flags.CarryFlag) bit0 = 1;
                    else bit0 = 0;

                    //check bit7 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[SwapNextTwoBytes()] & 128) != 0 ? true : false;
                    shiftedAccum = (byte)(Memory[SwapNextTwoBytes()] << 1); //shift the accum left 1
                    Memory[SwapNextTwoBytes()] = (byte)(shiftedAccum | bit0);
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;

                case 0x2F:
                    throw new NotImplementedException();
                #endregion
                #region 0x3
                //0x3 14 Left
                case 0x30: //BMI Branch on minus
                    Branch(Flags.NegativeFlag, sb, "BMI");
                    break;
                case 0x31: //AND Indirect Y
                    Accumulator &= Memory[CalcIndirectY()];
                    SetZeroAndNegFlag(Accumulator);
                    PC += 2;
                    break;
                    throw new NotImplementedException();
                case 0x32:
                    throw new NotImplementedException();
                case 0x33:
                    throw new NotImplementedException();
                case 0x34:
                    throw new NotImplementedException();
                case 0x35:
                    throw new NotImplementedException();
                case 0x36:
                    throw new NotImplementedException();
                case 0x37:
                    throw new NotImplementedException();
                case 0x38: //SEC Set Carry Flag

                    sb.Append("      SEC");
                    Flags.CarryFlag = true;
                    PC++;
                    CycleCPU(2);
                    break;
                case 0x39: //AND Absolute Y
                    {
                        var a = SwapNextTwoBytes();
                        var ba = Memory[a];
                        byte ca = (byte)(ba + RegisterY);

                        Accumulator &= ca;
                        PC += 3;
                        break;
                    }

                case 0x3A:
                    throw new NotImplementedException();
                case 0x3B:
                    throw new NotImplementedException();
                case 0x3C:
                    throw new NotImplementedException();
                case 0x3D:
                    throw new NotImplementedException();
                case 0x3E:
                    throw new NotImplementedException();
                case 0x3F:
                    throw new NotImplementedException();
                #endregion
                #region 0x4
                //0x4 7 Left
                case 0x40: //RTI Return From Interrupt

                    status = PopFromStack();
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
                    break;
                case 0x41: // EOR Indirect X

                    Accumulator ^= Memory[CalcIndirectX()];
                    PC += 2;
                    break;
                case 0x42:
                    throw new NotImplementedException();
                case 0x43:
                    throw new NotImplementedException();
                case 0x44:
                    throw new NotImplementedException();
                case 0x45: //EOR Zero Page

                    Accumulator ^= Memory[PeekNextByte()];
                    PC += 2;
                    break;
                case 0x46: //LSR Zero Page

                    temp = Memory[PeekNextByte()];
                    if ((temp & 1) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    temp = (byte)(temp >> 1);
                    SetZeroAndNegFlag(temp);
                    Memory[PeekNextByte()] = temp;
                    PC += 2;
                    break;
                case 0x47:
                    throw new NotImplementedException();
                case 0x48: //PHA push Accum to stack

                    PushToStack(Accumulator);
                    PC++;
                    break;
                case 0x49: //EOR Exlusive OR the accum

                    Accumulator ^= PeekNextByte();

                    PC += 2;
                    break;
                case 0x4A: //LSR: Accum Logical shift right, bit 0 Sets carry, bit 7 = 0
                    //I did so much fucking work for this before i realized i misunderstood
                    //the instruction.
                    if ((Accumulator & 1) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    Accumulator = (byte)(Accumulator >> 1);
                    PC++;
                    break;
                case 0x4B:
                    throw new NotImplementedException();
                case 0x4C: //jmp - Jump (addr1 addr2)

                    //Move the PC to a specific address and skip the next to addresses       
                    //skip because they are the location to jump too
                    sb.Append($"{PeekNextByte().ToString("X2")} {PeekByteAfterNext().ToString("X2")}");
                    PC = SwapNextTwoBytes();
                    sb.Append($" JMP ${PC.ToString("X2")}");
                    CycleCPU(3);
                    break;
                case 0x4D: //EOR Absolute
                    Accumulator ^= Memory[SwapNextTwoBytes()];
                    PC += 3;
                    break;
                case 0x4E: //LSR Absolute
                    temp = Memory[SwapNextTwoBytes()];

                    if ((temp & 1) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    temp = (byte)(temp >> 1);

                    Memory[SwapNextTwoBytes()] = temp;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0x4F:
                    throw new NotImplementedException();
                #endregion
                #region 0x5
                //0x5 15 Left
                case 0x50: //BVC Branch on OVerflow Clear

                    Branch(!Flags.OverflowFlag, sb, "BVC");
                    break;
                case 0x51:
                    Accumulator ^= Memory[CalcIndirectY()];
                    PC += 2;
                    break;
                    throw new NotImplementedException();
                case 0x52:
                    throw new NotImplementedException();
                case 0x53:
                    throw new NotImplementedException();
                case 0x54:
                    throw new NotImplementedException();
                case 0x55:
                    throw new NotImplementedException();
                case 0x56:
                    throw new NotImplementedException();
                case 0x57:
                    throw new NotImplementedException();
                case 0x58:
                    throw new NotImplementedException();
                case 0x59:
                    throw new NotImplementedException();
                case 0x5A:
                    throw new NotImplementedException();
                case 0x5B:
                    throw new NotImplementedException();
                case 0x5C:
                    throw new NotImplementedException();
                case 0x5D:
                    throw new NotImplementedException();
                case 0x5E:
                    throw new NotImplementedException();
                case 0x5F:
                    throw new NotImplementedException();
                #endregion
                #region 0x6
                //0x6 7 Left
                case 0x60: //RTS Return from SubRoutine

                    var addr2 = PopFromStack();
                    var addr1 = PopFromStack();
                    PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
                    sb.Append($"      RTS");
                    break;
                case 0x61: //ADC Indirect X

                    ADC(Memory[CalcIndirectX()]);
                    break;
                case 0x62:
                    throw new NotImplementedException();
                case 0x63:
                    throw new NotImplementedException();
                case 0x64:
                    throw new NotImplementedException();
                case 0x65: //ADC Zero Page

                    ADC(Memory[PeekNextByte()]);
                    break;
                case 0x66: //ROR Zero Page

                    byte bit7;
                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[PeekNextByte()] & 1) != 0 ? true : false;
                    shiftedAccum = (byte)(Memory[PeekNextByte()] >> 1); //shift the accum right 1
                    Memory[PeekNextByte()] = (byte)(shiftedAccum | bit7);

                    PC += 2;
                    break;
                case 0x67:
                    throw new NotImplementedException();
                case 0x68: //PLA set accumulator from the stack
                    Accumulator = PopFromStack();
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    sb.Append("      PLA");
                    break;
                case 0x69: //ADC Add with carry                     
                    ADC(PeekNextByte());

                    break;
                case 0x6A: //ROR Rotate Right
                           // carry slots into bit7 and bit 0 is shifted into the carry

                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Accumulator & 1) != 0 ? true : false;
                    shiftedAccum = (byte)(Accumulator >> 1); //shift the accum right 1
                    Accumulator = (byte)(shiftedAccum | bit7);

                    PC++;
                    break;
                case 0x6B:
                    throw new NotImplementedException();
                case 0x6C: //JMP Indirect
                    var ind = SwapNextTwoBytes();
                    if ((ind & 0x00FF) != 0)
                    {
                        PC = (ushort)(ind + 1);
                    }
                    else
                    {
                        var tempInd = Memory[ind];
                        var tempInd2 = Memory[(ind + 1)];
                        var newInd = (ushort)(tempInd2 << 8 | tempInd);
                        PC = newInd;
                    }

                    break;
                case 0x6D: //ADC Absolute

                    ADC(Memory[SwapNextTwoBytes()]);
                    PC++;
                    break;
                case 0x6E: //ROR Absolute

                    if (Flags.CarryFlag) bit7 = 1;
                    else bit7 = 0;
                    bit7 = (byte)(bit7 << 7);
                    //check bit0 to see what the new carry flag chould be
                    Flags.CarryFlag = (Memory[SwapNextTwoBytes()] & 1) != 0 ? true : false;
                    shiftedAccum = (byte)(Accumulator >> 1); //shift the accum right 1
                    Memory[SwapNextTwoBytes()] = (byte)(shiftedAccum | bit7);
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0x6F:
                    throw new NotImplementedException();
                #endregion
                #region 0x7
                //0x7 14 Left
                case 0x70: //BVS Branch on overflow set

                    Branch(Flags.OverflowFlag, sb, "BVS");
                    break;
                case 0x71: // ADC Indirect Y
                    ADC(Memory[CalcIndirectY()]);
                    break;
                    throw new NotImplementedException();
                case 0x72:
                    throw new NotImplementedException();
                case 0x73:
                    throw new NotImplementedException();
                case 0x74:
                    throw new NotImplementedException();
                case 0x75:
                    throw new NotImplementedException();
                case 0x76:
                    throw new NotImplementedException();
                case 0x77:
                    throw new NotImplementedException();
                case 0x78: //SEI Set interrupt

                    Flags.InterruptDisableFlag = true;
                    sb.Append($"      SEI");
                    PC++;
                    break;
                case 0x79:
                    throw new NotImplementedException();
                case 0x7A:
                    throw new NotImplementedException();
                case 0x7B:
                    throw new NotImplementedException();
                case 0x7C:
                    throw new NotImplementedException();
                case 0x7D:
                    throw new NotImplementedException();
                case 0x7E:
                    throw new NotImplementedException();
                case 0x7F:
                    throw new NotImplementedException();
                #endregion
                #region 0x8
                //0x8 7 Left
                case 0x80:
                    throw new NotImplementedException();
                case 0x81: // STA: indirect x

                    Memory[CalcIndirectX()] = Accumulator;
                    PC += 2;
                    break;
                case 0x82:
                    throw new NotImplementedException();
                case 0x83:
                    throw new NotImplementedException();
                case 0x84: //STY Zero Page

                    Memory[PeekNextByte()] = RegisterY;
                    PC += 2;
                    break;
                case 0x85: //STA Store accum into Zero page
                    //THIS MAY BE A PROBLEM
                    pos = PeekNextByte();
                    sb.Append($"{PeekNextByte().ToString("X2")}    STA ${PeekNextByte().ToString("X2")} = {Memory[pos].ToString("X2")}");
                    Memory[pos] = Accumulator;
                    PC += 2;
                    break;
                case 0x86: //STX Store X Register STX (val) Zero Page
                    /*
                    * 
                    * 
                    * THIS IS GONNA CAUSE A PROBLEM
                    * 
                    */
                    //Load the value from register X into the specifed memory address (zero page)
                    sb.Append($"{PeekNextByte().ToString("X2")}    STX ${PeekNextByte().ToString("X2")}");
                    pos = PeekNextByte();
                    Memory[pos] = RegisterX;
                    sb.Append($" = {RegisterX.ToString("X2")}");
                    PC += 2;
                    CycleCPU(3);
                    break;
                case 0x87:
                    throw new NotImplementedException();
                case 0x88: ///DEY Decrement Register Y

                    RegisterY--;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;
                case 0x89:
                    throw new NotImplementedException();
                case 0x8A: //TXA Transfer X to Accumulator

                    Accumulator = RegisterX;
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    break;
                case 0x8B:
                    throw new NotImplementedException();
                case 0x8C: //STY Absolute
                    Memory[SwapNextTwoBytes()] = RegisterY;
                    PC += 3;
                    break;
                case 0x8D: //STA Absolute

                    var memPos = SwapNextTwoBytes();
                    Memory[memPos] = Accumulator;
                    PC += 3;
                    break;
                case 0x8E: //STX: Absolute

                    Memory[SwapNextTwoBytes()] = RegisterX;
                    PC += 3;
                    CycleCPU(3);
                    break;
                case 0x8F:
                    throw new NotImplementedException();
                #endregion
                #region 0x9
                //0x9 13 Left
                case 0x90: //BCC Branch on carry clear same as BCS excpet check for false;

                    //jump as many addresses as the next address says plus one i think
                    Branch(!Flags.CarryFlag, sb, "BCC");
                    break;
                case 0x91: //STA Indirect Y
                    Memory[CalcIndirectY()] = Accumulator;
                    PC += 2;
                    break;
                    throw new NotImplementedException();
                case 0x92:
                    throw new NotImplementedException();
                case 0x93:
                    throw new NotImplementedException();
                case 0x94:
                    throw new NotImplementedException();
                case 0x95:
                    throw new NotImplementedException();
                case 0x96:
                    throw new NotImplementedException();
                case 0x97:
                    throw new NotImplementedException();
                case 0x98: //TYA Transfer Y to Accumulator

                    Accumulator = RegisterY;
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    break;
                case 0x99:
                    throw new NotImplementedException();
                case 0x9A: //TSX Transfer register x TO stack pointer
                    StackPointer = RegisterX;
                    PC++;
                    break;
                case 0x9B:
                    throw new NotImplementedException();
                case 0x9C:
                    throw new NotImplementedException();
                case 0x9D:
                    throw new NotImplementedException();
                case 0x9E:
                    throw new NotImplementedException();
                case 0x9F:
                    throw new NotImplementedException();
                #endregion
                #region 0xA
                //0xA 5 Left
                case 0xA0: //LDY Load Y register Immediate
                    RegisterY = PeekNextByte();
                    SetZeroAndNegFlag(RegisterY);
                    PC += 2;
                    break;
                case 0xA1: //LDA: Indirect X

                    Accumulator = Memory[CalcIndirectX()];
                    PC += 2;
                    break;
                case 0xA2://LDX LoadX Register LDX (val)

                    //store the value in the next address in Register x
                    RegisterX = PeekNextByte();
                    SetZeroAndNegFlag(RegisterX);
                    PC += 2;
                    sb.Append($"{RegisterX.ToString("X2")}    LDX #${RegisterX.ToString("X2")}");
                    CycleCPU(2);
                    break;
                case 0xA3:
                    throw new NotImplementedException();
                case 0xA4: // LDY zero page

                    RegisterY = Memory[PeekNextByte()];
                    SetZeroAndNegFlag(RegisterY);
                    PC += 2;
                    break;
                case 0xA5: //LDA ZeroPage

                    pos = PeekNextByte();
                    Accumulator = Memory[pos];
                    PC += 2;
                    break;
                case 0xA6: //LDX Zero Page

                    RegisterX = Memory[PeekNextByte()];
                    SetZeroAndNegFlag(RegisterX);
                    PC += 2;
                    break;
                case 0xA7:
                    throw new NotImplementedException();
                case 0xA8: //TAY Transfer Accum into Register Y

                    RegisterY = Accumulator;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;
                case 0xA9: //LDA Load Accumulator LDA (val)

                    Accumulator = PeekNextByte();
                    PC += 2;
                    sb.Append($"{Accumulator.ToString("X2")}    LDA #${Accumulator.ToString("X2")}");
                    break;
                case 0xAA: //TAX Transfer Accum into register X

                    RegisterX = Accumulator;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;
                case 0xAB:
                    throw new NotImplementedException();
                case 0xAC: //LDY Absolute

                    RegisterY = Memory[SwapNextTwoBytes()];
                    SetZeroAndNegFlag(RegisterY);
                    PC += 3;
                    break;
                case 0xAD: //LDA: Absolute

                    Accumulator = Memory[SwapNextTwoBytes()];
                    PC += 3;
                    CycleCPU(3);
                    break;
                case 0xAE: //LDX: Absolute

                    RegisterX = Memory[SwapNextTwoBytes()];
                    PC += 3;
                    SetZeroAndNegFlag(RegisterX);
                    CycleCPU(3);
                    break;
                case 0xAF:
                    throw new NotImplementedException();
                #endregion
                #region 0xB
                //0xB 13 Left
                case 0xB0: //BCS Branch on carry set BCS (label)

                    //jump as many addresses as the next address says plus one i think
                    var jumpDistance = PeekNextByte() + 2; //get past the operand(1) jump n times (operand) and then one more so its n jumps PAST the operand and not landing on the last one
                    Branch(Flags.CarryFlag, sb, "BCS");
                    break;
                case 0xB1: //LDA Indirect Y
                    Accumulator = Memory[CalcIndirectY()];
                    SetZeroAndNegFlag(Accumulator);
                    PC += 2;
                    break;
                    throw new NotImplementedException();
                case 0xB2:
                    throw new NotImplementedException();
                case 0xB3:
                    throw new NotImplementedException();
                case 0xB4:
                    throw new NotImplementedException();
                case 0xB5:
                    throw new NotImplementedException();
                case 0xB6:
                    throw new NotImplementedException();
                case 0xB7:
                    throw new NotImplementedException();
                case 0xB8: //CLV Clear overflow 

                    Flags.OverflowFlag = false;
                    PC++;
                    break;
                case 0xB9: //LDA Absolute Y
                    {
                        var a = SwapNextTwoBytes();
                        var ba = Memory[a];
                        byte ca = (byte)(ba + RegisterY);

                        Accumulator = ca;
                        PC += 3;
                        break;
                    }
                case 0xBA: //TSX Transfer Stack Pointer to Register X

                    RegisterX = StackPointer;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;
                case 0xBB:
                    throw new NotImplementedException();
                case 0xBC:
                    throw new NotImplementedException();
                case 0xBD:
                    throw new NotImplementedException();
                case 0xBE:
                    throw new NotImplementedException();
                case 0xBF:
                    throw new NotImplementedException();
                #endregion
                #region 0xC
                //0xC 5 Left
                case 0xC0: //CPY CMP Y Immediate

                    CMP(RegisterY, PeekNextByte());
                    PC += 2;
                    break;
                case 0xC1: //CMP Indirect X

                    CMP(Accumulator, Memory[CalcIndirectX()]);
                    PC += 2;
                    break;
                case 0xC2:
                    throw new NotImplementedException();
                case 0xC3:
                    throw new NotImplementedException();
                case 0xC4: //CPY Zero Page

                    CMP(RegisterY, Memory[PeekNextByte()]);
                    PC += 2;
                    break;
                case 0xC5: //CMP ZeroPage

                    CMP(Accumulator, Memory[PeekNextByte()]);

                    PC += 2;
                    break;
                case 0xC6: //DEC Zero Page

                    Memory[PeekNextByte()]--;
                    SetZeroAndNegFlag(Memory[PeekNextByte()]);
                    PC += 2;
                    break;
                case 0xC7:
                    throw new NotImplementedException();
                case 0xC8: // INY Increment Register Y

                    RegisterY++;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;
                case 0xC9: //CMP compare operand and Accum immediate value
                    CMP(Accumulator, PeekNextByte());
                    PC += 2;
                    break;
                case 0xCA: // DEX Decrement Register X

                    RegisterX--;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;
                case 0xCB:
                    throw new NotImplementedException();
                case 0xCC: // CPY Absolute

                    CMP(RegisterY, Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0xCD: //CMP Absolute

                    CMP(Accumulator, Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0xCE: //DEC Absolute
                    Memory[SwapNextTwoBytes()]--;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0xCF:
                    throw new NotImplementedException();
                #endregion
                #region 0xD
                //0xD 14 Left
                case 0xD0: //BNE Branch on not equal Zero flag not set;

                    Branch(!Flags.ZeroFlag, sb, "BNE");
                    break;
                case 0xD1: //CMP Indirect Y
                    CMP(Accumulator, Memory[CalcIndirectY()]);
                    PC += 2;
                    break;

                case 0xD2:
                    throw new NotImplementedException();
                case 0xD3:
                    throw new NotImplementedException();
                case 0xD4:
                    throw new NotImplementedException();
                case 0xD5:
                    throw new NotImplementedException();
                case 0xD6:
                    throw new NotImplementedException();
                case 0xD7:
                    throw new NotImplementedException();
                case 0xD8: //CLD Clear Dec Flag

                    Flags.DecimalModeFlag = false;
                    PC++;
                    break;
                case 0xD9:
                    throw new NotImplementedException();
                case 0xDA:
                    throw new NotImplementedException();
                case 0xDB:
                    throw new NotImplementedException();
                case 0xDC:
                    throw new NotImplementedException();
                case 0xDD:
                    throw new NotImplementedException();
                case 0xDE:
                    throw new NotImplementedException();
                case 0xDF:
                    throw new NotImplementedException();
                #endregion
                #region 0xE
                //0xE 5 Left
                case 0xE0: //CPX CMP X Immediate

                    CMP(RegisterX, PeekNextByte());
                    PC += 2;
                    break;
                case 0xE1: // SBC Inidrect X;

                    ADC((byte)~Memory[CalcIndirectX()]);
                    break;
                case 0xE2:
                    throw new NotImplementedException();
                case 0xE3:
                    throw new NotImplementedException();
                case 0xE4: //CPX Zero Page

                    CMP(RegisterX, Memory[PeekNextByte()]);
                    PC += 2;
                    break;
                case 0xE5: //SBC Zero Page

                    ADC((byte)~Memory[PeekNextByte()]);
                    break;
                case 0xE6: //INC ZeroPage

                    Memory[PeekNextByte()]++;
                    SetZeroAndNegFlag(Memory[PeekNextByte()]);
                    PC += 2;
                    break;
                case 0xE7:
                    throw new NotImplementedException();
                case 0xE8: //INX Increment Register x

                    RegisterX++;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;
                case 0xE9: //SBC Subtract with carry                
                    //we will see if this shit works.               
                    ADC((byte)~PeekNextByte());

                    break;
                case 0xEA: //NOP

                    sb.Append("      NOP");
                    PC++;
                    CycleCPU(2);
                    break;
                case 0xEB:
                    throw new NotImplementedException();
                case 0xEC: //CPX Absolute

                    CMP(RegisterX, Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0xED: //SBC Absolute

                    ADC((byte)~Memory[SwapNextTwoBytes()]);
                    PC++;
                    break;

                case 0xEE: // INC Absolute

                    Memory[SwapNextTwoBytes()]++;
                    SetZeroAndNegFlag(Memory[SwapNextTwoBytes()]);
                    PC += 3;
                    break;
                case 0xEF:
                    throw new NotImplementedException();
                #endregion
                #region 0xF
                //0xF 14 Left
                case 0xF0: //BEQ Branch On Equal BEQ (value)

                    Branch(Flags.ZeroFlag, sb, "BEQ");
                    break;
                case 0xF1: //SBC Indirect Y
                    ADC((byte)~Memory[CalcIndirectY()]);
                    break;


                case 0xF2:
                    throw new NotImplementedException();
                case 0xF3:
                    throw new NotImplementedException();
                case 0xF4:
                    throw new NotImplementedException();
                case 0xF5:
                    throw new NotImplementedException();
                case 0xF6:
                    throw new NotImplementedException();
                case 0xF7:
                    throw new NotImplementedException();
                case 0xF8: //SED Set Decimal

                    Flags.DecimalModeFlag = true;
                    sb.Append($"      SED");
                    PC++;
                    break;
                case 0xF9:
                    throw new NotImplementedException();
                case 0xFA:
                    throw new NotImplementedException();
                case 0xFB:
                    throw new NotImplementedException();
                case 0xFC:
                    throw new NotImplementedException();
                case 0xFD:
                    throw new NotImplementedException();
                case 0xFE:
                    throw new NotImplementedException();
                case 0xFF:
                    throw new NotImplementedException();
                #endregion
                #region Default
                default:
                    Trace.WriteLine($"Unknown OPCode: {Memory[PC].ToString("X2")} Mem Location: {PC.ToString("X2")}");
                    Trace.WriteLine("Execution Halted...");
                    IsNewOP = true;
                    break;
                    #endregion
            }

            if (prevAccum != Accumulator) AccumChanged();
            Trace.WriteLine(sb);
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
        private void ADC(byte op)
        {
            //this is out here for now se we can use it for both adding and subtraction
            // As The Prodigy once said: This is dangerous.
            //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
            var carry = Flags.CarryFlag ? 1 : 0; //is the carry flag set
            var sum = Accumulator + op + carry; //sum the Accum+operand+carry(if set)
            Flags.CarryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
            Flags.OverflowFlag = (~(Accumulator ^ op) & (Accumulator ^ sum) & 0x80) != 0 ? true : false;
            Accumulator = (byte)sum;
            PC += 2;
        }
        private byte PeekNextByte()
        {
            return Memory[PC + 1];
        }
        private byte PeekByteAfterNext()
        {
            return Memory[PC + 2];
        }
        private void SetZeroAndNegFlag(byte value)
        {
            if ((value & 128) != 0)
                Flags.NegativeFlag = true;
            else Flags.NegativeFlag = false;
            if (value != 0x00)
                Flags.ZeroFlag = false;
            else Flags.ZeroFlag = true;

        }
        private StringBuilder Branch(bool DoBranch, StringBuilder sb, string opcodeSTR)
        {
            var jumpDistance = PeekNextByte() + 2;

            if (DoBranch)
            {
                sb.Append($"{PeekNextByte().ToString("X2")}    {opcodeSTR} ${(PC + jumpDistance).ToString("X2")}");
                PC += (ushort)jumpDistance;

            }
            else
            {
                sb.Append($"{PeekNextByte().ToString("X2")}    {opcodeSTR} ${(PC + jumpDistance).ToString("X2")}");
                PC += 2;
            }
            return sb;
        }
        private void AccumChanged()
        {
            if (Accumulator != 0x00)
                Flags.ZeroFlag = false;
            else Flags.ZeroFlag = true;
            if ((Accumulator & 128) != 0)
            {
                Flags.NegativeFlag = true;
            }
            else Flags.NegativeFlag = false;

        }
        private ushort SwapNextTwoBytes()
        {
            var addr1 = PeekNextByte();
            var addr2 = PeekByteAfterNext();
            return (ushort)(addr2 << 8 | addr1);
        }
        private void CycleCPU(int cycles)
        {
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

        #region PPU Operations

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
