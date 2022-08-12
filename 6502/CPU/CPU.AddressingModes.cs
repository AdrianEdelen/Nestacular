using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixtyFiveOhTwo.Enums;
namespace SixtyFiveOhTwo;
public partial class CPU
{

    private AddressModes IMM() //Immediate
    {
        _registers.PC++;
        fetchedAddress = _registers.PC;
        fetchedByte = Read(_registers.PC);
        _registers.PC++;
        return AddressModes.Immediate;
    }
    private AddressModes XIN() //X IND
    {
        //operand is a zero page address

        _registers.PC++;
        var indexByte = Read(_registers.PC);
        var newPos = (byte)(indexByte + _registers.X);
        var calcedPos = Read(newPos);
        var calcedPos2 = Read((byte)(newPos + 1));
        ushort addr = (ushort)(calcedPos2 << 8 | calcedPos);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        _registers.PC++;
        return AddressModes.XIndirect;
    }
    private AddressModes YIN() //Y IND
    {
        //Differing from x Indirect, the order is a little different and there
        //is a carry
        _registers.PC++;
        byte indexByte = Read(_registers.PC);
        byte b1 = Read(indexByte);
        byte b2 = Read((byte)(indexByte + 1));
        if (indexByte == 0xFF) b2++;
        ushort addr = (ushort)(b2 << 8 | b1);

        if (_registers.Y == 0xFF && indexByte != 0xFF)
        {
            addr += 0x100;
            addr--;
        }
        else if (_registers.Y == 0xFF && indexByte == 0xFF) addr--;
        else addr += _registers.Y;

        fetchedAddress = addr;
        fetchedByte = Read(addr);
        _registers.PC++;
        return AddressModes.YIndirect;

    }
    private AddressModes ABS() //Absolute
    {
        //get the high and low bytes for the address and build a short;
        _registers.PC++;
        byte PCL = Read(_registers.PC);
        _registers.PC++;
        byte PCH = Read(_registers.PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        _registers.PC++;
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        return AddressModes.Absolute;
    }
    private AddressModes XAB() //X Absolute
    {
        _registers.PC++;
        byte PCL = Read(_registers.PC);
        _registers.PC++;
        byte PCH = Read(_registers.PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + _registers.X);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        _registers.PC++;
        return AddressModes.XAbsolute;
    }
    private AddressModes YAB() //Y Absolute
    {
        _registers.PC++;
        byte PCL = Read(_registers.PC);
        _registers.PC++;
        byte PCH = Read(_registers.PC);
        ushort addr = (ushort)(PCH << 8 | PCL);
        addr = (ushort)(addr + _registers.Y);
        fetchedAddress = addr;
        fetchedByte = Read(addr);
        _registers.PC++;
        return AddressModes.YAbsolute;
    }
    private AddressModes IMP() //Implied
    {
        _registers.PC++;
        AccumMode = true;
        return AddressModes.Implied;
    }
    private AddressModes IND() //Indirect
    {

        //From my understanding it's:
        //read the immediate bytes
        //go to the location from those bytes
        //return the value from the location calculated from the original immediate calculation

        _registers.PC++;
        byte PCL = Read(_registers.PC);
        _registers.PC++;
        byte PCH = Read(_registers.PC);
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
        _registers.PC++;
        return AddressModes.Indirect;
    }
    private AddressModes REL() /*relative*/
    {
        //TODO: Why is this empty
        return AddressModes.Relative;
    }
    private AddressModes ZPG() //Zero Page
    {
        _registers.PC++;
        fetchedAddress = Read(_registers.PC);
        fetchedByte = Read(fetchedAddress);
        _registers.PC++;
        return AddressModes.ZeroPage;
    }
    private AddressModes XZP() //X Zero Page
    {
        _registers.PC++;
        var tempAddr = Read(_registers.PC);
        fetchedAddress = (byte)(tempAddr + _registers.X);
        fetchedByte = Read(fetchedAddress);
        _registers.PC++;
        return AddressModes.XZeroPage;
    }
    private AddressModes YZP() //Y Zero Page
    {
        _registers.PC++;
        var tempAddr = Read(_registers.PC);
        fetchedAddress = (byte)(tempAddr + _registers.Y);
        fetchedByte = Read(fetchedAddress);
        _registers.PC++;
        return AddressModes.YZeroPage;
    }
}
