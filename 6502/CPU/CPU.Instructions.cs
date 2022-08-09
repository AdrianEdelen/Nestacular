using SixtyFiveOhTwo.Exceptions;
namespace SixtyFiveOhTwo;
public partial class CPU
{
    #region current Fully Functioning instructions
    int ADC()
    {
        // As The Prodigy once said: This is dangerous.
        //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
        var carry = _carryFlag ? 1 : 0; //is the carry flag set
        var sum = A + fetchedByte + carry; //sum the Accum+operand+carry(if set)
        _carryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        _overflowFlag = (~(A ^ fetchedByte) & (A ^ sum) & 0x80) != 0 ? true : false; //what the fuck
        A = (byte)sum;
        AccumChanged();
        return AddClockCyclesStandard();
    }
    int AND()
    {
        A = (byte)(A & fetchedByte);
        SetZeroAndNegFlag(A);
        return AddClockCyclesStandard();
    }

    #region in work

    #endregion





    #region Complex Instructions (can we improve them?)


    int ASL()
    {
        if (_currentAddressMode == AddressingModes.Implied)
        {
            if ((A & 128) != 0) _carryFlag = true;
            else _carryFlag = false;
            A = (byte)(A << 1);
            SetZeroAndNegFlag(A);
        }
        else
        {
            if ((fetchedByte & 128) != 0) _carryFlag = true;
            else _carryFlag = false;
            fetchedByte = (byte)(fetchedByte << 1);
            Write(fetchedAddress, fetchedByte); //TODO: how to write to memory, i think this is wrong
            SetZeroAndNegFlag(fetchedByte);
        }
        return _currentAddressMode switch
        {
            AddressingModes.Implied => 
        };
    }
    int BIT()
    {
        //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
        //value store at the tested address
        var clockCycles = 0;
        var pos = fetchedByte;
        if ((A & pos) == 0x00) _zeroFlag = true;
        else _zeroFlag = false;
        if ((pos & 128) != 0) _negativeFlag = true;
        else _negativeFlag = false;
        if ((pos & 64) != 0) _overflowFlag = true;
        else _overflowFlag = false;
        return clockCycles;
    }
    
