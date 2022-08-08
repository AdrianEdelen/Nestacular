namespace SixtyFiveOhTwo;
internal class Instruction
{
    private delegate int OpDel();
    private delegate int AddrMode();
    private string _name;

    OpDel _opDel;
    AddrMode _addrMode;
    public Instruction(string name, Func<int> op, Func<int> addrMode)
    {
        _name = name;
        _opDel = new OpDel(op);
        _addrMode = new AddrMode(addrMode);
    }
    public ulong Execute()
    {
        var additionalClockCycles = _addrMode.Invoke();
        var clockCycles = _opDel.Invoke();
        return (ulong)(clockCycles + additionalClockCycles);
    }
    public override string ToString()
    {
        //TODO: TODO
        return $"";
    }
}

