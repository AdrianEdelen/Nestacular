namespace SixtyFiveOhTwo.CPUCore;

public class Instruction
{
    private delegate void OpDel();
    private delegate void AddrMode();
    private string _name;

    private int _clockCycles;
    private int _additionalClockCycles;
    OpDel _opDel;
    AddrMode _addrMode;
    public Instruction(string name, Action op, Action addrMode, int clockCycles)
    {
        _name = name;
        _clockCycles = clockCycles;
        _opDel = new OpDel(op);
        _addrMode = new AddrMode(addrMode);
    }
    public ulong Execute()
    {
        _addrMode.Invoke();
        _opDel.Invoke();
        return (ulong)(_clockCycles + _additionalClockCycles);
    }
    public override string ToString()
    {
        //return $@"{_name} : {_clockCycles} | {_addrMode}";
        return $@"{_name} : {_clockCycles} | ";
    }
}