    int CMP()
    {
        var clockCycles = 0;
        var aa = fetchedByte;
        var bb = A;
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
    int CPX()
    {
        var clockCycles = 0;
        var aa = fetchedByte;
        var bb = X;
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
    int CPY()
    {
        var clockCycles = 0;
        var aa = fetchedByte;
        var bb = Y;
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
    int DEC()
    {
        var clockCycles = 0;
        fetchedByte--;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
        return clockCycles;
    }
    
    int INC()
    {
        var clockCycles = 0;
        fetchedByte++;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
        return clockCycles;
    }
    
    int JSR()
    {
        var clockCycles = 0;
        //Jump but save the location of the PC in the stack.
        var b = BitConverter.GetBytes((ushort)PC - 1);
        PushToStack(b[1]);
        PushToStack(b[0]);
        PC = fetchedAddress;
        return clockCycles;
    }
    
    int LSR()
    {
        var clockCycles = 0;
        if (AccumMode)
        {
            if ((A & 1) != 0) _carryFlag = true;
            else _carryFlag = false;
            A = (byte)(A >> 1);
            SetZeroAndNegFlag(A);
        }
        if ((fetchedByte & 1) != 0) _carryFlag = true;
        else _carryFlag = false;
        fetchedByte = (byte)(fetchedByte >> 1);
        SetZeroAndNegFlag(fetchedByte);
        Write(fetchedAddress, fetchedByte);
        return clockCycles;
    }
    
    int PHP()
    {
        var clockCycles = 0;
        bool[] flags = new bool[8] { _carryFlag, _zeroFlag, _interruptDisableFlag, _decimalModeFlag, true, true, _overflowFlag, _negativeFlag };
        byte range = 0;
        for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
        PushToStack(range);
        return clockCycles;
    }
    int PLP()
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
    int ROL()
    {
        var clockCycles = 0;
        if (AccumMode)
        {
            byte bit0;
            if (_carryFlag) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            _carryFlag = (A & 128) != 0 ? true : false;
            A = (byte)(A << 1);
            A = (byte)(A | bit0);
            SetZeroAndNegFlag((byte)(A | bit0));
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
    int ROR()
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
            _carryFlag = (A & 1) != 0 ? true : false;
            var shiftedAccum = (byte)(A >> 1); //shift the accum right 1
            A = (byte)(shiftedAccum | bit7);
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
    int RTI()
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
        PC = (ushort)(PC2 << 8 | PC1);
        return clockCycles;
    }
    int RTS()
    {
        var clockCycles = 0;
        var addr2 = PopFromStack();
        var addr1 = PopFromStack();
        PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
        return clockCycles;
    }
    int SBC()
    {
        var clockCycles = 0;
        byte op = (byte)~fetchedByte;

        var carry = _carryFlag ? 1 : 0; //is the carry flag set
        var sum = A + op + carry; //sum the Accum+operand+carry(if set)
        _carryFlag = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        _overflowFlag = (~(A ^ op) & (A ^ sum) & 0x80) != 0 ? true : false;//same as addition but with negated operand
        A = (byte)sum; //set accumulator
        AccumChanged();
        return clockCycles;
    }
    #endregion
    #region One Line / Simple Instructions
    int PLA() { var clockCycles = 0; A = PopFromStack(); SetZeroAndNegFlag(A); return clockCycles; }
    int STY() { var clockCycles = 0; Write(fetchedAddress, Y); return clockCycles; }
    int TAX() { var clockCycles = 0; X = A; SetZeroAndNegFlag(X); return clockCycles; }
    int TAY() { var clockCycles = 0; Y = A; SetZeroAndNegFlag(Y); return clockCycles; }
    int TSX() { var clockCycles = 0; X = SP; SetZeroAndNegFlag(X); return clockCycles; }
    int TXS() { var clockCycles = 0; SP = X; return clockCycles; }
    int TYA() { var clockCycles = 0; A = Y; SetZeroAndNegFlag(A); return clockCycles; }
    int SLO() { var clockCycles = 0; ASL(); A |= Read(fetchedAddress); SetZeroAndNegFlag(A); return clockCycles; }
    int INX() { var clockCycles = 0; X++; SetZeroAndNegFlag(X); return clockCycles; }
    int INY() { var clockCycles = 0; Y++; SetZeroAndNegFlag(Y); return clockCycles; }
    int JMP() { var clockCycles = 0; PC = fetchedAddress; return clockCycles; }
    int NOP() { var clockCycles = 0; /*This method intentionally left blank*/ return clockCycles; }
    int ORA() { var clockCycles = 0; A |= fetchedByte; SetZeroAndNegFlag(A); return clockCycles; }
    int PHA() { var clockCycles = 0; PushToStack(A); return clockCycles; }
    int LDA() { var clockCycles = 0; A = fetchedByte; AccumChanged(); return clockCycles; }
    int LDX() { var clockCycles = 0; X = fetchedByte; SetZeroAndNegFlag(X); return clockCycles; }
    int LDY() { var clockCycles = 0; Y = fetchedByte; SetZeroAndNegFlag(Y); return clockCycles; }
    int DEX() { var clockCycles = 0; X--; SetZeroAndNegFlag(X); return clockCycles; }
    int DEY() { var clockCycles = 0; Y--; SetZeroAndNegFlag(Y); return clockCycles; }
    int EOR() { var clockCycles = 0; A ^= fetchedByte; AccumChanged(); return clockCycles; }
    int JAM() { throw new CPUHaltedException($"JAM opcode called. CPU Status: {Status} | InstructionStatus: {InstructionStatus}"); }
    int BMI() { var clockCycles = 0; Branch(_negativeFlag); return clockCycles; }
    int BNE() { var clockCycles = 0; Branch(!_zeroFlag); return clockCycles; }
    int BPL() { var clockCycles = 0; Branch(!_negativeFlag); return clockCycles; }
    int BRK() { var clockCycles = 0; PC++; NMI(); return clockCycles; }
    int BVC() { var clockCycles = 0; Branch(!_overflowFlag); return clockCycles; }
    int BVS() { var clockCycles = 0; Branch(_overflowFlag); return clockCycles; }
    int CLC() { var clockCycles = 0; _carryFlag = false; return clockCycles; }
    int CLI() { var clockCycles = 0; _decimalModeFlag = false; return clockCycles; }
    int CLV() { var clockCycles = 0; _overflowFlag = false; return clockCycles; }
   
    int BCC() { var clockCycles = 0; Branch(!_carryFlag); return clockCycles; }
    int BCS() { var clockCycles = 0; Branch(_carryFlag); return clockCycles; }
    int BEQ() { var clockCycles = 0; Branch(_zeroFlag); return clockCycles; }
    int SEC() { var clockCycles = 0; _carryFlag = true; return clockCycles; }
    int SED() { var clockCycles = 0; _decimalModeFlag = true; return clockCycles; }
    int SEI() { var clockCycles = 0; _interruptDisableFlag = true; return clockCycles; }
    int STA() { var clockCycles = 0; Write(fetchedAddress, A); return clockCycles; }
    int STX() { var clockCycles = 0; Write(fetchedAddress, X); return clockCycles; }
    int RLA() { var clockCycles = 0; ROL(); fetchedByte = Read(fetchedAddress); AND(); return clockCycles; } //TODO: Verify this works.
    int SRE() { var clockCycles = 0; LSR(); fetchedByte = Read(fetchedAddress); EOR(); return clockCycles; } //TODO: Verify This works.
    int RRA() { var clockCycles = 0; ROR(); fetchedByte = Read(fetchedAddress); ADC(); return clockCycles; }
    int SAX() { var clockCycles = 0; Write(fetchedAddress, (byte)(A & X)); return clockCycles; }
    int TXA() { var clockCycles = 0; A = X; SetZeroAndNegFlag(A); return clockCycles; }
    int ISC() { var clockCycles = 0; INC(); SBC(); return clockCycles; }
    int USB() { var clockCycles = 0; SBC(); NOP(); return clockCycles; } //TODO: Why is this not referenced in the opcode table?
    int DCP() { var clockCycles = 0; DEC(); CMP(); return clockCycles; }
    int LAX() { var clockCycles = 0; LDA(); LDX(); return clockCycles; }
    int SBX() { var clockCycles = 0; CMP(); DEX(); SetZeroAndNegFlag(fetchedByte); return clockCycles; }
    int CLD() { var clockCycles = 0; _decimalModeFlag = false; return clockCycles; }
    #endregion
    #region Unimplemented Instructions
    //So far I have not encountered these instructions. 
    int ANC() { throw new NotImplementedException(); }
    int ALR() { throw new NotImplementedException(); }
    int ARR() { throw new NotImplementedException(); }
    int ANE() { throw new NotImplementedException(); }
    int SHA() { throw new NotImplementedException(); }
    int TAS() { throw new NotImplementedException(); }
    int SHY() { throw new NotImplementedException(); }
    int SHX() { throw new NotImplementedException(); }
    int LXA() { throw new NotImplementedException(); }
    int LAS() { throw new NotImplementedException(); }

    #endregion


    private int AddClockCyclesStandard()
    {
        var retVal = CheckPageCross();
        return retVal += _currentAddressMode switch
        {
            AddressingModes.Immediate => 2,
            AddressingModes.ZeroPage => 3,
            AddressingModes.XZeroPage => 4,
            AddressingModes.Absolute => 4,
            AddressingModes.XAbsolute => 4,
            AddressingModes.YAbsolute => 4,
            AddressingModes.XIndirect => 6,
            AddressingModes.YIndirect => 5,
            _ => throw new InvalidAddressingModeException()
        };
    }
    private int CheckPageCross()
    {
        //TODO: Implement this
        return 0;
    }
}
