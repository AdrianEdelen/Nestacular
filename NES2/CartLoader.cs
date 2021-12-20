using System;
using System.Collections.Generic;
using System.IO;

namespace Nestacular.NES2
{
    internal class CartLoader
    {
        private BUS _bus;
        public CartLoader(BUS bus)
        {
            _bus = bus;
        }

        public void InsertCart(string romFilepath)
        {
            var fp = romFilepath;
            //open filestream
            FileStream fs = new FileStream(fp, FileMode.Open);
            int hexIn; //placeholder for each read byte
            List<byte> LoadedRom = new List<byte>(); //loaded rom is just getting the rom data off the file.
            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            { //continue looping until no more data. one byte at a time.
                LoadedRom.Add(Convert.ToByte(hexIn));
            }
            void LoadRomIntoMemory(List<byte> LoadedRom)
            {
                //when we start to load different cart types, we need to be more specific with what is loaded
                //e.g. PRG PRF
                //16384 bytes for PRG-ROM
                //PRG rom lower
                for (var i = 0; i < 0x4000; i++)
                {
                    //first 16k starts from 0xC000 
                    //We also skip the first 0x10 for now, that is the header data.
                    var curByte = LoadedRom[0x10 + i];
                    _bus.Write((ushort)(0xC000 + i), curByte);
                }

                // CHR RAM, I think this is part of the VRAM.
                //for (var i = 0; i < 0x2000; i++)
                //{
                //    var curByte = LoadedRom[0x10 + i];
                //    _bus.Write((ushort)(0x);
                //}
            }

            LoadRomIntoMemory(LoadedRom);
        }

        public void EjectCart()
        {

        }
    }
}
