namespace SixtyFiveOhTwo.CPUCore;
public partial class CPU
{
    private ushort PC = 0x0000; //Program Counter
    private byte SP = 0xFD; //Stack Pointer
    private byte A = 0x00; //Accumulator
    private byte X = 0x00; //X Register
    private byte Y = 0x00; //Y Register
    private byte P => CreateStatusByte();         //Status Byte

    private bool C = false; //Carry Flag
    private bool Z = false; //Zero Flag
    private bool I = false; //Interrupt Disable Flag
    private bool D = false; //Decimal Mode Flag
    private bool B = false; //Break Command Flag
    private bool V = false; //Overflow Flag
    private bool N = false; //Negative Flag
}

