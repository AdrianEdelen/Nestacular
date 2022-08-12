using SixtyFiveOhTwo.Enums;
namespace SixtyFiveOhTwo;
internal class Instruction
{
    private delegate int OpDel(AddressModes addrMode);
    private delegate AddressModes AddrMode();
    private string _name;

    OpDel _opDel;
    AddrMode _addrMode;
    public Instruction(string name, Func<AddressModes, int> op, Func<AddressModes> addrMode)
    {
        _name = name;
        _opDel = new OpDel(op);
        _addrMode = new AddrMode(addrMode);
    }
    public ulong Execute()
    {
        //TODO: probably change _currentAddressMode from a state variable to a return value addrmode, and pass it into the opDel.
        var currentAddressMode = _addrMode.Invoke();
        var clockCycles = _opDel.Invoke(currentAddressMode);
        return (ulong)(clockCycles);
    }
    public override string ToString()
    {
        //TODO: TODO
        return $"";
    }
}

