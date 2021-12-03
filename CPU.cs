using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    internal class CPU
    {
        byte[] Memory = new byte[0x10000];
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
        byte StackPointer = 0xFF;
        public ushort PC = 0xC000; //skip the header for now
        public byte Accumulator = 0x00;
        public byte RegisterY = 0x00;
        public byte RegisterX = 0x00;
        public ProcessorStatus Flags = new ProcessorStatus();

        public ulong CPUCycle = 0;
        public CPU()
        {
            CPUCycle = 4; //Why is this?
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
            sb.Append($"Accum: {Accumulator.ToString("X2")} | {PC.ToString("X2")}  {Memory[PC].ToString("X2")} ");

            CheckFlagStatus(); //check flag status since last op
            switch (Memory[PC])
            {
                case 0x4C: //jmp - Jump (addr1 addr2)
                           //Move the PC to a specific address and skip the next to addresses
                           //skip because they are the location to jump too


                    sb.Append($"{Memory[PC + 1].ToString("X2")} {Memory[PC + 2].ToString("X2")}");
                    PC = SwapNextTwoBytes();
                    sb.Append($" JMP ${PC.ToString("X2")}       ");
                    CycleCPU(3);
                    break;

                case 0xA2://LDX LoadX Register LDX (val)
                    //store the value in the next address in Register x
                    RegisterX = Memory[PC + 1];


                    PC += 2;
                    sb.Append($"{RegisterX.ToString("X2")}    LDX #${RegisterX.ToString("X2")}        ");
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
                    sb.Append($"{Memory[PC + 1].ToString("X2")}    STX ${Memory[PC + 1].ToString("X2")}");

                    var pos = Memory[PC + 1];
                    Memory[pos] = RegisterX;
                    sb.Append($" = {RegisterX.ToString("X2")}    ");

                    PC += 2;
                    CycleCPU(3);
                    break;

                case 0x20: //JSR Jump SubRoutine JSR (addr addr)
                           //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address


                    var b = BitConverter.GetBytes((ushort)PC + 2);

                    PushToStack(b[1]);
                    PushToStack(b[0]);
                    PC = SwapNextTwoBytes();
                    sb.Append($"{Memory[PC + 1].ToString("X2")} {Memory[PC + 2].ToString("X2")} JSR ${PC.ToString("X2")}       ");
                    CycleCPU(6);
                    break;

                case 0xEA: //NOP
                    sb.Append("      NOP             ");
                    PC++;
                    CycleCPU(2);
                    break;

                case 0x38: //SEC Set Carry Flag
                    sb.Append("      SEC             ");
                    Flags.CarryFlag = true;
                    PC++;
                    CycleCPU(2);
                    break;

                case 0xB0: //BCS Branch on carry set BCS (label)
                    //jump as many addresses as the next address says plus one i think
                    var jumpDistance = Memory[PC + 1] + 2; //get past the operand(1) jump n times (operand) and then one more so its n jumps PAST the operand and not landing on the last one
                    Branch(Flags.CarryFlag, sb, "BCS");
                    break;

                case 0x18: //CEC clear carry
                    sb.Append("      CLC             ");
                    Flags.CarryFlag = false;
                    PC++;
                    break;

                case 0x90: //BCC Branch on carry clear same as BCS excpet check for false;
                    //jump as many addresses as the next address says plus one i think
                    Branch(!Flags.CarryFlag, sb, "BCC");
                    break;

                case 0xA9: //LDA Load Accumulator LDA (val)

                    Accumulator = Memory[PC + 1];
                    PC += 2;
                    if ((Accumulator & 128) != 0)
                    {
                        Flags.NegativeFlag = true;
                    }
                    sb.Append($"{Accumulator.ToString("X2")}    LDA #${Accumulator.ToString("X2")}        ");
                    break;


                case 0xF0: //BEQ Branch On Equal BEQ (value)
                    Branch(Flags.ZeroFlag, sb, "BEQ");
                    break;

                case 0xD0: //BNE Branch on not equal Zero flag not set;
                    Branch(!Flags.ZeroFlag, sb, "BNE");
                    break;

                //THIS MAY BE A PROBLEM
                case 0x85: //STA Store accum into Zero page
                    pos = Memory[PC + 1];
                    var before = Memory[pos];
                    Memory[pos] = Accumulator;
                    var after = Memory[pos];
                    sb.Append($"{Memory[PC + 1].ToString("X2")}    STA ${Memory[PC + 1].ToString("X2")} = {Memory[pos].ToString("X2")}  ");

                    PC += 2;
                    break;

                case 0x24: //BIT 
                    //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
                    //value store iat the tested address
                    pos = Memory[PC + 1];


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



                    sb.Append($"{Memory[PC + 1].ToString("X2")}    BIT ${Memory[PC + 1].ToString("X2")} = {Accumulator.ToString("X2")}  ");
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
                    sb.Append($"      RTS             ");
                    break;

                case 0x78: //SEI Set interrupt
                    Flags.InterruptDisableFlag = true;
                    sb.Append($"      SEI             ");
                    PC++;
                    break;

                case 0xF8: //SED Set Decimal
                    Flags.DecimalModeFlag = true;
                    sb.Append($"      SED             ");
                    PC ++;
                    break;

                case 0x08: //PHP Push Processor Status

                    var b0 = Flags.CarryFlag;
                    var b1 = Flags.ZeroFlag;
                    var b2 = Flags.InterruptDisableFlag;
                    var b3 = Flags.DecimalModeFlag;
                    var b4 = Flags.BreakCommandFlag;
                    var b5 = Flags.nullFlag;
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
                    break;


                default:

                    Console.WriteLine($"Unknown OPCode: {Memory[PC].ToString("X2")} Mem Location: {PC.ToString("X2")}");
                    Console.WriteLine("Execution Halted...");
                    Console.ReadKey();
                    break;
            }

            sb.Append($"  Cycles: {CPUCycle}");
            Console.WriteLine(sb);


        }


        private StringBuilder Branch(bool DoBranch, StringBuilder sb, string opcodeSTR)
        {
            var jumpDistance = Memory[PC + 1] + 2;

            if (DoBranch)
            {
                sb.Append($"{Memory[PC + 1].ToString("X2")}    {opcodeSTR} ${(PC + jumpDistance).ToString("X2")}       ");
                PC += (ushort)jumpDistance;

            }
            else
            {
                sb.Append($"{Memory[PC + 1].ToString("X2")}    {opcodeSTR} ${(PC + jumpDistance).ToString("X2")}       ");
                PC += 2;
            }
            return sb;
        }
        private void CheckFlagStatus()
        {
            if (Accumulator != 0x00)
                Flags.ZeroFlag = false;
            else Flags.ZeroFlag = true;

        }

        private ushort SwapNextTwoBytes()
        {
            var addr1 = Memory[PC + 1];
            var addr2 = Memory[PC + 2];
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
