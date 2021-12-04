using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    internal class CPU
    {
        public byte[] Memory = new byte[0x10000];
        //Memory Map 
        /* 0x0000 - 0x00FF
         * 0x0100 - 0x01FF STACK
         * 0x0200 - 0x07FF
         * 0x0800 - 0x1FFF
         * 0x2000 - 0x2007
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
        public ProcessorStatus Flags = new ProcessorStatus();

        public ulong CPUCycle = 0;
        public CPU()
        {
            //Startup routine
            //can probably make this more accurate
            CPUCycle = 4; //Why is this?
            Flags.InterruptDisableFlag = false;
            Flags.NegativeFlag = false; //test
            
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

        public void SearchForOpcode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{PC.ToString("X2")}  {Memory[PC].ToString("X2")} ");
            var prevAccum = Accumulator;

            switch (Memory[PC])
            {
                case 0x4C: //jmp - Jump (addr1 addr2)
                           
                    //Move the PC to a specific address and skip the next to addresses       
                    //skip because they are the location to jump too
                    sb.Append($"{PeekNextByte().ToString("X2")} {PeekByteAfterNext().ToString("X2")}");
                    PC = SwapNextTwoBytes();
                    sb.Append($" JMP ${PC.ToString("X2")}");
                    CycleCPU(3);
                    break;

                case 0xA2://LDX LoadX Register LDX (val)
                    
                    //store the value in the next address in Register x
                    RegisterX = PeekNextByte();
                    SetZeroAndNegFlag(RegisterX);
                    PC += 2;
                    sb.Append($"{RegisterX.ToString("X2")}    LDX #${RegisterX.ToString("X2")}");
                    CycleCPU(2);
                    break;

                /*
                 * 
                 * 
                 * THIS IS GONNA CAUSE A PROBLEM
                 * 
                 */
                case 0x86: //STX Store X Register STX (val) Zero Page
                    
                    //Load the value from register X into the specifed memory address (zero page)
                    sb.Append($"{PeekNextByte().ToString("X2")}    STX ${PeekNextByte().ToString("X2")}");
                    var pos = PeekNextByte();
                    Memory[pos] = RegisterX;
                    sb.Append($" = {RegisterX.ToString("X2")}");
                    PC += 2;
                    CycleCPU(3);
                    break;

                case 0x20: //JSR Jump SubRoutine JSR (addr addr)
                           
                    //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address
                    var b = BitConverter.GetBytes((ushort)PC + 2);
                    PushToStack(b[1]);
                    PushToStack(b[0]);
                    PC = SwapNextTwoBytes();
                    sb.Append($"{PeekNextByte().ToString("X2")} {PeekByteAfterNext().ToString("X2")} JSR ${PC.ToString("X2")}");
                    CycleCPU(6);
                    break;

                case 0xEA: //NOP
                    
                    sb.Append("      NOP");
                    PC++;
                    CycleCPU(2);
                    break;

                case 0x38: //SEC Set Carry Flag
                    
                    sb.Append("      SEC");
                    Flags.CarryFlag = true;
                    PC++;
                    CycleCPU(2);
                    break;

                case 0xB0: //BCS Branch on carry set BCS (label)
                    
                    //jump as many addresses as the next address says plus one i think
                    var jumpDistance = PeekNextByte() + 2; //get past the operand(1) jump n times (operand) and then one more so its n jumps PAST the operand and not landing on the last one
                    Branch(Flags.CarryFlag, sb, "BCS");
                    break;

                case 0x18: //CEC clear carry

                    sb.Append("      CLC");
                    Flags.CarryFlag = false;
                    PC++;
                    break;

                case 0x90: //BCC Branch on carry clear same as BCS excpet check for false;
                    
                    //jump as many addresses as the next address says plus one i think
                    Branch(!Flags.CarryFlag, sb, "BCC");
                    break;

                case 0xA9: //LDA Load Accumulator LDA (val)

                    Accumulator = PeekNextByte();
                    PC += 2;
                    sb.Append($"{Accumulator.ToString("X2")}    LDA #${Accumulator.ToString("X2")}");
                    break;


                case 0xF0: //BEQ Branch On Equal BEQ (value)

                    Branch(Flags.ZeroFlag, sb, "BEQ");
                    break;

                case 0xD0: //BNE Branch on not equal Zero flag not set;

                    Branch(!Flags.ZeroFlag, sb, "BNE");
                    break;

                //THIS MAY BE A PROBLEM
                case 0x85: //STA Store accum into Zero page

                    pos = PeekNextByte();
                    sb.Append($"{PeekNextByte().ToString("X2")}    STA ${PeekNextByte().ToString("X2")} = {Memory[pos].ToString("X2")}");
                    Memory[pos] = Accumulator;
                    PC += 2;
                    break;

                case 0x24: //BIT 

                    //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
                    //value store iat the tested address
                    pos = PeekNextByte();
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


                case 0x70: //BVS Branch on overflow set

                    Branch(Flags.OverflowFlag, sb, "BVS");
                    break;

                case 0x50: //BVC Branch on OVerflow Clear

                    Branch(!Flags.OverflowFlag, sb, "BVC");
                    break;

                case 0x10: //BPL Branch on plus

                    Branch(!Flags.NegativeFlag, sb, "BPL");
                    break;

                case 0x60: //RTS Return from SubRoutine

                    var addr2 = PopFromStack();
                    var addr1 = PopFromStack();
                    PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
                    sb.Append($"      RTS");
                    break;

                case 0x78: //SEI Set interrupt

                    Flags.InterruptDisableFlag = true;
                    sb.Append($"      SEI");
                    PC++;
                    break;

                case 0xF8: //SED Set Decimal

                    Flags.DecimalModeFlag = true;
                    sb.Append($"      SED");
                    PC++;
                    break;

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

                case 0x68: //PLA set accumulator from the stack
                    Accumulator = PopFromStack();
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    sb.Append("      PLA");
                    break;

                //FRom Here, the strings are simpler, since I plan on refactoring them anyway
                case 0x29: //AND Bitwise and with operand and accum
                    var aa = PeekNextByte();
                    var bb = Accumulator;
                    byte c = (byte)(aa & bb);
                    Accumulator = (byte)c;
                    SetZeroAndNegFlag(Accumulator);
                    sb.Append("      AND");
                    PC += 2;
                    break;

                case 0xC9: //CMP compare operand and Accum immediate value
                    CMP(Accumulator, PeekNextByte());
                    PC += 2;
                    break;

                case 0xD8: //CLD Clear Dec Flag

                    Flags.DecimalModeFlag = false;
                    PC++;
                    break;

                case 0x048: //PHA push Accum to stack

                    PushToStack(Accumulator);
                    PC++;
                    break;

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

                case 0x30: //BMI Branch on minus
                    Branch(Flags.NegativeFlag, sb, "BMI");
                    break;

                case 0x09: //ORA or the accum and operand

                    Accumulator |= PeekNextByte();
                    
                    PC += 2;
                    break;

                case 0xB8: //CLV Clear overflow 

                    Flags.OverflowFlag = false;
                    PC++;
                    break;

                case 0x49: //EOR Exlusive OR the accum

                    Accumulator ^= PeekNextByte();
                    
                    PC += 2;
                    break;

/////////////////////////////////////////////////////////////////////
                case 0x69: //ADC Add with carry                     // QUARANTINE ZONE
                    ADC(PeekNextByte());                            // DON'T OPEN
                                                                    // DEAD INSIDE
                    break;                                          //
                                                                    //
                case 0xE9: //SBC Subtract with carry                //
                    //we will see if this shit works.               //
                    ADC((byte)~PeekNextByte());                     //
                                                                    //
                    break;                                          //
/////////////////////////////////////////////////////////////////////              

                case 0xA0: //LDY Load Y register Immediate
                    RegisterY = PeekNextByte();
                    SetZeroAndNegFlag(RegisterY);
                    PC += 2;
                    break;

                case 0xC0: //CPY CMP Y Immediate

                    CMP(RegisterY, PeekNextByte());
                    PC += 2;
                    break;

                case 0xE0: //CPX CMP X Immediate

                    CMP(RegisterX, PeekNextByte());
                    PC += 2;
                    break;

                case 0xC8: // INY Increment Register Y

                    RegisterY++;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;

                case 0xE8: //INX Increment Register x

                    RegisterX++;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;

                case 0x88: ///DEY Decrement Register Y

                    RegisterY--;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;

                case 0xCA: // DEX Decrement Register X

                    RegisterX--;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;

                case 0xA8: //TAY Transfer Accum into Register Y

                    RegisterY = Accumulator;
                    SetZeroAndNegFlag(RegisterY);
                    PC++;
                    break;

                case 0xAA: //TAX Transfer Accum into register X

                    RegisterX = Accumulator;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;

                case 0x98: //TYA Transfer Y to Accumulator

                    Accumulator = RegisterY;
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    break;

                case 0x8A: //TXA Transfer X to Accumulator

                    Accumulator = RegisterX;
                    SetZeroAndNegFlag(Accumulator);
                    PC++;
                    break;

                case 0xBA: //TSX Transfer Stack Pointer to Register X

                    RegisterX = StackPointer;
                    SetZeroAndNegFlag(RegisterX);
                    PC++;
                    break;

                case 0x8E: //STX: Absolute

                    Memory[SwapNextTwoBytes()] = RegisterX;
                    PC += 3;
                    CycleCPU(3);
                    break;

                case 0x9A: //TSX Transfer register x TO stack pointer
                    StackPointer = RegisterX;
                    PC++;
                    break;


                case 0xAE: //LDX: Absolute

                    RegisterX = Memory[SwapNextTwoBytes()];
                    PC += 3;
                    CycleCPU(3);
                    break;

                case 0xAD: //LDA: Absolute

                    Accumulator = Memory[SwapNextTwoBytes()];
                    PC += 3;
                    CycleCPU(3);
                    break;


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

                case 0x0A: //ASL Shift Accum Left, similar to LSR
                    if ((Accumulator & 128) != 0)
                        Flags.CarryFlag = true;
                    else
                        Flags.CarryFlag = false;
                    Accumulator = (byte)(Accumulator << 1);
                    PC++;
                    break;

                default:
                    Console.WriteLine($"Unknown OPCode: {Memory[PC].ToString("X2")} Mem Location: {PC.ToString("X2")}");
                    Console.WriteLine("Execution Halted...");
                    Console.Read();
                    break;
            }

            if (prevAccum != Accumulator) AccumChanged();
            Console.WriteLine(sb);
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
        private void ADC(byte op) {
            //this is out here for now se we can use it for both adding and subtraction
            // As The Prodigy once said: This is dangerous.
                    //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
                    var carry = Flags.CarryFlag ? 1 : 0; //is the carry flag set
                    var sum = Accumulator + op + carry; //sum the Accum+operand+carry(if set)
                    Flags.CarryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
                    Flags.OverflowFlag = (~(Accumulator ^ op) & (Accumulator ^ sum) & 0x80) != 0 ? true :false;
                    Accumulator = (byte)sum;
                    PC += 2;
        }
        private byte PeekNextByte() {
            return Memory[PC + 1];
        }
        private byte PeekByteAfterNext(){
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

    }
    struct ProcessorStatus
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
