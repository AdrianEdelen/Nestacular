namespace SixtyFiveOhTwo.Registers;
internal class ByteRegister
{
    private byte value;
    internal string Name { get;  private set; }
    internal byte Get() { return value; }
    internal void Set(byte val) { value = val; }
    public ByteRegister(byte startingValue, string name)
    {
        value = startingValue;
        Name = name;
    }
    public ByteRegister(byte startingValue)
    {
        value = startingValue;
        Name = "";
    }
    public ByteRegister()
    {
        value = 0x00;
        Name = "";
    }
    public override string ToString()
    {
        return $"{Name}: {value}";
    }
}
