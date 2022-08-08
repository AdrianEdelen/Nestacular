using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SixtyFiveOhTwo;
public partial class CPU
{
    private int IMM() //Immediate
    {
        PC++;
        fetchedAddress = PC;
        fetchedByte = Read(PC);
        PC++;
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int XIN() //X IND
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int YIN() //Y IND
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;

    }
    private int ABS() //Absolute
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int XAB() //X Absolute
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int YAB() //Y Absolute
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int IMP() //Implied
    {
        //TODO: Verify This is Correct
        PC++;
        AccumMode = true;
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int IND() //Indirect
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
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int REL() /*relative*/ 
    {
        /*Dumb me I don't remember why there is nothing here */
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int ZPG() //Zero Page
    {
        PC++;
        fetchedAddress = Read(PC);
        fetchedByte = Read(fetchedAddress);
        PC++;
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int XZP() //X Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + X);
        fetchedByte = Read(fetchedAddress);
        PC++;
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
    private int YZP() //Y Zero Page
    {
        PC++;
        var tempAddr = Read(PC);
        fetchedAddress = (byte)(tempAddr + Y);
        fetchedByte = Read(fetchedAddress);
        PC++;
        return 0; //Extra Cycles based on address mode; TODO: Determine Extra Cycles;
    }
}
