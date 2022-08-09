using EmulatorTools.Memory;
using SixtyFiveOhTwo.Registers;
using SixtyFiveOhTwo.Status;
using EmulatorTools.CPU;
namespace SixtyFiveOhTwo;
public partial class CPU : ICPU
{
    #region Registers

    private UshortRegister _programCounter;
    private ByteRegister _stackPointer;
    private ByteRegister _accumulator;
    private ByteRegister _xRegister;
    private ByteRegister _yRegister;

    private ushort PC { get => _programCounter.Get(); set => _programCounter.Set(value); }
    private byte SP { get => _stackPointer.Get(); set => _stackPointer.Set(value); }
    private byte A { get => _accumulator.Get(); set => _accumulator.Set(value); }
    private byte X { get => _xRegister.Get(); set => _xRegister.Set(value); }
    private byte Y { get => _yRegister.Get(); set => _yRegister.Set(value); }


    public string ProgramCounterStatus { get => _programCounter.ToString(); }
    public string StackPointerStatus { get => _stackPointer.ToString(); }
    public string AccumulatorStatus { get => _accumulator.ToString(); }
    public string XRegisterStatus { get => _xRegister.ToString(); }
    public string YRegisterStatus { get => _yRegister.ToString(); }

    public Status.Status Status { get; private set; }
    public InstructionStatus InstructionStatus { get; private set; }

    #endregion

    #region Flags

    //TODO: Change this to be built from the status byte proper
    private bool _carryFlag = false;
    private bool _zeroFlag = false;
    private bool _interruptDisableFlag = false;
    private bool _decimalModeFlag = false;
    private bool _breakCommandFlag = false;
    private bool _overflowFlag = false;
    private bool _negativeFlag = false;

    #endregion

    IMemory _bus;
    internal ulong InternalClock { get; private set; }

    //internal representations, no real world equivalent (that I know of).
    private bool _BCDEnabled;
    private bool AccumMode = false;
    private bool _crossedPage = false;
    private byte _opCode;
    private byte fetchedByte = 0x00;
    private ushort fetchedAddress = 0x0000;


    private static List<Instruction> _opCodes = new List<Instruction>();



    // *
    // add 1 to cycles if page boundary is crossed
    // **
    // add 1 to cycles if branch occurs on same page
    // add 2 to cycles if branch occurs to different page
    //need to figure out how to set the clock cycles for these conditions proper.
    //both addressing mode and the instruction itself allow for setting clock cycles.
    //so now there should just be an enum for the address mode
    //and the instruction will check that, and sum the cycles.
    //the instruction itself can also check for edgecases such as page crossing to modify the number of cycles.


    public ulong Step(ulong masterClock)
    {
        if (masterClock >= InternalClock)
        {
            InternalClock += Clock();
            UpdateStatus();
            updateInstructionStatus();

            //reset the temp flags
            AccumMode = false;
            _crossedPage = false;
            _currentAddressMode = AddressingModes.Undefined;
        }
        return InternalClock;
    }

    public ulong Clock()
    {
        _opCode = _bus.Read(PC); //get the byte of memory at the address of the PC
        var clockIncrement = _opCodes[_opCode].Execute(); //Actually Execute the op
        return clockIncrement;
    }
    internal void UpdateStatus()
    {
        Status = new Status.Status(PC, SP, A, X, Y, CreateStatusByte(), 
            _carryFlag, _zeroFlag, _interruptDisableFlag, _decimalModeFlag, 
            _breakCommandFlag, _overflowFlag, _negativeFlag, AccumMode, 
            fetchedByte, fetchedAddress, InternalClock);
    }
    internal void updateInstructionStatus()
    {
        InstructionStatus = new InstructionStatus(PC, _opCodes[_opCode].ToString());
    }

    private void NMI() { /* nmi not implemented TODO */ }
    public void Startup() { throw new NotImplementedException(); }

    public void Shutdown() { throw new NotImplementedException(); }
    public void Reset() { throw new NotImplementedException(); }
    private byte CreateStatusByte()
    {
        var flags = new bool[8] { _carryFlag, _zeroFlag, _interruptDisableFlag, _decimalModeFlag, false, true, _overflowFlag, _negativeFlag };
        byte range = 0;
        if (flags.Length < 8) range = 0;
        for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
        return range;
    }
}
