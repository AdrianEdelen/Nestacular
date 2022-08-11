using SixtyFiveOhTwo.Exceptions;
namespace SixtyFiveOhTwo;
public partial class CPU
{
    #region current Fully Functioning instructions Incl. Cycle Accuracy.
    //not including page crossing or other conditional cycle count changes.
    int ADC(AddressModes addr)
    {
        // As The Prodigy once said: This is dangerous.
        //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
        var carry = _carryFlag ? 1 : 0; //is the carry flag set
        var sum = _registers.A + fetchedByte + carry; //sum the Accum+operand+carry(if set)
        _carryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        _overflowFlag = (~(_registers.A ^ fetchedByte) & (_registers.A ^ sum) & 0x80) != 0 ? true : false; //what the fuck
        _registers.A = (byte)sum;
        AccumChanged();
        return AddClockCyclesStandard(addr);
    }
    int AND(AddressModes addr)
    {
        _registers.A = (byte)(_registers.A & fetchedByte);
        SetZeroAndNegFlag(_registers.A);
        return AddClockCyclesStandard(addr);
    }
    int ASL(AddressModes addr)
    {
        if (addr == AddressModes.Implied)
        {
            if ((_registers.A & 128) != 0) _carryFlag = true;
            else _carryFlag = false;
            _registers.A = (byte)(_registers.A << 1);
            SetZeroAndNegFlag(_registers.A);
        }
        else
        {
            if ((fetchedByte & 128) != 0) _carryFlag = true;
            else _carryFlag = false;
            fetchedByte = (byte)(fetchedByte << 1);
            Write(fetchedAddress, fetchedByte); 
            SetZeroAndNegFlag(fetchedByte);
        }
        return addr switch
        {
            AddressModes.Accumulator => 2,
            AddressModes.ZeroPage => 5,
            AddressModes.XZeroPage => 6,
            AddressModes.Absolute => 6,
            AddressModes.XAbsolute => 7,
            _ => throw new InvalidAddressingModeException()
        };
    }
    int BIT(AddressModes addr)
    {
        //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
        //value store at the tested address
        var pos = fetchedByte;
        if ((_registers.A & pos) == 0x00) _zeroFlag = true;
        else _zeroFlag = false;
        if ((pos & 128) != 0) _negativeFlag = true;
        else _negativeFlag = false;
        if ((pos & 64) != 0) _overflowFlag = true;
        else _overflowFlag = false;
        return addr switch
        {
            AddressModes.ZeroPage => 3,
            AddressModes.Absolute => 4,
            _ => throw new InvalidAddressingModeException()
        };
    }
    int CMP(AddressModes addr)
    {
        var aa = fetchedByte;
        var bb = _registers.A;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = true;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = false;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else
        {
            _negativeFlag = false;
            _zeroFlag = true;
            _carryFlag = true;
        }
        return AddClockCyclesStandard(addr);
    }
    #endregion
    #region in work
    int CPX(AddressModes addr)
    {
        var clockCycles = 0;
        var aa = fetchedByte;
        var bb = _registers.X;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = true;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = false;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else
        {
            _negativeFlag = false;
            _zeroFlag = true;
            _carryFlag = true;
        }
        return clockCycles;
    }
    #endregion

    #region Complex Instructions (can we improve them?)







