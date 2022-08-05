namespace SixtyFiveOhTwo.Registers;
internal static class ProgramCounter
{
    private static ushort value;
    internal static ushort Get() { return value; }
    internal static void Set(ushort val) { value = val; }
}
