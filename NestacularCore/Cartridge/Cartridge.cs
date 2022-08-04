using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore.CartCore;

public class Cartridge
{
    BusCore.BUS _bus;

    List<byte> header = new List<byte>();
    public List<byte> romData { get; private set; }
    private bool _cartridgeIsInserted = false;

    public Cartridge(BusCore.BUS bus)
    {
        _bus = bus;
        romData = new List<byte>();

    }

    public void Insert(string filePath)
    {
        if (_cartridgeIsInserted) return;
        //when we start to load different cart types, we need to be more specific with what is loaded
        //e.g. PRG PRF
        //16384 bytes for PRG-ROM
        //PRG rom lower

        //first 16k starts from 0xC000 
        FileStream fs = new FileStream(filePath, FileMode.Open);

        int fileByte;
        for (int i = 0; (fileByte = fs.ReadByte()) != -1; i++) romData.Add(Convert.ToByte(fileByte));
        header = romData.Take(0x10).ToList(); //grab the header data;
        romData.RemoveRange(0, 0x10); //remove the header data from the rom data
        for (var i = 0; i < 0x4000; i++) _bus.Write((ushort)(0xC000 + i),  romData[i]); //TODO: why is this loaded in at 0xC000


        
        /*
         * some hints on the PRG vs CHR rom/ram
         * so this implementation has seperate two seperate bits of memory,
         * one for the PRGRom and one for the CHRRom
         * they get loaded into raw and then split and copied to their respective spots.
         * 
         * there is also a PRG RAM that does not get loaded in from the cart.
         * 
        PRGROM = new byte[PRGROMSize];
        Array.Copy(Raw, PRGROMOffset, PRGROM, 0, PRGROMSize);
        source, source index, Destination, destination index, size
        if (CHRROMSize == 0)
            CHRROM = new byte[0x2000];
        else
        {
            CHRROM = new byte[CHRROMSize];
            Array.Copy(Raw, PRGROMOffset + PRGROMSize, CHRROM, 0, CHRROMSize);
        }
        */

        _cartridgeIsInserted = true;
    }
    public void Eject()
    {
        header.RemoveRange(0, header.Count);
        romData.RemoveRange(0, romData.Count);
        _cartridgeIsInserted = false;
    }

}
