namespace SixtyFiveOhTwo.Registers;
internal class UshortRegister
{
    private ushort value;
    internal string Name { get; private set; }
    internal ushort Get() { return value; }
    internal void Set(ushort val) { value = val; }
    public UshortRegister(ushort startingValue, string name)
    {
        value = startingValue;
        Name = name;    
    }
    public UshortRegister(ushort startingValue)
    {
        value=startingValue;
        Name = "";
    }
    public UshortRegister()
    {
        value = 0x0000;
        Name = "";
    }

    public override string ToString()
    {
        return $"{Name}: {value}";
    }
}