    int CPY(AddressModes addr)
    {
        var clockCycles = 0;
        var aa = fetchedByte;
        var bb = _registers.Y;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = true;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            _carryFlag = false;
            _zeroFlag = false;
            _negativeFlag = (cc & 128) != 0;
        }
        else
        {
            _negativeFlag = false;
            _zeroFlag = true;
            _carryFlag = true;
        }
        return clockCycles;
    }
    int DEC(AddressModes addr)
    {
        var clockCycles = 0;
        fetchedByte--;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
        return clockCycles;
    }

    int INC(AddressModes addr)
    {
        var clockCycles = 0;
        fetchedByte++;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
        return clockCycles;
    }

    int JSR(AddressModes addr)
    {
        var clockCycles = 0;
        //Jump but save the location of the PC in the stack.
        var b = BitConverter.GetBytes((ushort)_registers.PC - 1);
        PushToStack(b[1]);
        PushToStack(b[0]);
        _registers.PC = fetchedAddress;
        return clockCycles;
    }

    int LSR(AddressModes addr)
    {
        var clockCycles = 0;
        if (AccumMode)
        {
            if ((_registers.A & 1) != 0) _carryFlag = true;
            else _carryFlag = false;
            _registers.A = (byte)(_registers.A >> 1);
            SetZeroAndNegFlag(_registers.A);
        }
        if ((fetchedByte & 1) != 0) _carryFlag = true;
        else _carryFlag = false;
        fetchedByte = (byte)(fetchedByte >> 1);
        SetZeroAndNegFlag(fetchedByte);
        Write(fetchedAddress, fetchedByte);
        return clockCycles;
    }

    int PHP(AddressModes addr)
    {
        var clockCycles = 0;
        bool[] flags = new bool[8] { _carryFlag, _zeroFlag, _interruptDisableFlag, _decimalModeFlag, true, true, _overflowFlag, _negativeFlag };
        byte range = 0;
        for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
        PushToStack(range);
        return clockCycles;
    }
    int PLP(AddressModes addr)
    {
        var clockCycles = 0;
        var status = PopFromStack();
        _carryFlag = (status & 1) != 0;
        _zeroFlag = (status & 2) != 0;
        _interruptDisableFlag = (status & 4) != 0;
        _decimalModeFlag = (status & 8) != 0;
        //TODO: why is this commented out (im assuming they are ignored here
        //Flags.BreakCommandFlag = (status & 16) != 0;
        //Flags.nullFlag = (status & 32) != 0;
        _overflowFlag = (status & 64) != 0;
        _negativeFlag = (status & 128) != 0;
        return clockCycles;
    }
    int ROL(AddressModes addr)
    {
        var clockCycles = 0;
        if (AccumMode)
        {
            byte bit0;
            if (_carryFlag) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            _carryFlag = (_registers.A & 128) != 0 ? true : false;
            _registers.A = (byte)(_registers.A << 1);
            _registers.A = (byte)(_registers.A | bit0);
            SetZeroAndNegFlag((byte)(_registers.A | bit0));
        }
        else
        {
            byte bit0;
            if (_carryFlag) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            _carryFlag = (fetchedByte & 128) != 0 ? true : false;
            fetchedByte = (byte)(fetchedByte << 1); //shift the accum left 1
            Write(fetchedAddress, (byte)(fetchedByte | bit0));
            SetZeroAndNegFlag((byte)(fetchedByte | bit0));
        }
        return clockCycles;
    }
    int ROR(AddressModes addr)
    {
        var clockCycles = 0;
        byte bit7;
        if (AccumMode)
        {
            // carry slots into bit7 and bit 0 is shifted into the carry
            if (_carryFlag) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            _carryFlag = (_registers.A & 1) != 0 ? true : false;
            var shiftedAccum = (byte)(_registers.A >> 1); //shift the accum right 1
            _registers.A = (byte)(shiftedAccum | bit7);
            AccumChanged();
        }
        else
        {
            if (_carryFlag) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            _carryFlag = (fetchedByte & 1) != 0 ? true : false;
            fetchedByte = (byte)(fetchedByte >> 1); //shift the accum right 1
            Write(fetchedAddress, (byte)(fetchedByte | bit7));
            SetZeroAndNegFlag((byte)(fetchedByte | bit7));
        }
        return clockCycles;
    }
    int RTI(AddressModes addr)
    {
        var clockCycles = 0;
        var status = PopFromStack();
        _carryFlag = (status & 1) != 0;
        _zeroFlag = (status & 2) != 0;
        _interruptDisableFlag = (status & 4) != 0;
        _decimalModeFlag = (status & 8) != 0;
        _overflowFlag = (status & 64) != 0;
        _negativeFlag = (status & 128) != 0;
        var PC1 = PopFromStack();
        var PC2 = PopFromStack();
        _registers.PC = (ushort)(PC2 << 8 | PC1);
        return clockCycles;
    }
    int RTS(AddressModes addr)
    {
        var clockCycles = 0;
        var addr2 = PopFromStack();
        var addr1 = PopFromStack();
        _registers.PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
        return clockCycles;
    }
    int SBC(AddressModes addr)
    {
        var clockCycles = 0;
        byte op = (byte)~fetchedByte;

        var carry = _carryFlag ? 1 : 0; //is the carry flag set
        var sum = _registers.A + op + carry; //sum the Accum+operand+carry(if set)
        _carryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        _overflowFlag = (~(_registers.A ^ op) & (_registers.A ^ sum) & 0x80) != 0 ? true : false;//same as addition but with negated operand
        _registers.A = (byte)sum; //set accumulator
        AccumChanged();
        return clockCycles;
    }
    #endregion
    #region One Line / Simple Instructions
    int PLA(AddressModes addr) { var clockCycles = 0; _registers.A = PopFromStack(); SetZeroAndNegFlag(_registers.A); return clockCycles; }
    int STY(AddressModes addr) { var clockCycles = 0; Write(fetchedAddress, _registers.Y); return clockCycles; }
    int TAX(AddressModes addr) { var clockCycles = 0; _registers.X = _registers.A; SetZeroAndNegFlag(_registers.X); return clockCycles; }
    int TAY(AddressModes addr) { var clockCycles = 0; _registers.Y = _registers.A; SetZeroAndNegFlag(_registers.Y); return clockCycles; }
    int TSX(AddressModes addr) { var clockCycles = 0; _registers.X = _registers.SP; SetZeroAndNegFlag(_registers.X); return clockCycles; }
    int TXS(AddressModes addr) { var clockCycles = 0; _registers.SP = _registers.X; return clockCycles; }
    int TYA(AddressModes addr) { var clockCycles = 0; _registers.A = _registers.Y; SetZeroAndNegFlag(_registers.A); return clockCycles; }
    int SLO(AddressModes addr) { var clockCycles = 0; ASL(addr); _registers.A |= Read(fetchedAddress); SetZeroAndNegFlag(_registers.A); return clockCycles; }
    int INX(AddressModes addr) { var clockCycles = 0; _registers.X++; SetZeroAndNegFlag(_registers.X); return clockCycles; }
    int INY(AddressModes addr) { var clockCycles = 0; _registers.Y++; SetZeroAndNegFlag(_registers.Y); return clockCycles; }
    int JMP(AddressModes addr) { var clockCycles = 0; _registers.PC = fetchedAddress; return clockCycles; }
    int NOP(AddressModes addr) { var clockCycles = 0; /*This method intentionally left blank*/ return clockCycles; }
    int ORA(AddressModes addr) { var clockCycles = 0; _registers.A |= fetchedByte; SetZeroAndNegFlag(_registers.A); return clockCycles; }
    int PHA(AddressModes addr) { var clockCycles = 0; PushToStack(_registers.A); return clockCycles; }
    int LDA(AddressModes addr) { var clockCycles = 0; _registers.A = fetchedByte; AccumChanged(); return clockCycles; }
    int LDX(AddressModes addr) { var clockCycles = 0; _registers.X = fetchedByte; SetZeroAndNegFlag(_registers.X); return clockCycles; }
    int LDY(AddressModes addr) { var clockCycles = 0; _registers.Y = fetchedByte; SetZeroAndNegFlag(_registers.Y); return clockCycles; }
    int DEX(AddressModes addr) { var clockCycles = 0; _registers.X--; SetZeroAndNegFlag(_registers.X); return clockCycles; }
    int DEY(AddressModes addr) { var clockCycles = 0; _registers.Y--; SetZeroAndNegFlag(_registers.Y); return clockCycles; }
    int EOR(AddressModes addr) { var clockCycles = 0; _registers.A ^= fetchedByte; AccumChanged(); return clockCycles; }
    int JAM(AddressModes addr) { throw new CPUHaltedException($"JAM opcode called. CPU Status: {Status} | InstructionStatus: {InstructionStatus}"); }
    int BMI(AddressModes addr) { var clockCycles = 0; Branch(_negativeFlag); return clockCycles; }
    int BNE(AddressModes addr) { var clockCycles = 0; Branch(!_zeroFlag); return clockCycles; }
    int BPL(AddressModes addr) { var clockCycles = 0; Branch(!_negativeFlag); return clockCycles; }
    int BRK(AddressModes addr) { var clockCycles = 0; _registers.PC++; NMI(); return clockCycles; }
    int BVC(AddressModes addr) { var clockCycles = 0; Branch(!_overflowFlag); return clockCycles; }
    int BVS(AddressModes addr) { var clockCycles = 0; Branch(_overflowFlag); return clockCycles; }
    int CLC(AddressModes addr) { var clockCycles = 0; _carryFlag = false; return clockCycles; }
    int CLI(AddressModes addr) { var clockCycles = 0; _decimalModeFlag = false; return clockCycles; }
    int CLV(AddressModes addr) { var clockCycles = 0; _overflowFlag = false; return clockCycles; }
    int BCC(AddressModes addr) { var clockCycles = 0; Branch(!_carryFlag); return clockCycles; }
    int BCS(AddressModes addr) { var clockCycles = 0; Branch(_carryFlag); return clockCycles; }
    int BEQ(AddressModes addr) { var clockCycles = 0; Branch(_zeroFlag); return clockCycles; }
    int SEC(AddressModes addr) { var clockCycles = 0; _carryFlag = true; return clockCycles; }
    int SED(AddressModes addr) { var clockCycles = 0; _decimalModeFlag = true; return clockCycles; }
    int SEI(AddressModes addr) { var clockCycles = 0; _interruptDisableFlag = true; return clockCycles; }
    int STA(AddressModes addr) { var clockCycles = 0; Write(fetchedAddress, _registers.A); return clockCycles; }
    int STX(AddressModes addr) { var clockCycles = 0; Write(fetchedAddress, _registers.X); return clockCycles; }
    int RLA(AddressModes addr) { var clockCycles = 0; ROL(addr); fetchedByte = Read(fetchedAddress); AND(addr); return clockCycles; } //TODO: Verify this works.
    int SRE(AddressModes addr) { var clockCycles = 0; LSR(addr); fetchedByte = Read(fetchedAddress); EOR(addr); return clockCycles; } //TODO: Verify This works.
    int RRA(AddressModes addr) { var clockCycles = 0; ROR(addr); fetchedByte = Read(fetchedAddress); ADC(addr); return clockCycles; }
    int SAX(AddressModes addr) { var clockCycles = 0; Write(fetchedAddress, (byte)(_registers.A & _registers.X)); return clockCycles; }
    int TXA(AddressModes addr) { var clockCycles = 0; _registers.A = _registers.X; SetZeroAndNegFlag(_registers.A); return clockCycles; }
    int ISC(AddressModes addr) { var clockCycles = 0; INC(addr); SBC(addr); return clockCycles; }
    int USB(AddressModes addr) { var clockCycles = 0; SBC(addr); NOP(addr); return clockCycles; } //TODO: Why is this not referenced in the opcode table?
    int DCP(AddressModes addr) { var clockCycles = 0; DEC(addr); CMP(addr); return clockCycles; }
    int LAX(AddressModes addr) { var clockCycles = 0; LDA(addr); LDX(addr); return clockCycles; }
    int SBX(AddressModes addr) { var clockCycles = 0; CMP(addr); DEX(addr); SetZeroAndNegFlag(fetchedByte); return clockCycles; }
    int CLD(AddressModes addr) { var clockCycles = 0; _decimalModeFlag = false; return clockCycles; }
    #endregion
    #region Unimplemented Instructions
    //So far I have not encountered these instructions. 
    int ANC(AddressModes addr) { throw new NotImplementedException(); }
    int ALR(AddressModes addr) { throw new NotImplementedException(); }
    int ARR(AddressModes addr) { throw new NotImplementedException(); }
    int ANE(AddressModes addr) { throw new NotImplementedException(); }
    int SHA(AddressModes addr) { throw new NotImplementedException(); }
    int TAS(AddressModes addr) { throw new NotImplementedException(); }
    int SHY(AddressModes addr) { throw new NotImplementedException(); }
    int SHX(AddressModes addr) { throw new NotImplementedException(); }
    int LXA(AddressModes addr) { throw new NotImplementedException(); }
    int LAS(AddressModes addr) { throw new NotImplementedException(); }

    #endregion


    private int AddClockCyclesStandard(AddressModes addr)
    {
        var retVal = 0;
        if (didBranch) retVal += CheckBranchSamePage();
        retVal += CheckPageCross();
        return retVal += addr switch
        {
            AddressModes.Immediate => 2,
            AddressModes.ZeroPage => 3,
            AddressModes.XZeroPage => 4,
            AddressModes.Absolute => 4,
            AddressModes.XAbsolute => 4,
            AddressModes.YAbsolute => 4,
            AddressModes.XIndirect => 6,
            AddressModes.YIndirect => 5,
            _ => throw new InvalidAddressingModeException()
        };
    }
    private int CheckPageCross()
    {
        //TODO: Implement this
        return 0;
    }
    private int CheckBranchSamePage()
    {
        bool branchOnSamePage = false;
        //TODO: Determine is the branch occurs on the same page;
        if (branchOnSamePage) return 1;
        else return 2;
    }

}
