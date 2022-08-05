using EmulatorTools.Memory;
namespace SixtyFiveOhTwo.CPUCore;
public partial class CPU
{
    public CPU(IMemory bus, bool BCDEnabled)
    {
        _BCDEnabled = BCDEnabled;
        _bus = bus;
        //Beware those who enter for this is a beast
        static Instruction M(string name, Action op, Action addr, int cycles) { return new Instruction(name, op, addr, cycles); }
        PC = 0xC000; //Change this to actually read the correct PC location on startup. //NOTE/WARNING/HACK, this is probably what has been fucking me up. look into this
        Cycles = 7; //TODO: change this to be the true value, (what are those seven starting cycles)
        I = true; //I think this starts on true
        N = false; //test
        _opCodes.AddRange(new List<Instruction>(){
//    00                 01                 02                 03                 04                 05                 06                 07                 08                 09                 0A                 0B                 0C                 0D                 0E                 0F            
/*00*/M("BRK",BRK,IMP,7),M("ORA",ORA,XIN,6),M("JAM",JAM,IMP,2),M("SLO",SLO,XIN,8),M("NOP",NOP,ZPG,3),M("ORA",ORA,ZPG,3),M("ASL",ASL,ZPG,5),M("SLO",SLO,ZPG,5),M("PHP",PHP,IMP,3),M("ORA",ORA,IMM,2),M("ASL",ASL,IMP,2),M("ANC",ANC,IMM,2),M("NOP",NOP,ABS,4),M("ORA",ORA,ABS,4),M("ASL",ASL,ABS,6),M("SLO",SLO,ABS,6),
/*10*/M("BPL",BPL,REL,2),M("ORA",ORA,YIN,5),M("JAM",JAM,IMP,0),M("SLO",SLO,YIN,8),M("NOP",NOP,XZP,4),M("ORA",ORA,XZP,4),M("ASL",ASL,XZP,6),M("SLO",SLO,XZP,6),M("CLC",CLC,IMP,2),M("ORA",ORA,YAB,4),M("NOP",NOP,IMP,7),M("SLO",SLO,YAB,7),M("NOP",NOP,XAB,7),M("ORA",ORA,XAB,7),M("ASL",ASL,XAB,7),M("SLO",SLO,XAB,7),
/*20*/M("JSR",JSR,ABS,7),M("AND",AND,XIN,7),M("JAM",JAM,IMP,0),M("RLA",RLA,XIN,7),M("BIT",BIT,ZPG,7),M("AND",AND,ZPG,7),M("ROL",ROL,ZPG,7),M("RLA",RLA,ZPG,7),M("PLP",PLP,IMP,7),M("AND",AND,IMM,7),M("ROL",ROL,IMP,7),M("ANC",ANC,IMM,7),M("BIT",BIT,ABS,7),M("AND",AND,ABS,7),M("ROL",ROL,ABS,7),M("RLA",RLA,ABS,7),
/*30*/M("BMI",BMI,REL,7),M("AND",AND,YIN,7),M("JAM",JAM,IMP,0),M("RLA",RLA,YIN,7),M("NOP",NOP,XZP,7),M("AND",AND,XZP,7),M("ROL",ROL,XZP,7),M("RLA",RLA,XZP,7),M("SEC",SEC,IMP,7),M("AND",AND,YAB,7),M("NOP",NOP,IMP,7),M("RLA",RLA,YAB,7),M("NOP",NOP,XAB,7),M("AND",AND,XAB,7),M("ROL",ROL,XAB,7),M("RLA",RLA,XAB,7),
/*40*/M("RTI",RTI,IMP,7),M("EOR",EOR,XIN,7),M("JAM",JAM,IMP,0),M("SRE",SRE,XIN,7),M("NOP",NOP,ZPG,7),M("EOR",EOR,ZPG,7),M("LSR",LSR,ZPG,7),M("SRE",SRE,ZPG,7),M("PHA",PHA,IMP,7),M("EOR",EOR,IMM,7),M("LSR",LSR,IMP,7),M("ALR",ALR,IMM,7),M("JMP",JMP,ABS,7),M("EOR",EOR,ABS,7),M("LSR",LSR,ABS,7),M("SRE",SRE,ABS,7),
/*50*/M("BVC",BVC,REL,2),M("EOR",EOR,YIN,7),M("JAM",JAM,IMP,0),M("SRE",SRE,YIN,7),M("NOP",NOP,XZP,7),M("EOR",EOR,XZP,7),M("LSR",LSR,XZP,7),M("SRE",SRE,XZP,7),M("CLI",CLI,IMP,7),M("EOR",EOR,YAB,7),M("NOP",NOP,IMP,7),M("SRE",SRE,YAB,7),M("NOP",NOP,XAB,7),M("EOR",EOR,XAB,7),M("LSR",LSR,XAB,7),M("SRE",SRE,XAB,7),
/*60*/M("RTS",RTS,IMP,7),M("ADC",ADC,XIN,7),M("JAM",JAM,IMP,0),M("RRA",RRA,XIN,7),M("NOP",NOP,ZPG,7),M("ADC",ADC,ZPG,7),M("ROR",ROR,ZPG,7),M("RRA",RRA,ZPG,7),M("PLA",PLA,IMP,7),M("ADC",ADC,IMM,7),M("ROR",ROR,IMP,7),M("ARR",ARR,IMM,7),M("JMP",JMP,IND,7),M("ADC",ADC,ABS,7),M("ROR",ROR,ABS,7),M("RRA",RRA,ABS,7),
/*70*/M("BVS",BVS,REL,7),M("ADC",ADC,YIN,7),M("JAM",JAM,IMP,0),M("RRA",RRA,YIN,7),M("NOP",NOP,XZP,7),M("ADC",ADC,XZP,7),M("ROR",ROR,XZP,7),M("RRA",RRA,XZP,7),M("SEI",SEI,IMP,7),M("ADC",ADC,YAB,7),M("NOP",NOP,IMP,7),M("RRA",RRA,YAB,7),M("NOP",NOP,XAB,7),M("ADC",ADC,XAB,7),M("ROR",ROR,XAB,7),M("RRA",RRA,XAB,7),
/*80*/M("NOP",NOP,IMM,7),M("STA",STA,XIN,7),M("NOP",NOP,IMM,7),M("SAX",SAX,XIN,7),M("STY",STY,ZPG,7),M("STA",STA,ZPG,7),M("STX",STX,ZPG,7),M("SAX",SAX,ZPG,7),M("DEY",DEY,IMP,7),M("NOP",NOP,IMM,7),M("TXA",TXA,IMP,7),M("ANE",ANE,IMM,7),M("STY",STY,ABS,7),M("STA",STA,ABS,7),M("STX",STX,ABS,7),M("SAX",SAX,ABS,7),
/*90*/M("BCC",BCC,REL,7),M("STA",STA,YIN,7),M("JAM",JAM,IMP,0),M("SHA",SHA,YIN,7),M("STY",STY,XZP,7),M("STA",STA,XZP,7),M("STX",STX,YZP,7),M("SAX",SAX,YZP,7),M("TYA",TYA,IMP,7),M("STA",STA,YAB,7),M("TXS",TXS,IMP,7),M("TAS",TAS,YAB,7),M("SHY",SHY,XAB,7),M("STA",STA,XAB,7),M("SHX",SHX,YAB,7),M("SHY",SHY,YAB,7),
/*A0*/M("LDY",LDY,IMM,7),M("LDA",LDA,XIN,7),M("LDX",LDX,IMM,7),M("LAX",LAX,XIN,7),M("LDY",LDY,ZPG,7),M("LDA",LDA,ZPG,7),M("LDX",LDX,ZPG,7),M("LAX",LAX,ZPG,7),M("TAY",TAY,IMP,7),M("LDA",LDA,IMM,7),M("TAX",TAX,IMP,7),M("LXA",LXA,IMM,7),M("LDY",LDY,ABS,7),M("LDA",LDA,ABS,7),M("LDX",LDX,ABS,7),M("LAX",LAX,ABS,7),
/*B0*/M("BCS",BCS,REL,7),M("LDA",LDA,YIN,7),M("JAM",JAM,IMP,0),M("LAX",LAX,YIN,7),M("LDY",LDY,XZP,7),M("LDA",LDA,XZP,7),M("LDX",LDX,YZP,7),M("LAX",LAX,YZP,7),M("CLV",CLV,IMP,7),M("LDA",LDA,YAB,7),M("TSX",TSX,IMP,7),M("LAS",LAS,YAB,7),M("LDY",LDY,XAB,7),M("LDA",LDA,XAB,7),M("LDX",LDX,YAB,7),M("LAX",LAX,YAB,7),
/*C0*/M("CPY",CPY,IMM,7),M("CMP",CMP,XIN,7),M("NOP",NOP,IMM,7),M("DCP",DCP,XIN,7),M("CPY",CPY,ZPG,7),M("CMP",CMP,ZPG,7),M("DEC",DEC,ZPG,7),M("DCP",DCP,ZPG,7),M("INY",INY,IMP,7),M("CMP",CMP,IMM,7),M("DEX",DEX,IMP,7),M("SBX",SBX,IMM,7),M("CPY",CPY,ABS,7),M("CMP",CMP,ABS,7),M("DEC",DEC,ABS,7),M("DCP",DCP,ABS,7),
/*D0*/M("BNE",BNE,REL,7),M("CMP",CMP,YIN,7),M("JAM",JAM,IMP,0),M("DCP",DCP,YIN,7),M("NOP",NOP,XZP,7),M("CMP",CMP,XZP,7),M("DEC",DEC,XZP,7),M("DCP",DCP,XZP,7),M("CLD",CLD,IMP,2),M("CMP",CMP,YAB,7),M("NOP",NOP,IMP,7),M("DCP",DCP,YAB,7),M("NOP",NOP,XAB,7),M("CMP",CMP,XAB,7),M("DEC",DEC,XAB,7),M("DCP",DCP,XAB,7),
/*E0*/M("CPX",CPX,IMM,7),M("SBC",SBC,XIN,7),M("NOP",NOP,IMM,7),M("ISB",ISC,XIN,7),M("CPX",CPX,ZPG,7),M("SBC",SBC,ZPG,7),M("INC",INC,ZPG,7),M("ISB",ISC,ZPG,7),M("INX",INX,IMP,7),M("SBC",SBC,IMM,7),M("NOP",NOP,IMP,7),M("SBC",SBC,IMM,7),M("CPX",CPX,ABS,7),M("SBC",SBC,ABS,7),M("INC",INC,ABS,7),M("ISB",ISC,ABS,7),
/*F0*/M("BEQ",BEQ,REL,7),M("SBC",SBC,YIN,7),M("JAM",JAM,IMP,0),M("ISB",ISC,YIN,7),M("NOP",NOP,XZP,7),M("SBC",SBC,XZP,7),M("INC",INC,XZP,7),M("ISB",ISC,XZP,7),M("SED",SED,IMP,7),M("SBC",SBC,YAB,7),M("NOP",NOP,IMP,7),M("ISB",ISC,YAB,7),M("NOP",NOP,XAB,7),M("SBC",SBC,XAB,7),M("INC",INC,XAB,7),M("ISB",ISC,XAB,7)
            });
    }
}
