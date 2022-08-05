namespace SixtyFiveOhTwo.CPUCore;
public struct InstructionStatus
{
    internal readonly ushort Address;
    internal readonly string Instruction;

    internal InstructionStatus(ushort address, string instruction)
    {
        Address = address;
        Instruction = instruction;
    }
    public override string ToString()
    {
        return $"{Address:X4} | {Instruction}";
    }
}

