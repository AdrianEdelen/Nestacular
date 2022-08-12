using EmulatorTools.Memory;
using SixtyFiveOhTwo.Registers;
using SixtyFiveOhTwo.Flags;
using SixtyFiveOhTwo.Enums;

namespace SixtyFiveOhTwo;
public partial class CPU
{
    public CPU(IMemory bus, bool BCDEnabled)
    {
        _registers = new RegisterGroup(
            new UshortRegister(0xC000, "Program Counter"),/* TODO: What is the actual starting value here */
            new ByteRegister(0xFD, "Stack Pointer"),
            new ByteRegister(0x00, "Accumulator"),
            new ByteRegister(0x00, "X Register"),
            new ByteRegister(0x00, "Y register")
            );
        _flags = new StatusFlag()
        {
            InterruptDisable = true, /* //TODO: what is the actual starting value;*/
            Negative = false         /* //TODO: what is the actual starting value */
        };


        _BCDEnabled = BCDEnabled;
        _bus = bus;
        InternalClock = 7; 

        //Beware those who enter for this is a beast
        //This list is byte mapped, the order of the list is critical, and you can determine what byte aligns with what opcode by using the comment table surrounding the list definition.
        static Instruction M(string name, Func<AddressModes, int> op, Func<AddressModes> addr) { return new Instruction(name, op, addr); }
        _opCodes.AddRange(new List<Instruction>(){
//    00               01               02               03               04               05               06               07               08               09               0A               0B               0C               0D               0E                0F            
/*00*/M("BRK",BRK,IMP),M("ORA",ORA,XIN),M("JAM",JAM,IMP),M("SLO",SLO,XIN),M("NOP",NOP,ZPG),M("ORA",ORA,ZPG),M("ASL",ASL,ZPG),M("SLO",SLO,ZPG),M("PHP",PHP,IMP),M("ORA",ORA,IMM),M("ASL",ASL,IMP),M("ANC",ANC,IMM),M("NOP",NOP,ABS),M("ORA",ORA,ABS),M("ASL",ASL,ABS),M("SLO",SLO,ABS),
/*10*/M("BPL",BPL,REL),M("ORA",ORA,YIN),M("JAM",JAM,IMP),M("SLO",SLO,YIN),M("NOP",NOP,XZP),M("ORA",ORA,XZP),M("ASL",ASL,XZP),M("SLO",SLO,XZP),M("CLC",CLC,IMP),M("ORA",ORA,YAB),M("NOP",NOP,IMP),M("SLO",SLO,YAB),M("NOP",NOP,XAB),M("ORA",ORA,XAB),M("ASL",ASL,XAB),M("SLO",SLO,XAB),
/*20*/M("JSR",JSR,ABS),M("AND",AND,XIN),M("JAM",JAM,IMP),M("RLA",RLA,XIN),M("BIT",BIT,ZPG),M("AND",AND,ZPG),M("ROL",ROL,ZPG),M("RLA",RLA,ZPG),M("PLP",PLP,IMP),M("AND",AND,IMM),M("ROL",ROL,IMP),M("ANC",ANC,IMM),M("BIT",BIT,ABS),M("AND",AND,ABS),M("ROL",ROL,ABS),M("RLA",RLA,ABS),
/*30*/M("BMI",BMI,REL),M("AND",AND,YIN),M("JAM",JAM,IMP),M("RLA",RLA,YIN),M("NOP",NOP,XZP),M("AND",AND,XZP),M("ROL",ROL,XZP),M("RLA",RLA,XZP),M("SEC",SEC,IMP),M("AND",AND,YAB),M("NOP",NOP,IMP),M("RLA",RLA,YAB),M("NOP",NOP,XAB),M("AND",AND,XAB),M("ROL",ROL,XAB),M("RLA",RLA,XAB),
/*40*/M("RTI",RTI,IMP),M("EOR",EOR,XIN),M("JAM",JAM,IMP),M("SRE",SRE,XIN),M("NOP",NOP,ZPG),M("EOR",EOR,ZPG),M("LSR",LSR,ZPG),M("SRE",SRE,ZPG),M("PHA",PHA,IMP),M("EOR",EOR,IMM),M("LSR",LSR,IMP),M("ALR",ALR,IMM),M("JMP",JMP,ABS),M("EOR",EOR,ABS),M("LSR",LSR,ABS),M("SRE",SRE,ABS),
/*50*/M("BVC",BVC,REL),M("EOR",EOR,YIN),M("JAM",JAM,IMP),M("SRE",SRE,YIN),M("NOP",NOP,XZP),M("EOR",EOR,XZP),M("LSR",LSR,XZP),M("SRE",SRE,XZP),M("CLI",CLI,IMP),M("EOR",EOR,YAB),M("NOP",NOP,IMP),M("SRE",SRE,YAB),M("NOP",NOP,XAB),M("EOR",EOR,XAB),M("LSR",LSR,XAB),M("SRE",SRE,XAB),
/*60*/M("RTS",RTS,IMP),M("ADC",ADC,XIN),M("JAM",JAM,IMP),M("RRA",RRA,XIN),M("NOP",NOP,ZPG),M("ADC",ADC,ZPG),M("ROR",ROR,ZPG),M("RRA",RRA,ZPG),M("PLA",PLA,IMP),M("ADC",ADC,IMM),M("ROR",ROR,IMP),M("ARR",ARR,IMM),M("JMP",JMP,IND),M("ADC",ADC,ABS),M("ROR",ROR,ABS),M("RRA",RRA,ABS),
/*70*/M("BVS",BVS,REL),M("ADC",ADC,YIN),M("JAM",JAM,IMP),M("RRA",RRA,YIN),M("NOP",NOP,XZP),M("ADC",ADC,XZP),M("ROR",ROR,XZP),M("RRA",RRA,XZP),M("SEI",SEI,IMP),M("ADC",ADC,YAB),M("NOP",NOP,IMP),M("RRA",RRA,YAB),M("NOP",NOP,XAB),M("ADC",ADC,XAB),M("ROR",ROR,XAB),M("RRA",RRA,XAB),
/*80*/M("NOP",NOP,IMM),M("STA",STA,XIN),M("NOP",NOP,IMM),M("SAX",SAX,XIN),M("STY",STY,ZPG),M("STA",STA,ZPG),M("STX",STX,ZPG),M("SAX",SAX,ZPG),M("DEY",DEY,IMP),M("NOP",NOP,IMM),M("TXA",TXA,IMP),M("ANE",ANE,IMM),M("STY",STY,ABS),M("STA",STA,ABS),M("STX",STX,ABS),M("SAX",SAX,ABS),
/*90*/M("BCC",BCC,REL),M("STA",STA,YIN),M("JAM",JAM,IMP),M("SHA",SHA,YIN),M("STY",STY,XZP),M("STA",STA,XZP),M("STX",STX,YZP),M("SAX",SAX,YZP),M("TYA",TYA,IMP),M("STA",STA,YAB),M("TXS",TXS,IMP),M("TAS",TAS,YAB),M("SHY",SHY,XAB),M("STA",STA,XAB),M("SHX",SHX,YAB),M("SHY",SHY,YAB),
/*A0*/M("LDY",LDY,IMM),M("LDA",LDA,XIN),M("LDX",LDX,IMM),M("LAX",LAX,XIN),M("LDY",LDY,ZPG),M("LDA",LDA,ZPG),M("LDX",LDX,ZPG),M("LAX",LAX,ZPG),M("TAY",TAY,IMP),M("LDA",LDA,IMM),M("TAX",TAX,IMP),M("LXA",LXA,IMM),M("LDY",LDY,ABS),M("LDA",LDA,ABS),M("LDX",LDX,ABS),M("LAX",LAX,ABS),
/*B0*/M("BCS",BCS,REL),M("LDA",LDA,YIN),M("JAM",JAM,IMP),M("LAX",LAX,YIN),M("LDY",LDY,XZP),M("LDA",LDA,XZP),M("LDX",LDX,YZP),M("LAX",LAX,YZP),M("CLV",CLV,IMP),M("LDA",LDA,YAB),M("TSX",TSX,IMP),M("LAS",LAS,YAB),M("LDY",LDY,XAB),M("LDA",LDA,XAB),M("LDX",LDX,YAB),M("LAX",LAX,YAB),
/*C0*/M("CPY",CPY,IMM),M("CMP",CMP,XIN),M("NOP",NOP,IMM),M("DCP",DCP,XIN),M("CPY",CPY,ZPG),M("CMP",CMP,ZPG),M("DEC",DEC,ZPG),M("DCP",DCP,ZPG),M("INY",INY,IMP),M("CMP",CMP,IMM),M("DEX",DEX,IMP),M("SBX",SBX,IMM),M("CPY",CPY,ABS),M("CMP",CMP,ABS),M("DEC",DEC,ABS),M("DCP",DCP,ABS),
/*D0*/M("BNE",BNE,REL),M("CMP",CMP,YIN),M("JAM",JAM,IMP),M("DCP",DCP,YIN),M("NOP",NOP,XZP),M("CMP",CMP,XZP),M("DEC",DEC,XZP),M("DCP",DCP,XZP),M("CLD",CLD,IMP),M("CMP",CMP,YAB),M("NOP",NOP,IMP),M("DCP",DCP,YAB),M("NOP",NOP,XAB),M("CMP",CMP,XAB),M("DEC",DEC,XAB),M("DCP",DCP,XAB),
/*E0*/M("CPX",CPX,IMM),M("SBC",SBC,XIN),M("NOP",NOP,IMM),M("ISB",ISC,XIN),M("CPX",CPX,ZPG),M("SBC",SBC,ZPG),M("INC",INC,ZPG),M("ISB",ISC,ZPG),M("INX",INX,IMP),M("SBC",SBC,IMM),M("NOP",NOP,IMP),M("SBC",SBC,IMM),M("CPX",CPX,ABS),M("SBC",SBC,ABS),M("INC",INC,ABS),M("ISB",ISC,ABS),
/*F0*/M("BEQ",BEQ,REL),M("SBC",SBC,YIN),M("JAM",JAM,IMP),M("ISB",ISC,YIN),M("NOP",NOP,XZP),M("SBC",SBC,XZP),M("INC",INC,XZP),M("ISB",ISC,XZP),M("SED",SED,IMP),M("SBC",SBC,YAB),M("NOP",NOP,IMP),M("ISB",ISC,YAB),M("NOP",NOP,XAB),M("SBC",SBC,XAB),M("INC",INC,XAB),M("ISB",ISC,XAB)
            });
    }
}
