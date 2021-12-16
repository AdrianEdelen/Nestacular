using Nestacular.NES2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nestacular.NESCore.CPU.Opcodes;

namespace Nestacular.NESCore
{
    internal class CPU2
    {
        /* Names of variables match the documented names of the components
         * this makes it easier to compare against established Documentation */

        /* The CPU reads and writes to the bus to communicate with the rest of the NES, 
         * it should also function as a regular 6502 just without decimal mode
         * as the NES RICOH 2A0C did not have decimal mode enabled */


        //bus ref
        BUS _bus;

        //Registers
        private ushort PC = 0x0000; //Program Counter
        private byte SP = 0xFD; //Stack Pointer
        private byte A = 0x00; //Accumulator
        private byte X = 0x00; //X Register
        private byte Y = 0x00; //Y Register
        private byte P;         //Status Byte

        //Flags
        private bool C = false; //Carry Flag
        private bool Z = false; //Zero Flag
        private bool I = false; //Interrupt Disable Flag
        private bool D = false; //Decimal Mode Flag
        private bool B = false; //Break Command Flag
        private bool V = false; //Null Flag
        private bool N = false; //

        //Internal Flags and helper variables
        //These do not have real world equivalents.
        private bool _isHalted = false;

        private List<INSTR> _opCodes = new List<INSTR>()
        {
            
            new INSTR("BRK", new INSTR.OpDel(BRK), new INSTR.AddrMode(Implied),   7),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(XIndirect), 6),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   2),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(XIndirect), 8),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(ZeroPage),  3),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(ZeroPage),  3),
            new INSTR("ASL", new INSTR.OpDel(ASL), new INSTR.AddrMode(ZeroPage),  5),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(ZeroPage),  5),
            new INSTR("PHP", new INSTR.OpDel(PHP), new INSTR.AddrMode(Implied),   3),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(Immediate), 2),
            new INSTR("ASL", new INSTR.OpDel(ASL), new INSTR.AddrMode(Implied),   2),
            new INSTR("ANC", new INSTR.OpDel(ANC), new INSTR.AddrMode(Immediate), 2),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Absolute),  4),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(Absolute),  4),
            new INSTR("ASL", new INSTR.OpDel(ASL), new INSTR.AddrMode(Absolute),  6),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(Absolute),  6),
            
            new INSTR("BPL", new INSTR.OpDel(BPL), new INSTR.AddrMode(Relative),   2), //**
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(YIndirect),  5), //*
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),    2),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(YIndirect),  4),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("ASL", new INSTR.OpDel(ASL), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("CLC", new INSTR.OpDel(CLC), new INSTR.AddrMode(Implied),    7),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(YAbsolute),  7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),    7),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(YAbsolute),  7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("ORA", new INSTR.OpDel(ORA), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("ASL", new INSTR.OpDel(ASL), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("SLO", new INSTR.OpDel(SLO), new INSTR.AddrMode(XAbsolute),  7),
            
            new INSTR("JSR", new INSTR.OpDel(JSR), new INSTR.AddrMode(Absolute),  7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("BIT", new INSTR.OpDel(BIT), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("ROL", new INSTR.OpDel(ROL), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("PLP", new INSTR.OpDel(PLP), new INSTR.AddrMode(Implied),   7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(Immediate), 7),
            new INSTR("ROL", new INSTR.OpDel(ROL), new INSTR.AddrMode(Implied),   7),
            new INSTR("ANC", new INSTR.OpDel(ANC), new INSTR.AddrMode(Immediate), 7),
            new INSTR("BIT", new INSTR.OpDel(BIT), new INSTR.AddrMode(Absolute),  7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(Absolute),  7),
            new INSTR("ROL", new INSTR.OpDel(ROL), new INSTR.AddrMode(Absolute),  7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BMI", new INSTR.OpDel(BMI), new INSTR.AddrMode(Implied),   7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("ROL", new INSTR.OpDel(ROL), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("SEC", new INSTR.OpDel(SEC), new INSTR.AddrMode(Implied),   7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("AND", new INSTR.OpDel(AND), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("ROL", new INSTR.OpDel(ROL), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("RLA", new INSTR.OpDel(RLA), new INSTR.AddrMode(XAbsolute), 7),
            
            new INSTR("RTI", new INSTR.OpDel(RTI), new INSTR.AddrMode(Implied),   7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("LSR", new INSTR.OpDel(LSR), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("PHA", new INSTR.OpDel(PHA), new INSTR.AddrMode(Implied),   7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(Immediate), 7),
            new INSTR("LSR", new INSTR.OpDel(LSR), new INSTR.AddrMode(Implied),   7),
            new INSTR("ALR", new INSTR.OpDel(ALR), new INSTR.AddrMode(Immediate), 7),
            new INSTR("JMP", new INSTR.OpDel(JMP), new INSTR.AddrMode(Implied),   7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(Absolute),  7),
            new INSTR("LSR", new INSTR.OpDel(LSR), new INSTR.AddrMode(Absolute),  7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BVC", new INSTR.OpDel(BVC), new INSTR.AddrMode(Implied),   7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("LSR", new INSTR.OpDel(LSR), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("CLI", new INSTR.OpDel(CLI), new INSTR.AddrMode(Implied),   7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("EOR", new INSTR.OpDel(EOR), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("LSR", new INSTR.OpDel(LSR), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("SRE", new INSTR.OpDel(SRE), new INSTR.AddrMode(XAbsolute), 7),
            
            new INSTR("RTS", new INSTR.OpDel(RTS), new INSTR.AddrMode(Implied),   7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("ROR", new INSTR.OpDel(ROR), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("PLA", new INSTR.OpDel(PLA), new INSTR.AddrMode(Implied),   7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(Immediate), 7),
            new INSTR("ROR", new INSTR.OpDel(ROR), new INSTR.AddrMode(Implied),   7),
            new INSTR("ARR", new INSTR.OpDel(ARR), new INSTR.AddrMode(Immediate), 7),
            new INSTR("JMP", new INSTR.OpDel(JMP), new INSTR.AddrMode(Indirect),  7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(Absolute),  7),
            new INSTR("ROR", new INSTR.OpDel(ROR), new INSTR.AddrMode(Absolute),  7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BVS", new INSTR.OpDel(BVS), new INSTR.AddrMode(Implied),   7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("ROR", new INSTR.OpDel(ROR), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("SEI", new INSTR.OpDel(SEI), new INSTR.AddrMode(Implied),   7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("ADC", new INSTR.OpDel(ADC), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("ROR", new INSTR.OpDel(ROR), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("RRA", new INSTR.OpDel(RRA), new INSTR.AddrMode(XAbsolute), 7),
           
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Immediate), 7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Immediate), 7),
            new INSTR("SAX", new INSTR.OpDel(SAX), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("STY", new INSTR.OpDel(STY), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("STX", new INSTR.OpDel(STX), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("SAX", new INSTR.OpDel(SAX), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("DEY", new INSTR.OpDel(DEY), new INSTR.AddrMode(Implied),   7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Immediate), 7),
            new INSTR("TXA", new INSTR.OpDel(TXA), new INSTR.AddrMode(Implied),   7),
            new INSTR("ANE", new INSTR.OpDel(ANE), new INSTR.AddrMode(Immediate), 7),
            new INSTR("STY", new INSTR.OpDel(STY), new INSTR.AddrMode(Absolute),  7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(Absolute),  7),
            new INSTR("STX", new INSTR.OpDel(STX), new INSTR.AddrMode(Absolute),  7),
            new INSTR("SAX", new INSTR.OpDel(SAX), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BCC", new INSTR.OpDel(BCC), new INSTR.AddrMode(Implied),   7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("SHA", new INSTR.OpDel(SHA), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("STY", new INSTR.OpDel(STY), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(XZeroPage), 7),
            new INSTR("STX", new INSTR.OpDel(STX), new INSTR.AddrMode(YZeroPage),  7),
            new INSTR("SAX", new INSTR.OpDel(SAX), new INSTR.AddrMode(YZeroPage),  7),
            new INSTR("TYA", new INSTR.OpDel(TYA), new INSTR.AddrMode(Implied),   7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("TXS", new INSTR.OpDel(TXS), new INSTR.AddrMode(Implied),   7),
            new INSTR("TAS", new INSTR.OpDel(TAS), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("SHY", new INSTR.OpDel(SHY), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("STA", new INSTR.OpDel(STA), new INSTR.AddrMode(XAbsolute), 7),
            new INSTR("SHX", new INSTR.OpDel(SHX), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("SHY", new INSTR.OpDel(SHY), new INSTR.AddrMode(YAbsolute), 7),
           
            new INSTR("LDY", new INSTR.OpDel(LDY), new INSTR.AddrMode(Immediate),   7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("LDX", new INSTR.OpDel(LDX), new INSTR.AddrMode(Immediate),   7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("LDY", new INSTR.OpDel(LDY), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("LDX", new INSTR.OpDel(LDX), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("TAY", new INSTR.OpDel(TAY), new INSTR.AddrMode(Implied),   7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(Immediate), 7),
            new INSTR("TAX", new INSTR.OpDel(TAX), new INSTR.AddrMode(Implied),   7),
            new INSTR("LXA", new INSTR.OpDel(LXA), new INSTR.AddrMode(Immediate),   7),
            new INSTR("LDY", new INSTR.OpDel(LDY), new INSTR.AddrMode(Absolute),  7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(Absolute),  7),
            new INSTR("LDX", new INSTR.OpDel(LDX), new INSTR.AddrMode(Absolute),  7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(Absolute),  7),
          
            new INSTR("BCS", new INSTR.OpDel(BCS), new INSTR.AddrMode(Implied),   7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("LDY", new INSTR.OpDel(LDY), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("LDX", new INSTR.OpDel(LDX), new INSTR.AddrMode(YZeroPage),  7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(YZeroPage),  7),
            new INSTR("CLV", new INSTR.OpDel(CLV), new INSTR.AddrMode(Implied),   7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("TSX", new INSTR.OpDel(TSX), new INSTR.AddrMode(Implied),   7),
            new INSTR("LAS", new INSTR.OpDel(LAS), new INSTR.AddrMode(YAbsolute),   7),
            new INSTR("LDY", new INSTR.OpDel(LDY), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("LDA", new INSTR.OpDel(LDA), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("LDX", new INSTR.OpDel(LDX), new INSTR.AddrMode(YAbsolute),  7),
            new INSTR("LAX", new INSTR.OpDel(LAX), new INSTR.AddrMode(YAbsolute),  7),
           
            new INSTR("CPY", new INSTR.OpDel(CPY), new INSTR.AddrMode(Immediate),   7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Immediate),   7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("CPY", new INSTR.OpDel(CPY), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("DEC", new INSTR.OpDel(DEC), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("INY", new INSTR.OpDel(INY), new INSTR.AddrMode(Implied),   7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(Immediate), 7),
            new INSTR("DEX", new INSTR.OpDel(DEX), new INSTR.AddrMode(Implied),   7),
            new INSTR("SBX", new INSTR.OpDel(SBX), new INSTR.AddrMode(Immediate),   7),
            new INSTR("CPY", new INSTR.OpDel(CPY), new INSTR.AddrMode(Absolute),  7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(Absolute),  7),
            new INSTR("DEC", new INSTR.OpDel(DEC), new INSTR.AddrMode(Absolute),  7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BNE", new INSTR.OpDel(BNE), new INSTR.AddrMode(Implied),   7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("DEC", new INSTR.OpDel(DEC), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("CLD", new INSTR.OpDel(CLD), new INSTR.AddrMode(Implied),   7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(YAbsolute),   7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("DEC", new INSTR.OpDel(DEC), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("DCP", new INSTR.OpDel(DCP), new INSTR.AddrMode(XAbsolute),  7),
            
            new INSTR("CPX", new INSTR.OpDel(CPX), new INSTR.AddrMode(Immediate),   7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Immediate),   7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(XIndirect), 7),
            new INSTR("CMP", new INSTR.OpDel(CMP), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("INC", new INSTR.OpDel(INC), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(ZeroPage),  7),
            new INSTR("INX", new INSTR.OpDel(INX), new INSTR.AddrMode(Implied),   7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(Immediate), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("USBC",new INSTR.OpDel(USBC), new INSTR.AddrMode(Immediate),   7),
            new INSTR("CPX", new INSTR.OpDel(CPX), new INSTR.AddrMode(Absolute),  7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(Absolute),  7),
            new INSTR("INC", new INSTR.OpDel(INC), new INSTR.AddrMode(Absolute),  7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(Absolute),  7),
            
            new INSTR("BEQ", new INSTR.OpDel(BEQ), new INSTR.AddrMode(Implied),   7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("JAM", new INSTR.OpDel(JAM), new INSTR.AddrMode(Implied),   7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(YIndirect), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("INC", new INSTR.OpDel(INC), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(XZeroPage),  7),
            new INSTR("SED", new INSTR.OpDel(SED), new INSTR.AddrMode(Implied),   7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(YAbsolute), 7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(Implied),   7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(YAbsolute),   7),
            new INSTR("NOP", new INSTR.OpDel(NOP), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("SBC", new INSTR.OpDel(SBC), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("INC", new INSTR.OpDel(INC), new INSTR.AddrMode(XAbsolute),  7),
            new INSTR("ISC", new INSTR.OpDel(ISC), new INSTR.AddrMode(XAbsolute),  7),
            
        };

        public CPU2(BUS bus)
        {
            _bus = bus;
        }

        void StepTO() { }


        private byte _opCode;
        public void Clock()
        {
            _opCode = _bus.Read(PC);
            PC++;
            _opCodes[0].Execute();
        }





        static void JAM()
        {
            while (true) { }
        }
        
        void ADC()
        {  }
        static void AND()
        {  }
        static void ASL()
        {  }
        static void BCC()
        {  }
        static void BCS()
        {  }
        static void BEQ()
        {  }
        static void BIT()
        {  }
        static void BMI()
        {  }
        static void BNE()
        {  }
         void BPL()
        {  }
         void BRK()
        {  }
         void BVC()
        {  }
         void BVS()
        {  }
         void CLC()
        {  }
         void CLI()
        {  }
         void CLV()
        {  }
         void CMP()
        {  }
         void CPX()
        {  }
         void CPY()
        {  }
         void DEC()
        {  }
         void DEX()
        {  }
         void DEY()
        {  }
         void EOR()
        {  }
         void INC()
        {  }
         void INX()
        {  }
         void INY()
        {  }
         void JMP()
        {  }
         void JSR()
        {  }
         void LDA()
        {  }
         void LDX()
        {  }
         void LDY()
        {  }
         void LSR()
        {  }
         void NOP()
        {  }
         void ORA()
        {  }
         void PHA()
        {  }
         void PHP()
        {  }
         void PLA()
        {  }
         void PLP()
        {  }
         void ROL()
        {  }
         void ROR()
        {  }
         void RTI()
        {  }
         void RTS()
        {  }
         void SBC()
        {  }
         void SEC()
        {  }
         void SED()
        {  }
         void SEI()
        {  }
         void STA()
        {  }
         void STX()
        {  }
         void STY()
        {  }
         void TAX()
        {  }
         void TAY()
        {  }
         void TSX()
        {  }
         void TXS()
        {  }
         void TYA()
        {  }
         void SLO()
        {  }
         void ANC()
        {  }
         void RLA()
        {  }
         void SRE()
        {  }
         void ALR()
        {  }
         void RRA()
        {  }
         void ARR()
        {  }
        static void SAX()
        {  }
        static void TXA()
        {  }
        static void ANE()
        {  }
        static void SHA()
        {  }
        static void TAS()
        {  }
        static void ISC()
        {  }
        static void USBC()
        {  }
        static void DCP()
        {   }
        static void LAX()
        {   }
        static void SHY()
        {   }
        static void SHX()
        {   }
        static void LXA()
        {   }
        static void LAS()
        {   }
        static void SBX()
        {   }
        static void CLD()
        {   }


       static void Immediate()
        { }
       static void XIndirect()
        { }
       static void YIndirect()
        { }
       static void Absolute()
        { }
       static void XAbsolute()
        { }
       static void YAbsolute()
        { }
       static void Implied()
        { }
       static void Indirect()
        { }
       static void Relative()
        { }
       static void ZeroPage()
        { }
       static void XZeroPage()
        { }
       static void YZeroPage()
        { }

    }


    internal class INSTR
    {
        //Possibly override ToString here to print out for the disassembler

        public string Name;
        public delegate void OpDel();
        public delegate void AddrMode();
        public int ClockCycles;
        OpDel _opDel;
        AddrMode _addrMode;
        public INSTR(string name, OpDel opDel , AddrMode addrMode, int clockCycles)
        {
            Name = name;
            ClockCycles = clockCycles;
            _opDel = opDel;
            _addrMode = addrMode;

        }

        public void Execute()
        {
            _addrMode.Invoke();
            _opDel.Invoke();
        }
    }


}