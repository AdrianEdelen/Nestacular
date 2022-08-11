using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixtyFiveOhTwo.Registers;

internal class RegisterGroup
{
    private UshortRegister _programCounter;
    private ByteRegister _stackPointer;
    private ByteRegister _accumulator;
    private ByteRegister _xRegister;
    private ByteRegister _yRegister;

    internal ushort PC { get => _programCounter.Get(); set => _programCounter.Set(value); }
    internal byte SP { get => _stackPointer.Get(); set => _stackPointer.Set(value); }
    internal byte A { get => _accumulator.Get(); set => _accumulator.Set(value); }
    internal byte X { get => _xRegister.Get(); set => _xRegister.Set(value); }
    internal byte Y { get => _yRegister.Get(); set => _yRegister.Set(value); }


    public string ProgramCounterStatus { get => _programCounter.ToString(); }
    public string StackPointerStatus { get => _stackPointer.ToString(); }
    public string AccumulatorStatus { get => _accumulator.ToString(); }
    public string XRegisterStatus { get => _xRegister.ToString(); }
    public string YRegisterStatus { get => _yRegister.ToString(); }

    public RegisterGroup(UshortRegister programCounter, ByteRegister stackPointer, ByteRegister accumulator, 
        ByteRegister xRegister, ByteRegister YRegister)
    {
        _programCounter = programCounter;
        _stackPointer = stackPointer;
        _accumulator = accumulator;
        _xRegister = xRegister;
        _yRegister = YRegister;
    }
}
