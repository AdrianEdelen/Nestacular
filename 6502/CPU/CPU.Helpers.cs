namespace SixtyFiveOhTwo;
public partial class CPU
{
    //These Read and Write methods are just wrappers for legibility throughout the opCodes.
    private byte Read(ushort location) { return _bus.Read(location); }
    private void Write(ushort addr, byte value) { _bus.Write(addr, value); }

    //branching is done several times throughout various opcodes.
    //the branching behavior is always the same, only the condition that determines if the branch should happen.
    //TODO: Move this
    bool didBranch = false;
    private void Branch(bool DoBranch)
    {
        PC++; //this gets us to the operand
        var jumpDistance = Read(PC) + 1; //plus two to get over the operands.
        //determine if the jump is fwd or bwd.
        if (DoBranch && jumpDistance >= 0x80) PC -= (byte)(0xFF - jumpDistance + 1);
        else if (DoBranch) PC += ((byte)jumpDistance);
        else PC += 1;
        didBranch = true;

    }
    //wrappers for stack manipulation.
    private void PushToStack(byte value)
    {
        //if (value == 0x3A) Debugger.Break();
        ushort currentStackPosition = (ushort)(0x01 << 8 | SP);
        Write(currentStackPosition, value);
        SP--;
    }
    private byte PopFromStack()
    {
        SP++;
        ushort currentStackPosition = (ushort)(0x01 << 8 | SP);
        return Read(currentStackPosition);
    }

    private void SetZeroAndNegFlag(byte value)
    {
        if ((value & 128) != 0) _negativeFlag = true;
        else _negativeFlag = false;
        if (value != 0x00) _zeroFlag = false;
        else _zeroFlag = true;
    }
    private void AccumChanged()
    {
        if (A != 0x00) _zeroFlag = false; else _zeroFlag = true;
        if ((A & 128) != 0) _negativeFlag = true; else _negativeFlag = false;
    }
}
