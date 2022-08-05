namespace SixtyFiveOhTwo.CPUCore;
public partial class CPU
{
    void JAM() { _isHalted = true; }
    void ADC()
    {
        // As The Prodigy once said: This is dangerous.
        //stop it patrick you're scaring him ^~&|^&~&^|()(|&^)
        var carry = C ? 1 : 0; //is the carry flag set
        var sum = A + fetchedByte + carry; //sum the Accum+operand+carry(if set)
        C = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        V = (~(A ^ fetchedByte) & (A ^ sum) & 0x80) != 0 ? true : false;
        A = (byte)sum;
        AccumChanged();
    }
    void AND()
    {
        A = (byte)(A & fetchedByte);
        SetZeroAndNegFlag(A);
    }
    void ASL()
    {
        //the byte that is passed is the calculated value from the position of the next byte/s in memory 
        //e.g. PeekNextByte(PC).. The problem is we have moved the PC already in the address mode method, so now we cannot
        //be sure where the actual POSITION that this value needs to be stored in.
        //we are calculating the correct value, we just lost where it belongs.

        //I think the only solution is for ASL to have a second argument.
        //I hate this since it breaks the pattern.
        if (AccumMode)
        {
            if ((A & 128) != 0) C = true;
            else C = false;
            A = (byte)(A << 1);
            SetZeroAndNegFlag(A);
        }
        else
        {
            if ((fetchedByte & 128) != 0) C = true;
            else C = false;
            fetchedByte = (byte)(fetchedByte << 1);
            Write(fetchedAddress, fetchedByte); //TODO: how to write to memory, i think this is wrong
            SetZeroAndNegFlag(fetchedByte);
        }
    }
    void BCC()
    {
        Branch(!C);
    }
    void BCS()
    {
        Branch(C);
    }
    void BEQ()
    {
        Branch(Z);
    }
    void BIT()
    {
        //BIT sets the z flag as though the value in the address tested were anded together with the accum the n and v flags are set to match bits 7 and 6 respectively in the 
        //value store at the tested address
        var pos = fetchedByte;
        if ((A & pos) == 0x00) Z = true;
        else Z = false;
        if ((pos & 128) != 0) N = true;
        else N = false;
        if ((pos & 64) != 0) V = true;
        else V = false;


    }
    void BMI()
    {
        Branch(N);
    }
    void BNE()
    {
        Branch(!Z);
    }
    void BPL()
    {
        Branch(!N);
    }
    void BRK()
    {
        PC++;
        NMI();
    }
    void BVC()
    {
        Branch(!V);

    }
    void BVS()
    {
        Branch(V);
    }
    void CLC()
    {
        C = false;
    }
    void CLI()
    {
        D = false;
    }
    void CLV()
    {
        V = false;
    }
    void CMP()
    {
        var aa = fetchedByte;
        var bb = A;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            C = true;
            Z = false;
            N = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            C = false;
            Z = false;
            N = (cc & 128) != 0;
        }
        else
        {
            N = false;
            Z = true;
            C = true;
        }



    }
    void CPX()
    {

        var aa = fetchedByte;
        var bb = X;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            C = true;
            Z = false;
            N = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            C = false;
            Z = false;
            N = (cc & 128) != 0;
        }
        else
        {
            N = false;
            Z = true;
            C = true;
        }

    }
    void CPY()
    {
        var aa = fetchedByte;
        var bb = Y;
        if (bb > aa)
        {
            byte cc = (byte)(bb - aa);
            C = true;
            Z = false;
            N = (cc & 128) != 0;
        }
        else if (bb < aa)
        {
            byte cc = (byte)(bb - aa);
            C = false;
            Z = false;
            N = (cc & 128) != 0;
        }
        else
        {
            N = false;
            Z = true;
            C = true;
        }

    }
    void DEC()
    {
        fetchedByte--;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
    }
    void DEX()
    {
        X--;
        SetZeroAndNegFlag(X);
    }
    void DEY()
    {
        Y--;
        SetZeroAndNegFlag(Y);
    }
    void EOR()
    {
        A ^= fetchedByte;
        AccumChanged();
    }
    void INC()
    {
        fetchedByte++;
        Write(fetchedAddress, fetchedByte);
        SetZeroAndNegFlag(fetchedByte);
    }
    void INX()
    {
        X++;
        SetZeroAndNegFlag(X);
    }
    void INY()
    {
        Y++;
        SetZeroAndNegFlag(Y);
    }
    void JMP()
    {
        PC = fetchedAddress;
    }
    void JSR()
    {
        //Jump but save the location of the PC in the stack.
        var b = BitConverter.GetBytes((ushort)PC - 1);
        PushToStack(b[1]);
        PushToStack(b[0]);
        PC = fetchedAddress;
    }
    void LDA()
    {
        A = fetchedByte;
        AccumChanged();
    }
    void LDX()
    {
        X = fetchedByte;
        SetZeroAndNegFlag(X);
    }
    void LDY()
    {
        Y = fetchedByte;
        SetZeroAndNegFlag(Y);
    }
    void LSR()
    {
        if (AccumMode)
        {
            if ((A & 1) != 0) C = true;
            else C = false;
            A = (byte)(A >> 1);
            SetZeroAndNegFlag(A);
        }
        if ((fetchedByte & 1) != 0) C = true;
        else C = false;
        fetchedByte = (byte)(fetchedByte >> 1);
        SetZeroAndNegFlag(fetchedByte);
        Write(fetchedAddress, fetchedByte);
    }
    void NOP() { /*This method intentionally left blank*/ }
    void ORA()
    {
        A |= fetchedByte;
        SetZeroAndNegFlag(A);
    }
    void PHA()
    {
        PushToStack(A);
    }
    void PHP()
    {
        bool[] flags = new bool[8] { C, Z, I, D, true, true, V, N };
        byte range = 0;
        for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
        PushToStack(range);
    }
    void PLA()
    {
        A = PopFromStack();
        SetZeroAndNegFlag(A);
    }
    void PLP()
    {
        var status = PopFromStack();
        C = (status & 1) != 0;
        Z = (status & 2) != 0;
        I = (status & 4) != 0;
        D = (status & 8) != 0;
        //Flags.BreakCommandFlag = (status & 16) != 0;
        //Flags.nullFlag = (status & 32) != 0;
        V = (status & 64) != 0;
        N = (status & 128) != 0;
    }
    void ROL()
    {
        if (AccumMode)
        {
            byte bit0;
            if (C) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            C = (A & 128) != 0 ? true : false;
            A = (byte)(A << 1);
            A = (byte)(A | bit0);
            SetZeroAndNegFlag((byte)(A | bit0));
        }
        else
        {
            byte bit0;
            if (C) bit0 = 1;
            else bit0 = 0;
            //check bit7 to see what the new carry flag chould be
            C = (fetchedByte & 128) != 0 ? true : false;
            fetchedByte = (byte)(fetchedByte << 1); //shift the accum left 1
            Write(fetchedAddress, (byte)(fetchedByte | bit0));
            SetZeroAndNegFlag((byte)(fetchedByte | bit0));
        }


    }
    void ROR()
    {
        byte bit7;
        if (AccumMode)
        {
            // carry slots into bit7 and bit 0 is shifted into the carry
            if (C) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            C = (A & 1) != 0 ? true : false;
            var shiftedAccum = (byte)(A >> 1); //shift the accum right 1
            A = (byte)(shiftedAccum | bit7);
            AccumChanged();
        }
        else
        {
            if (C) bit7 = 1;
            else bit7 = 0;
            bit7 = (byte)(bit7 << 7);
            //check bit0 to see what the new carry flag chould be
            C = (fetchedByte & 1) != 0 ? true : false;
            fetchedByte = (byte)(fetchedByte >> 1); //shift the accum right 1
            Write(fetchedAddress, (byte)(fetchedByte | bit7));
            SetZeroAndNegFlag((byte)(fetchedByte | bit7));
        }
    }
    void RTI()
    {
        var status = PopFromStack();
        C = (status & 1) != 0;
        Z = (status & 2) != 0;
        I = (status & 4) != 0;
        D = (status & 8) != 0;
        V = (status & 64) != 0;
        N = (status & 128) != 0;
        var PC1 = PopFromStack();
        var PC2 = PopFromStack();
        PC = (ushort)(PC2 << 8 | PC1);

    }
    void RTS()
    {
        var addr2 = PopFromStack();
        var addr1 = PopFromStack();
        PC = (ushort)((addr1 << 8 | addr2) + 0x0001);
    }
    void SBC()
    {
        byte op = (byte)~fetchedByte;

        var carry = C ? 1 : 0; //is the carry flag set
        var sum = A + op + carry; //sum the Accum+operand+carry(if set)
        C = sum > 0xFF ? true : false; //set/clear the carry based on the result.
        V = (~(A ^ op) & (A ^ sum) & 0x80) != 0 ? true : false;//same as addition but with negated operand
        A = (byte)sum; //set accumulator
        AccumChanged();
    }
    void SEC()
    {
        C = true;
    }
    void SED()
    {
        D = true;
    }
    void SEI()
    {
        I = true;
    }
    void STA()
    {
        Write(fetchedAddress, A);

    }
    void STX()
    {
        Write(fetchedAddress, X);

    }
    void STY()
    {
        Write(fetchedAddress, Y);

    }
    void TAX()
    {
        X = A;
        SetZeroAndNegFlag(X);

    }
    void TAY()
    {
        Y = A;
        SetZeroAndNegFlag(Y);
    }
    void TSX()
    {
        X = SP;
        SetZeroAndNegFlag(X);
    }
    void TXS()
    {
        SP = X;
    }
    void TYA()
    {
        A = Y;
        SetZeroAndNegFlag(A);
    }
    void SLO()
    {
        ASL();
        A |= Read(fetchedAddress);
        SetZeroAndNegFlag(A);
    }
    void RLA()
    {//TODO this is probably not going to work
     //HACK: we need to acces the NEW fetched byte that was modified by the first part of the instruction
     //so we read from the address again and overwrite the fetchedByte
        ROL();
        fetchedByte = Read(fetchedAddress);
        AND();
    }
    void SRE()
    {//TODO: This also is probably not going to work
        LSR();
        fetchedByte = Read(fetchedAddress);
        EOR();
        //SetZeroAndNegFlag(fetchedByte);
    }
    void RRA()
    {
        ROR();
        fetchedByte = Read(fetchedAddress);
        ADC();
    }
    void SAX()
    {
        Write(fetchedAddress, (byte)(A & X));
    }
    void TXA()
    {
        A = X;
        SetZeroAndNegFlag(A);
    }
    void ISC()
    {
        INC();
        SBC();
    }
    void USB()
    {
        SBC();
        NOP();
    }
    void DCP()
    {
        DEC();
        CMP();
    }
    void LAX()
    {
        LDA();
        LDX();
    }
    void SBX()
    {
        CMP();
        DEX();
        SetZeroAndNegFlag(fetchedByte);
    }
    void CLD()
    {
        D = false;
    }
    #region Unimplemented Instructions
    //So far I have not encountered this instructions. 
    void ANC() { throw new NotImplementedException(); }
    void ALR() { throw new NotImplementedException(); }
    void ARR() { throw new NotImplementedException(); }
    void ANE() { throw new NotImplementedException(); }
    void SHA() { throw new NotImplementedException(); }
    void TAS() { throw new NotImplementedException(); }
    void SHY() { throw new NotImplementedException(); }
    void SHX() { throw new NotImplementedException(); }
    void LXA() { throw new NotImplementedException(); }
    void LAS() { throw new NotImplementedException(); }
    #endregion
    #region Addressing Modes
    void IMM() //Immediate
    {
        PC++;
        fetchedAddress = PC;
        fetchedByte = Read(PC);
        PC++;
    }
    void XIN() //X IND
    {
        //operand is a zero page address

        PC++;
        var indexByte = Read(PC);
        var newPos = (byte)(indexByte + X);
        var calcedPos = Read(newPos);
        var calcedPos2 = Read((byte)(newPos + 1));
        ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
    }
    void YIN() //Y IND
    {
        //Differing from x Indirect, the order is a little different and there
        //is a carry
        PC++;
        byte indexByte = Read(PC);
        byte b1 = Read(indexByte);
        byte b2 = Read((byte)(indexByte + 1));
        if (indexByte == 0xFF) b2++;
        ushort addr = 0x00;
        addr = (ushort)(b2 << 8 | b1);

        if (Y == 0xFF && indexByte != 0xFF)
        {
            addr += 0x100;
            addr--;
        }
        else if (Y == 0xFF && indexByte == 0xFF)
        {
            addr--;
        }
        else
        {
            addr += Y;
        }

        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;


        //byte indexByte = PeekNextByte();
        //byte b1 = Memory[indexByte];
        //byte b2 = Memory[indexByte + 1];
        //if (indexByte == 0xFF) b2++;
        //ushort tempShort = 0x00;
        //tempShort = (ushort)(b2 << 8 | b1);
        //tempShort += RegisterY;
        //curMemLocation = tempShort;
        //return Memory[curMemLocation];


    }
    void ABS() //Absolute
    {
        //get the high and low bytes for the address and build a short;
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        PC++;
        fetchedAddress = addr;
        fetchedByte = Read(addr);
    }
    void XAB() //X Absolute
    {
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + X);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
    }
    void YAB() //Y Absolute
    {
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + Y);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
    }
    void IMP() //Implied
    {
        //TODO: don;t think this is right
        PC++;
        AccumMode = true;
    }
    void IND() //Indirect
    {

        //From my understanding it's:
        //read the immediate bytes
        //go to the location from those bytes
        //return the value from the location calculated from the original immediate calculation

        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        if ((addr & 0x00FF) == 0xFF)
        {
            fetchedAddress = addr += 1;
            fetchedByte = Read(addr += 1);

        }
        else
        {
            var b1 = Read(addr);
            var b2 = Read((ushort)(addr + 1));
            var calcedLocation = (ushort)(b2 << 8 | b1);

            fetchedAddress = calcedLocation;
            fetchedByte = Read(calcedLocation);

        }
        PC++;


        //curMemLocation = SwapNextTwoBytes();
        //if ((curMemLocation & 0x00FF) == 0xFF)
        //{
        //    return curMemLocation += 1;
        //}
        //var b1 = Memory[curMemLocation];
        //var b2 = Memory[curMemLocation + 1];
        //var calcedLocation = (ushort)(b2 << 8 | b1);

        //return calcedLocation;

    }
    void REL() /*relative*/ { /*Dumb me I don't remember why there is nothing here */ }
    void ZPG() //Zero Page
    {
        PC++;
        fetchedAddress = Read(PC);
        fetchedByte = Read(fetchedAddress);
        PC++;
    }
    void XZP() //X Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + X);
        fetchedByte = Read(fetchedAddress);
        PC++;
    }
    void YZP() //Y Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + Y);
        fetchedByte = Read(fetchedAddress);
        PC++;
    }
    #endregion
}
