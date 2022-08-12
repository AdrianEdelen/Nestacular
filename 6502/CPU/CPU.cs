using EmulatorTools.Memory;
using SixtyFiveOhTwo.Registers;
using SixtyFiveOhTwo.Status;
using EmulatorTools.CPU;
using SixtyFiveOhTwo.Flags;
namespace SixtyFiveOhTwo;
/* 
       The CPU reads and writes to the bus to communicate with the rest of the NES, 
       it should also function as a regular 6502 just without decimal mode
       as the NES RICOH 2A0C did not have decimal mode enabled 

       On reset, the processor will read address $FFFC and $FFFD (called the reset vector) and load the program counter (PC) with their content.
       For example, if $FFFC = $00 and $FFFD = $10, then the PC will get loaded with $1000 and execution will start there. However, most 6502 systems contain ROM in the upper address region, say $E000-$FFFF so it's likely that the reset vector will point to a location there.
       Most systems have an OS of some sorts - ranging from a simple machine language monitor, BASIC interpreter, even GUI interfaces such as Contiki.
       Your OS must have a method of loading the programs generated from an assembler or compiler into RAM. It must also have a method of executing code in RAM.
       For simplicity, lets say you have a simple command line promt and you can load a program using the "LOAD Example.obj, $1000" command.
       This will load the program named Example.obj into RAM at address $1000.
       Next, from the command prompt, you would type "Exec $1000" which would move the address $1000 into the PC register and begin executing your program.
       You must have some sort of OS capable of doing these two steps in order to load and execute programs.

       I think that anything that is going to modify the operand qill just require a write as the last step,
       so basically we will read the value, do all our operations and then write it back into that position.
       so I believe there will be precarious manipulation of the PC for this.

     */
public partial class CPU : ICPU
{
    #region Registers

    private RegisterGroup _registers;
    private StatusFlag _flags;
    IMemory _bus;

    public Status.Status Status { get; private set; }
    public InstructionStatus InstructionStatus { get; private set; }

    #endregion

    

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
            didBranch = false;
        }
        return InternalClock;
    }

    public ulong Clock()
    {
        _opCode = _bus.Read(_registers.PC); //get the byte of memory at the address of the PC
        var clockIncrement = _opCodes[_opCode].Execute(); //Actually Execute the op
        return clockIncrement;
    }
    internal void UpdateStatus()
    {
        Status = new Status.Status(_registers.PC, _registers.SP, _registers.A, _registers.X, _registers.Y, CreateStatusByte(), 
            _flags.Carry, _flags.Zero, _flags.InterruptDisable, _flags.DecimalMode, 
            _flags.BreakCommand, _flags.Overflow, _flags.Negative, AccumMode, 
            fetchedByte, fetchedAddress, InternalClock);
    }
    internal void updateInstructionStatus()
    {
        InstructionStatus = new InstructionStatus(_registers.PC, _opCodes[_opCode].ToString());
    }

    private void NMI() { /* nmi not implemented TODO */ }
    public void Startup() { throw new NotImplementedException(); }

    public void Shutdown() { throw new NotImplementedException(); }
    public void Reset() { throw new NotImplementedException(); }
    private byte CreateStatusByte()
    {
        var flags = new bool[8] { _flags.Carry, _flags.Zero, _flags.InterruptDisable, _flags.DecimalMode, false, true, _flags.Overflow, _flags.Negative };
        byte range = 0;
        if (flags.Length < 8) range = 0;
        for (int i = 0; i < 8; i++) if (flags[i]) range |= (byte)(1 << i);
        return range;
    }
}
