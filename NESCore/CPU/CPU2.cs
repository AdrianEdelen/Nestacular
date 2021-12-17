using Nestacular.NES2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore
{
    public class CPU2
    {
//         /* Names of variables match the documented names of the components
//          * this makes it easier to compare against established Documentation */

//         /* The CPU reads and writes to the bus to communicate with the rest of the NES, 
//          * it should also function as a regular 6502 just without decimal mode
//          * as the NES RICOH 2A0C did not have decimal mode enabled 
         
         
// On reset, the processor will read address $FFFC and $FFFD (called the reset vector) and load the program counter (PC) with their content.
// For example, if $FFFC = $00 and $FFFD = $10, then the PC will get loaded with $1000 and execution will start there. However, most 6502 systems contain ROM in the upper address region, say $E000-$FFFF so it's likely that the reset vector will point to a location there.
// Most systems have an OS of some sorts - ranging from a simple machine language monitor, BASIC interpreter, even GUI interfaces such as Contiki.
// Your OS must have a method of loading the programs generated from an assembler or compiler into RAM. It must also have a method of executing code in RAM.
// For simplicity, lets say you have a simple command line promt and you can load a program using the "LOAD Example.obj, $1000" command.
// This will load the program named Example.obj into RAM at address $1000.
// Next, from the command prompt, you would type "Exec $1000" which would move the address $1000 into the PC register and begin executing your program.
// You must have some sort of OS capable of doing these two steps in order to load and execute programs.

         
//          */

//         //bus ref
//         BUS _bus;
//         #region Registers
//         private ushort PC = 0x0000; //Program Counter
//         private byte SP = 0xFD; //Stack Pointer
//         private byte A = 0x00; //Accumulator
//         private byte X = 0x00; //X Register
//         private byte Y = 0x00; //Y Register
//         private byte P;        //Status Byte
//         #endregion
//         #region Flags
//         private bool C = false; //Carry Flag
//         private bool Z = false; //Zero Flag
//         private bool I = false; //Interrupt Disable Flag
//         private bool D = false; //Decimal Mode Flag
//         private bool B = false; //Break Command Flag
//         private bool V = false; //Overflow Flag
//         private bool N = false; //Negative Flag
//         #endregion
//         //Internal Flags and helper variables
//         //These do not have real world equivalents.
//         private bool _isHalted = false;
//         private UInt64 internalClock = 0;
//         private List<INSTR> _opCodes = new List<INSTR>();

//         public CPU2(BUS bus)
//         {
//             #region Some Notes on WTF is happening here
//             /*
//                 Some Explanation:
//                 Instructions with odd timings:
//                 0x11;

//                 M is a method to create a new INSTR object, this is just to squeeze the size of the table a little smaller it is like a psuedo factory
//                 we are assigning delegates for the opcodes and addressing modes for each of the possible 256 instructions available on the cpu
//                 this is basically a giant list of object initialization formatted in a table to make finding the an opcode actually easier believe it or not.

//                 to search the table as a human, just look at the strings and how they match up to the x and y and that is your byte for the opcode
//                 Additionally you can ctrl+F and search for a 3 character opcode mnemonic e.g. ADC and see everywhere in the table that opcode is referenced.
//                 to search the table as a computer, just index based off of the read byte in memory, the table is formatted so that the index lines up with the hex opcode value
//                 eg opcode 0x00 == _opcodes[0] and opcode 0xFF == _opcodes[255]


//                 There are 256 total opcodes, some are legal some are not. a lot of them just halt the CPU, but hey, if the programmer wants to halt the cpu she can.
//             */
//             #endregion
//             _bus = bus;
//             //Beware those who enter for this is a beast
//             static INSTR M(string name, Action<byte> op, Func<byte> addr, int cycles) { return new INSTR(name, op, addr, cycles); }
//             _opCodes.AddRange(new List<INSTR>(){
// //    00                 01                 02                 03                 04                 05                 06                 07                 08                 09                 0A                 0B                 0C                 0D                 0E                 0F            
// /*00*/M("BRK",BRK,IMP,7),M("ORA",ORA,XIN,6),M("JAM",JAM,IMP,2),M("SLO",SLO,XIN,8),M("NOP",NOP,ZPG,3),M("ORA",ORA,ZPG,3),M("ASL",ASL,ZPG,5),M("SLO",SLO,ZPG,5),M("PHP",PHP,IMP,3),M("ORA",ORA,IMM,2),M("ASL",ASL,IMP,2),M("ANC",ANC,IMM,2),M("NOP",NOP,ABS,4),M("ORA",ORA,ABS,4),M("ASL",ASL,ABS,6),M("SLO",SLO,ABS,6),
// /*10*/M("BPL",BPL,REL,2),M("ORA",ORA,YIN,5),M("JAM",JAM,IMP,2),M("SLO",SLO,YIN,4),M("NOP",NOP,XZP,7),M("ORA",ORA,XZP,7),M("ASL",ASL,XZP,7),M("SLO",SLO,XZP,7),M("CLC",CLC,IMP,7),M("ORA",ORA,YAB,7),M("NOP",NOP,IMP,7),M("SLO",SLO,YAB,7),M("NOP",NOP,XAB,7),M("ORA",ORA,XAB,7),M("ASL",ASL,XAB,7),M("SLO",SLO,XAB,7),
// /*20*/M("JSR",JSR,ABS,7),M("AND",AND,XIN,7),M("JAM",JAM,IMP,7),M("RLA",RLA,XIN,7),M("BIT",BIT,ZPG,7),M("AND",AND,ZPG,7),M("ROL",ROL,ZPG,7),M("RLA",RLA,ZPG,7),M("PLP",PLP,IMP,7),M("AND",AND,IMM,7),M("ROL",ROL,IMP,7),M("ANC",ANC,IMM,7),M("BIT",BIT,ABS,7),M("AND",AND,ABS,7),M("ROL",ROL,ABS,7),M("RLA",RLA,ABS,7),
// /*30*/M("BMI",BMI,IMP,7),M("AND",AND,YIN,7),M("JAM",JAM,IMP,7),M("RLA",RLA,YIN,7),M("NOP",NOP,XZP,7),M("AND",AND,XZP,7),M("ROL",ROL,XZP,7),M("RLA",RLA,XZP,7),M("SEC",SEC,IMP,7),M("AND",AND,YAB,7),M("NOP",NOP,IMP,7),M("RLA",RLA,YAB,7),M("NOP",NOP,XAB,7),M("AND",AND,XAB,7),M("ROL",ROL,XAB,7),M("RLA",RLA,XAB,7),
// /*40*/M("RTI",RTI,IMP,7),M("EOR",EOR,XIN,7),M("JAM",JAM,IMP,7),M("SRE",SRE,XIN,7),M("NOP",NOP,ZPG,7),M("EOR",EOR,ZPG,7),M("LSR",LSR,ZPG,7),M("SRE",SRE,ZPG,7),M("PHA",PHA,IMP,7),M("EOR",EOR,IMM,7),M("LSR",LSR,IMP,7),M("ALR",ALR,IMM,7),M("JMP",JMP,IMP,7),M("EOR",EOR,ABS,7),M("LSR",LSR,ABS,7),M("SRE",SRE,ABS,7),
// /*50*/M("BVC",BVC,IMP,7),M("EOR",EOR,YIN,7),M("JAM",JAM,IMP,7),M("SRE",SRE,YIN,7),M("NOP",NOP,XZP,7),M("EOR",EOR,XZP,7),M("LSR",LSR,XZP,7),M("SRE",SRE,XZP,7),M("CLI",CLI,IMP,7),M("EOR",EOR,YAB,7),M("NOP",NOP,IMP,7),M("SRE",SRE,YAB,7),M("NOP",NOP,XAB,7),M("EOR",EOR,XAB,7),M("LSR",LSR,XAB,7),M("SRE",SRE,XAB,7),
// /*60*/M("RTS",RTS,IMP,7),M("ADC",ADC,XIN,7),M("JAM",JAM,IMP,7),M("RRA",RRA,XIN,7),M("NOP",NOP,ZPG,7),M("ADC",ADC,ZPG,7),M("ROR",ROR,ZPG,7),M("RRA",RRA,ZPG,7),M("PLA",PLA,IMP,7),M("ADC",ADC,IMM,7),M("ROR",ROR,IMP,7),M("ARR",ARR,IMM,7),M("JMP",JMP,IND,7),M("ADC",ADC,ABS,7),M("ROR",ROR,ABS,7),M("RRA",RRA,ABS,7),
// /*70*/M("BVS",BVS,IMP,7),M("ADC",ADC,YIN,7),M("JAM",JAM,IMP,7),M("RRA",RRA,YIN,7),M("NOP",NOP,XZP,7),M("ADC",ADC,XZP,7),M("ROR",ROR,XZP,7),M("RRA",RRA,XZP,7),M("SEI",SEI,IMP,7),M("ADC",ADC,YAB,7),M("NOP",NOP,IMP,7),M("RRA",RRA,YAB,7),M("NOP",NOP,XAB,7),M("ADC",ADC,XAB,7),M("ROR",ROR,XAB,7),M("RRA",RRA,XAB,7),
// /*80*/M("NOP",NOP,IMM,7),M("STA",STA,XIN,7),M("NOP",NOP,IMM,7),M("SAX",SAX,XIN,7),M("STY",STY,ZPG,7),M("STA",STA,ZPG,7),M("STX",STX,ZPG,7),M("SAX",SAX,ZPG,7),M("DEY",DEY,IMP,7),M("NOP",NOP,IMM,7),M("TXA",TXA,IMP,7),M("ANE",ANE,IMM,7),M("STY",STY,ABS,7),M("STA",STA,ABS,7),M("STX",STX,ABS,7),M("SAX",SAX,ABS,7),
// /*90*/M("BCC",BCC,IMP,7),M("STA",STA,YIN,7),M("JAM",JAM,IMP,7),M("SHA",SHA,YIN,7),M("STY",STY,XZP,7),M("STA",STA,XZP,7),M("STX",STX,YZP,7),M("SAX",SAX,YZP,7),M("TYA",TYA,IMP,7),M("STA",STA,YAB,7),M("TXS",TXS,IMP,7),M("TAS",TAS,YAB,7),M("SHY",SHY,XAB,7),M("STA",STA,XAB,7),M("SHX",SHX,YAB,7),M("SHY",SHY,YAB,7),
// /*A0*/M("LDY",LDY,IMM,7),M("LDA",LDA,XIN,7),M("LDX",LDX,IMM,7),M("LAX",LAX,XIN,7),M("LDY",LDY,ZPG,7),M("LDA",LDA,ZPG,7),M("LDX",LDX,ZPG,7),M("LAX",LAX,ZPG,7),M("TAY",TAY,IMP,7),M("LDA",LDA,IMM,7),M("TAX",TAX,IMP,7),M("LXA",LXA,IMM,7),M("LDY",LDY,ABS,7),M("LDA",LDA,ABS,7),M("LDX",LDX,ABS,7),M("LAX",LAX,ABS,7),
// /*B0*/M("BCS",BCS,IMP,7),M("LDA",LDA,YIN,7),M("JAM",JAM,IMP,7),M("LAX",LAX,YIN,7),M("LDY",LDY,XZP,7),M("LDA",LDA,XZP,7),M("LDX",LDX,YZP,7),M("LAX",LAX,YZP,7),M("CLV",CLV,IMP,7),M("LDA",LDA,YAB,7),M("TSX",TSX,IMP,7),M("LAS",LAS,YAB,7),M("LDY",LDY,XAB,7),M("LDA",LDA,XAB,7),M("LDX",LDX,YAB,7),M("LAX",LAX,YAB,7),
// /*C0*/M("CPY",CPY,IMM,7),M("CMP",CMP,XIN,7),M("NOP",NOP,IMM,7),M("DCP",DCP,XIN,7),M("CPY",CPY,ZPG,7),M("CMP",CMP,ZPG,7),M("DEC",DEC,ZPG,7),M("DCP",DCP,ZPG,7),M("INY",INY,IMP,7),M("CMP",CMP,IMM,7),M("DEX",DEX,IMP,7),M("SBX",SBX,IMM,7),M("CPY",CPY,ABS,7),M("CMP",CMP,ABS,7),M("DEC",DEC,ABS,7),M("DCP",DCP,ABS,7),
// /*D0*/M("BNE",BNE,IMP,7),M("CMP",CMP,YIN,7),M("JAM",JAM,IMP,7),M("DCP",DCP,YIN,7),M("NOP",NOP,XZP,7),M("CMP",CMP,XZP,7),M("DEC",DEC,XZP,7),M("DCP",DCP,XZP,7),M("CLD",CLD,IMP,7),M("CMP",CMP,YAB,7),M("NOP",NOP,IMP,7),M("DCP",DCP,YAB,7),M("NOP",NOP,XAB,7),M("CMP",CMP,XAB,7),M("DEC",DEC,XAB,7),M("DCP",DCP,XAB,7),
// /*E0*/M("CPX",CPX,IMM,7),M("SBC",SBC,XIN,7),M("NOP",NOP,IMM,7),M("ISC",ISC,XIN,7),M("CMP",CMP,ZPG,7),M("SBC",SBC,ZPG,7),M("INC",INC,ZPG,7),M("ISC",ISC,ZPG,7),M("INX",INX,IMP,7),M("SBC",SBC,IMM,7),M("NOP",NOP,IMP,7),M("USB",USB,IMM,7),M("CPX",CPX,ABS,7),M("SBC",SBC,ABS,7),M("INC",INC,ABS,7),M("ISC",ISC,ABS,7),
// /*F0*/M("BEQ",BEQ,IMP,7),M("SBC",SBC,YIN,7),M("JAM",JAM,IMP,7),M("ISC",ISC,YIN,7),M("NOP",NOP,XZP,7),M("SBC",SBC,XZP,7),M("INC",INC,XZP,7),M("ISC",ISC,XZP,7),M("SED",SED,IMP,7),M("SBC",SBC,YAB,7),M("NOP",NOP,IMP,7),M("ISC",ISC,YAB,7),M("NOP",NOP,XAB,7),M("SBC",SBC,XAB,7),M("INC",INC,XAB,7),M("ISC",ISC,XAB,7)
//             });
//         }

//         //this should be the only interface to this class. 
//         //since so much is happening here, i believe it to be critical to limit
//         //outside influence of the state (plus that's how it really works in real life)
//         public void StepTo(ulong masterClockCycles)
//         {
//             //check internal clock against master clock,
//             //if the cpu is behind, go ahead and execute.
//             //otherwise halt until master clock catches up
//             //since the cpu should be MUCH MUCH faster than the master clock
//             //we should basically be halted most of the time,
//             //if we have enough free time in between cycles, 
//             //then we will spit out the dissasembler info also
//             while (masterClockCycles > internalClock)
//             {
                
//                 //advance the cpu clock as long as the master clock is ahead.
//                 Clock();
//             }
//         }
//         #region CPU built in routines e.g. startup/shutdown
//         private byte _opCode;
//         public void Clock()
//         {
//             _opCode = _bus.Read(PC); //get the byte of memory at the address of the PC
//             _opCodes[_opCode].Execute(); //Actually Execute the op
//         }

//         private void Startup()
//         {

//         }

//         private void Shutdown()
//         {

//         }
//         private void Reset()
//         {

//         }
//         #endregion
//         #region OpCodes
//         void JAM(byte op)
//         {
//             while (true) { }
//         }
//         void ADC(byte op)
//         {
//             // As The Prodigy once said: This is dangerous.
//             //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
//             var carry = C ? 1 : 0; //is the carry flag set
//             var sum = A + op + carry; //sum the Accum+operand+carry(if set)
//             C = sum > 0xFF ? true : false; //set/clear the carry based on the result.
//             V = (~(A ^ op) & (A ^ sum) & 0x80) != 0 ? true : false;
//             A = (byte)sum;



//         }
//         void AND(byte op)
//         {
//             A = (byte)(A & op);
//             SetZeroAndNegFlag(A);
//         }
//         void ASL(byte op)
//         {
//             //the byte that is passed is the calculated value from the position of the next byte/s in memory 
//             //e.g. PeekNextByte(PC).. The problem is we have moved the PC already in the address mode method, so now we cannot
//             //be sure where the actual POSITION that this value needs to be stored in.
//             //we are calculating the correct value, we just lost where it belongs.

//             //I think the only solution is for ASL to have a second argument.
//             //I hate this since it breaks the pattern.

//             var temp = op;
//             if ((temp & 128) != 0) C = true;
//             else C = false;
//             temp = (byte)(temp << 1);
//             Write(op) = temp;
//             SetZeroAndNegFlag(Read(CurMemLocation));


//             //Implied Overload

//             if ((A & 128) != 0) C = true;
//             else C = false;
//             A = (byte)(A << 1);
//         }
//         void BCC(byte op)
//         {
//             Branch(!C);
//         }
//         void BCS(byte op)
//         {
//             Branch(C);
//         }
//         void BEQ(byte op)
//         {
//             Branch(Z);
//         }
//         void BIT(byte op)
//         {
//             //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
//             //value store at the tested address
//             var pos = op;
//             if ((A & pos) == 0x00) Z = true;
//             else Z = false;
//             if ((pos & 128) != 0) N = true;
//             else N = false;
//             if ((pos & 64) != 0) V = true;
//             else V = false;


//         }
//         void BMI(byte op)
//         {
//             Branch(N);

//         }
//         void BNE(byte op)
//         {
//             Branch(!Z);
//         }
//         void BPL(byte op)
//         {
//             Branch(!N);
//         }
//         void BRK(byte op)
//         {
//             PC++;
//             NMI();
//         }
//         void BVC(byte op)
//         {
//             Branch(!V);

//         }
//         void BVS(byte op)
//         {
//             Branch(V);
//         }
//         void CLC(byte op)
//         {
//             C = false;
//         }
//         void CLI(byte op)
//         {
//             D = false;
//         }
//         void CLV(byte op)
//         {
//             I = false;
//         }
//         void CMP(byte op)
//         {
//             var aa = op;
//             var bb = A;
//             if (bb > aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = true;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else if (bb < aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = false;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else
//             {
//                 N = false;
//                 Z = true;
//                 C = true;
//             }



//         }
//         void CPX(byte op)
//         {
//             var aa = op;
//             var bb = X;
//             if (bb > aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = true;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else if (bb < aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = false;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else
//             {
//                 N = false;
//                 Z = true;
//                 C = true;
//             }

//         }
//         void CPY(byte op)
//         {
//             var aa = op;
//             var bb = Y;
//             if (bb > aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = true;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else if (bb < aa)
//             {
//                 byte cc = (byte)(bb - aa);
//                 C = false;
//                 Z = false;
//                 N = (cc & 128) != 0;
//             }
//             else
//             {
//                 N = false;
//                 Z = true;
//                 C = true;
//             }

//         }
//         void DEC(byte op)
//         {
//             //I think here we need to read off of the bus
//             //store in var
//             //decrement car
//             //write var back to bus in same location
//             Memory[curMemLocation]--;
//             SetZeroAndNegFlag(Memory[curMemLocation]);


//         }
//         void DEX(byte op)
//         {
//             X--;
//             SetZeroAndNegFlag(X);

//         }
//         void DEY(byte op)
//         {
//             Y--;
//             SetZeroAndNegFlag(Y);

//         }
//         void EOR(byte op)
//         {
//             A ^= op;

//         }
//         void INC(byte op)
//         {
//             Memory[curMemLocation]++;
//             SetZeroAndNegFlag(Memory[curMemLocation]);

//         }
//         void INX(byte op)
//         {
//             X++;
//             SetZeroAndNegFlag(X);

//         }
//         void INY(byte op)
//         {
//             Y++;
//             SetZeroAndNegFlag(Y);

//         }
//         void JMP(byte op)
//         {
//             PC = jmpLocation;


//             //Move the PC to a specific address and skip the next to addresses       
//             //skip because they are the location to jump too
//             PC = SwapNextTwoBytes();


//         }
//         void JSR(byte op)
//         {
//             //JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address
//             var b = BitConverter.GetBytes((ushort)PC + 2);
//             PushToStack(b[1]);
//             PushToStack(b[0]);


//             PC = SwapNextTwoBytes();

//         }
//         void LDA(byte op)
//         {
//             A = op;
//             AccumChanged();
//             //HACK: the accum changed doesn't actually trigger, it didn't change (but was accessed)

//         }
//         void LDX(byte op)
//         {
//             X = op;
//             SetZeroAndNegFlag(X);

//         }
//         void LDY(byte op)
//         {
//             Y = op;
//             SetZeroAndNegFlag(Y);

//         }
//         void LSR(byte op)
//         {
//             if ((op & 1) != 0) C = true;
//             else C = false;
//             op = (byte)(op >> 1);
//             SetZeroAndNegFlag(op);
//             Memory[curMemLocation] = op;


//         }
//         void NOP(byte op)
//         {

//         }
//         void ORA(byte op)
//         {
//             A |= op;
//             SetZeroAndNegFlag(A);
//         }
//         void PHA(byte op)
//         {
//             PushToStack(A);

//         }
//         void PHP(byte op)
//         {
//             var b0 = C;
//             var b1 = Z;
//             var b2 = I;
//             var b3 = D;
//             var b4 = true;
//             var b5 = true;
//             var b6 = V;
//             var b7 = N;
//             var flags = new bool[8]
//             {
//                 b0, b1, b2, b3, b4, b5, b6, b7
//             };
//             byte range = 0;
//             if (flags == null || flags.Length < 8)
//             {
//                 range = 0;
//             }

//             for (int i = 0; i < 8; i++)
//             {
//                 if (flags[i])
//                 {
//                     range |= (byte)(1 << i);
//                 }
//             }
//             PushToStack(range);


//         }
//         void PLA(byte op)
//         {
//             A = PopFromStack();
//             SetZeroAndNegFlag(A);


//         }
//         void PLP(byte op)
//         {
//             var status = PopFromStack();
//             C = (status & 1) != 0;
//             Z = (status & 2) != 0;
//             I = (status & 4) != 0;
//             D = (status & 8) != 0;
//             //Flags.BreakCommandFlag = (status & 16) != 0;
//             //Flags.nullFlag = (status & 32) != 0;
//             V = (status & 64) != 0;
//             N = (status & 128) != 0;


//         }
//         void ROL(byte op)
//         {
//             byte bit0;
//             byte shiftedAccum;
//             if (C) bit0 = 1;
//             else bit0 = 0;
//             //check bit7 to see what the new carry flag chould be
//             C = (op & 128) != 0 ? true : false;
//             shiftedAccum = (byte)(op << 1); //shift the accum left 1
//             Memory[curMemLocation] = (byte)(shiftedAccum | bit0);
//             SetZeroAndNegFlag(Memory[curMemLocation]);

//             //A Overload
//             byte bit0;
//             byte shiftedAccum;
//             if (C) bit0 = 1;
//             else bit0 = 0;
//             //check bit7 to see what the new carry flag chould be
//             C = (A & 128) != 0 ? true : false;
//             shiftedAccum = (byte)(A << 1); //shift the accum left 1
//             A = (byte)(shiftedAccum | bit0);


//         }
//         void ROR(byte op)
//         {
//             byte bit7;
//             if (C) bit7 = 1;
//             else bit7 = 0;
//             bit7 = (byte)(bit7 << 7);
//             //check bit0 to see what the new carry flag chould be
//             C = (op & 1) != 0 ? true : false;
//             var shiftedAccum = (byte)(op >> 1); //shift the accum right 1
//             Memory[curMemLocation] = (byte)(shiftedAccum | bit7);
//             SetZeroAndNegFlag(Memory[curMemLocation]);

//             //Im assuming no-arg overload
//             byte bit7;
//             // carry slots into bit7 and bit 0 is shifted into the carry
//             if (C) bit7 = 1;
//             else bit7 = 0;
//             bit7 = (byte)(bit7 << 7);
//             //check bit0 to see what the new carry flag chould be
//             C = (A & 1) != 0 ? true : false;
//             var shiftedAccum = (byte)(A >> 1); //shift the accum right 1
//             A = (byte)(shiftedAccum | bit7);
//         }
//         void RTI(byte op)
//         {
//             var status = PopFromStack();
//             C = (status & 1) != 0;
//             Z = (status & 2) != 0;
//             I = (status & 4) != 0;
//             D = (status & 8) != 0;
//             V = (status & 64) != 0;
//             N = (status & 128) != 0;
//             var PC1 = PopFromStack();
//             var PC2 = PopFromStack();
//             PC = (ushort)(PC2 << 8 | PC1);

//         }
//         void RTS(byte op)
//         {
//             var addr2 = PopFromStack();
//             var addr1 = PopFromStack();
//             PC = (ushort)((addr1 << 8 | addr2) + 0x0001);


//         }
//         void SBC(byte op)
//         {
//             ADC((byte)~op);


//         }
//         void SEC(byte op)
//         {
//             C = true;
//         }
//         void SED(byte op)
//         {
//             D = true;

//         }
//         void SEI(byte op)
//         {
//             I = true;


//         }
//         void STA(byte op)
//         {
//             Memory[curMemLocation] = A;

//         }
//         void STX(byte op)
//         {
//             Memory[curMemLocation] = RegisterX;

//         }
//         void STY(byte op)
//         {
//             Memory[curMemLocation] = RegisterY;

//         }
//         void TAX(byte op)
//         {
//             X = A;
//             SetZeroAndNegFlag(X);

//         }
//         void TAY(byte op)
//         {
//             Y = A;
//             SetZeroAndNegFlag(Y);


//         }
//         void TSX(byte op)
//         {
//             X = SP;
//             SetZeroAndNegFlag(X);

//         }
//         void TXS(byte op)
//         {
//             SP = X;
//         }
//         void TYA(byte op)
//         {
//             A = Y;
//             SetZeroAndNegFlag(A);
//         }
//         void SLO(byte op)
//         {
//             ASL(op);
//             A |= Memory[curMemLocation];

//         }
//         void ANC(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void RLA(byte op)
//         {
//             ROL(Memory[curMemLocation]);
//             AND(Memory[curMemLocation]);

//         }
//         void SRE(byte op)
//         {
//             ROR(Memory[curMemLocation]);
//             EOR(Memory[curMemLocation]);
//             SetZeroAndNegFlag(Memory[curMemLocation]);
//         }
//         void ALR(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void RRA(byte op)
//         {
//             ROR(op); ADC(op);
//         }
//         void ARR(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void SAX(byte op)
//         {
//             Memory[curMemLocation] = ((byte)(A & RegisterX));
//         }
//         void TXA(byte op)
//         {
//             A = RegisterX;
//             SetZeroAndNegFlag(A);
//         }
//         void ANE(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void SHA(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void TAS(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void ISC(byte op)
//         {
//             Memory[curMemLocation]++;
//             SBC(Memory[curMemLocation]);

//         }
//         void USB(byte op)
//         {
//             SBC(op);
//         }
//         void DCP(byte op)
//         {
//             DEC(op);
//             CMP(A, Memory[curMemLocation]);
//         }
//         void LAX(byte op)
//         {
//             LDA(op);
//             LDX(op);
//         }
//         void SHY(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void SHX(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void LXA(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void LAS(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void SBX(byte op)
//         {
//             throw new NotImplementedException();
//         }
//         void CLD(byte op)
//         {
//             D = false;
//         }
//         #endregion
//         #region Addressing Modes
//         byte IMM() //Immediate
//         {
//             PC++;
//             return Read(PC);
//         }
//         byte XIN() //X IND
//         {
//             //operand is a zero page address

//             PC++;
//             var indexByte = Read(PC);
//             var newPos = (byte)(indexByte + X);
//             var calcedPos = Read(newPos);
//             var calcedPos2 = Read((byte)(newPos + 1));
//             ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
//             return Read(addr);
//         }
//         byte YIN() //Y IND
//         {
//             //Differing from x Indirect, the order is a little different and there
//             //is a carry
//             PC++;
//             byte indexByte = Read(PC);
//             byte b1 = Read(indexByte);
//             byte b2 = Read((byte)(indexByte + 1)); ;
//             if (indexByte == 0xFF) b2++;
//             ushort addr = 0x00;
//             addr = (ushort)(b2 << 8 | b1);
//             addr += Y;
//             return Read(addr);
//         }
//         byte ABS() //Absolute
//         {
//             //get the high and low bytes for the address and build a short;
//             PC++;
//             byte PCH = Read(PC);
//             PC++;
//             byte PCL = Read(PC);
//             ushort addr = (ushort)(PCH << 8 | PCL);
//             return Read(addr);
//         }
//         byte XAB() //X Absolute
//         {
//             PC++;
//             byte PCH = Read(PC);
//             PC++;
//             byte PCL = Read(PC);
//             ushort addr = (ushort)(PCH << 8 | PCL);
//             addr = (ushort)(addr + X);
//             return Read(addr);
//         }
//         byte YAB() //Y Absolute
//         {
//             PC++;
//             byte PCH = Read(PC);
//             PC++;
//             byte PCL = Read(PC);
//             ushort addr = (ushort)(PCH << 8 | PCL);
//             addr = (ushort)(addr + Y);
//             return Read(addr);
//         }
//         byte IMP() //Implied
//         {
//             throw new NotImplementedException();
//         }
//         byte IND() //Indirect
//         {

//             //From my understanding it's:
//             //read the immediate bytes
//             //go to the location from those bytes
//             //return the value from the location calculated from the original immediate calculation

//             PC++;
//             byte PCH = Read(PC);
//             PC++;
//             byte PCL = Read(PC);
//             ushort addr = (ushort)(PCH << 8 | PCL);
//             if ((addr & 0x00FF) == 0xFF)
//             {
//                 return Read(addr += 1);
//             }
//             var b1 = Read(addr);
//             var b2 = Read((byte)(addr + 1));
//             var calcedLocation = (ushort)(b2 << 8 | b1);

//             return Read(calcedLocation);
//         }
//         byte REL() //relative
//         {
//             throw new NotImplementedException();
//         }
//         byte ZPG() //Zero Page
//         {
//             PC++;
//             return Read(PC);
//         }
//         byte XZP() //X Zero Page
//         {
//             PC++;
//             return Read((byte)(Read(PC) + X));
//         }
//         byte YZP() //Y Zero Page
//         {
//             PC++;
//             return Read((byte)(Read(PC) + Y));
//         }
//         #endregion
//         #region Helpers
        //These Read and Write methods are just wrappers for legibility throughout the opCodes.
        // private byte Read(ushort location)
        // {
        //     return _bus.Read(location);
        // }

        // private void Write(ushort addr, byte value)
        // {
        //     _bus.Write(addr, value);
        // }

        // private void Branch(bool DoBranch)
        // {
        //     PC++;
        //     var jumpDistance = Read(PC) + 2; //plus two to get over the operands.
        //     //determine if the jump is fwd or bwd.
        //     if (DoBranch && jumpDistance >= 0x80) PC -= (byte)(0xFF - jumpDistance + 1);
        //     else if (DoBranch) PC += ((byte)jumpDistance);
        //     else PC += 2;
        // }
        // private void PushToStack(byte operand)
        // {
        //     throw new NotImplementedException();
        // }
        // private byte PopFromStack()
        // {
        //     throw new NotImplementedException();
        // }
        // private void SetZeroAndNegFlag(byte operand)
        // {
        //     throw new NotImplementedException();
        // }

        // #endregion
    }
    internal class INSTR
    {
        //Possibly override ToString here to print out for the disassembler
        private delegate void OpDel(byte b);
        private delegate byte AddrMode();
        private string _name;

        private int _clockCycles;
        private int _additionalClockCycles;
        OpDel _opDel;
        AddrMode _addrMode;
        public INSTR(string name, Action<byte> op, Func<byte> addrMode, int clockCycles)
        {
            _name = name;
            _clockCycles = clockCycles;
            _opDel = new OpDel(op);
            _addrMode = new AddrMode(addrMode);
        }
        public int Execute()
        {
            byte operand = _addrMode.Invoke();
            _opDel.Invoke(operand);
            return _clockCycles + _additionalClockCycles;
        }
    }


}