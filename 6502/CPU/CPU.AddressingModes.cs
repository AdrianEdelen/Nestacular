using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SixtyFiveOhTwo;
public partial class CPU
{
    private enum AddressingModes
    {
        Immediate,
        XIndirect,
        YIndirect,
        Absolute,
        XAbsolute,
        YAbsolute,
        Implied,
        Indirect,
        Relative,
        ZeroPage,
        XZeroPage,
        YZeroPage,
        Accumulator,
        Undefined
    }
    private AddressingModes _currentAddressMode = AddressingModes.Undefined;
    private void IMM() //Immediate
    {
        PC++;
        fetchedAddress = PC;
        fetchedByte = Read(PC);
        PC++;
        _currentAddressMode = AddressingModes.Immediate;
    }
    private void XIN() //X IND
    {
        //operand is a zero page address

        PC++;
        var indexByte = Read(PC);
        var newPos = (byte)(indexByte + X);
        var calcedPos = Read(newPos);
        var calcedPos2 = Read((byte)(newPos + 1));
        ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
        _currentAddressMode = AddressingModes.XIndirect;
    }
    private void YIN() //Y IND
    {
        //Differing from x Indirect, the order is a little different and there
        //is a carry
        PC++;
        byte indexByte = Read(PC);
        byte b1 = Read(indexByte);
        byte b2 = Read((byte)(indexByte + 1));
        if (indexByte == 0xFF) b2++;
        ushort addr = (ushort)(b2 << 8 | b1);

        if (Y == 0xFF && indexByte != 0xFF)
        {
            addr += 0x100;
            addr--;
        }
        else if (Y == 0xFF && indexByte == 0xFF) addr--; 
        else addr += Y;

        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
        _currentAddressMode = AddressingModes.YIndirect;

    }
    private void ABS() //Absolute
    {
        //get the high and low bytes for the address and build a short;
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        PC++;
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        _currentAddressMode = AddressingModes.Absolute;
    }
    private void XAB() //X Absolute
    {
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + X);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
        _currentAddressMode = AddressingModes.XAbsolute;
    }
    private void YAB() //Y Absolute
    {
        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + Y);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        PC++;
        _currentAddressMode = AddressingModes.YAbsolute;
    }
    private void IMP() //Implied
    {
        PC++;
        AccumMode = true;
        _currentAddressMode = AddressingModes.Implied;
    }
    private void IND() //Indirect
    {

        //From my understanding it's:
        //read the immediate bytes
        //go to the location from those bytes
        //return the value from the location calculated from the original immediate calculation

        PC++;
        byte PCL = Read(PC);
        PC++;
        byte PCH = Read(PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        if ((addr & 0x00FF) == 0xFF)
        {
            fetchedAddress = addr += 1;
            fetchedByte = Read(addr += 1);

        }
        else
        {
            var b1 = Read(addr);
            var b2 = Read((ushort)(addr + 1));
            var calcedLocation = (ushort)(b2 << 8 | b1);

            fetchedAddress = calcedLocation;
            fetchedByte = Read(calcedLocation);

        }
        PC++;
        _currentAddressMode = AddressingModes.Indirect;
    }
    private void REL() /*relative*/ 
    {
        //TODO: Why is this empty
        _currentAddressMode = AddressingModes.Relative;
    }
    private void ZPG() //Zero Page
    {
        PC++;
        fetchedAddress = Read(PC);
        fetchedByte = Read(fetchedAddress);
        PC++;
        _currentAddressMode = AddressingModes.ZeroPage;
    }
    private void XZP() //X Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + X);
        fetchedByte = Read(fetchedAddress);
        PC++;
        _currentAddressMode = AddressingModes.XZeroPage;
    }
    private void YZP() //Y Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + Y);
        fetchedByte = Read(fetchedAddress);
        PC++;
        _currentAddressMode = AddressingModes.YZeroPage;
    }
}
