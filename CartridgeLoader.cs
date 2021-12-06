using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular
{
    static public class CartridgeLoader
    {
        static public List<byte> LoadCart()
        {
            var fp = @"nestest.nes";
            var verFp = @"VerificationFile.txt";

            //open filestream
            FileStream fs = new FileStream(fp, FileMode.Open);
            int hexIn; //placeholder for each read byte
            List<byte> LoadedRom = new List<byte>();
            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            { //continue looping until no more data. one byte at a time.

                //now we have our rom loaded into the application.
                //from this point though, we should be treating the rom 
                //as if it was actually a cartridge
                //especially if we want to be able to read real cartridges later.
                LoadedRom.Add(Convert.ToByte(hexIn));
            }

            return LoadedRom;
        }

        static public void RomChecker(List<byte> rom)
        {
            //16-byte header
            var Header = new List<byte>();
            for (var i = 0; i < 16; i++)
            {
                Header.Add(rom[i]);
            }
            ASCIIEncoding ascii = new ASCIIEncoding();
            var NESStringIdentifier = ascii.GetString(Header.GetRange(0, 3).ToArray());
            var OneA = Header[3].ToString();
            var NumOfPRG_ROMBanks16KB = Header[4].ToString();
            var NumOfCHR_ROMBanks8KB = Header[5].ToString();
            var ROMControlByte1 = Header[6];
            var ROMControlByte2 = Header[7];
            var NumOfRAMBanks8KB = Header[8].ToString();
            string mirrorType = "Horizontal";
            string batteryBackedRam = "N/A";
            string trainer = "N/A";
            string fourScreenMirroring = "N/A";
            if ((ROMControlByte1 & 1) == 1) mirrorType = "Vertical";
            if ((ROMControlByte1 & 2) == 1) batteryBackedRam = "Located in $6000-$7FFF";
            if ((ROMControlByte1 & 4) == 1) trainer = "512 byte trainer located in $7000-$71FF";
            if ((ROMControlByte1 & 8) == 1) fourScreenMirroring = "Four Way Screen Mirroring Enabled";
            StringBuilder mapperNumber = new StringBuilder();
            if ((ROMControlByte1 & 16) == 1) mapperNumber.Append("1");
            else mapperNumber.Append("0");
            if ((ROMControlByte1 & 32) == 1) mapperNumber.Append("1");
            else mapperNumber.Append("0");
            if ((ROMControlByte1 & 64) == 1) mapperNumber.Append("1");
            else mapperNumber.Append("0");
            if ((ROMControlByte1 & 128) == 1) mapperNumber.Append("1");
            else mapperNumber.Append("0");

            StringBuilder reserved = new StringBuilder();
            StringBuilder upperMapperNumber = new StringBuilder();

            if ((ROMControlByte2 & 1) == 1) reserved.Append("1");
            else reserved.Append("0");
            if ((ROMControlByte2 & 2) == 1) reserved.Append("1");
            else reserved.Append("0");
            if ((ROMControlByte2 & 4) == 1) reserved.Append("1");
            else reserved.Append("0");
            if ((ROMControlByte2 & 8) == 1) reserved.Append("1");
            else reserved.Append("0");

            if ((ROMControlByte2 & 16) == 1) upperMapperNumber.Append("1");
            else upperMapperNumber.Append("0");
            if ((ROMControlByte2 & 32) == 1) upperMapperNumber.Append("1");
            else upperMapperNumber.Append("0");
            if ((ROMControlByte2 & 64) == 1) upperMapperNumber.Append("1");
            else upperMapperNumber.Append("0");
            if ((ROMControlByte2 & 128) == 1) upperMapperNumber.Append("1");
            else upperMapperNumber.Append("0");

            Console.WriteLine("NES Cartridge/ROM Verification");
            Console.WriteLine($"iNES File Identifier: {NESStringIdentifier}");
            Console.WriteLine($"$1A: {OneA}");
            Console.WriteLine($"Number of PRG_ROM Banks (16KB): {NumOfPRG_ROMBanks16KB}");
            Console.WriteLine($"Number of CHR_ROM Banks (8kb): {NumOfCHR_ROMBanks8KB}");
            Console.WriteLine($"Control Bits 1: {ROMControlByte1}");
            Console.WriteLine($"Mirroring type: {mirrorType}");
            Console.WriteLine($"Battery Backed Ram: {batteryBackedRam}");
            Console.WriteLine($"trainer: {trainer}");
            Console.WriteLine($"Four-screen mirroring: {fourScreenMirroring}");
            Console.WriteLine($"Lower Bits of Mapper Number: {mapperNumber}b");
            Console.WriteLine($"Control Bits 2: {ROMControlByte2} ");
            Console.WriteLine($"Reserved (should be zero): {reserved}");
            Console.WriteLine($"Upper bits of Mapper Number: {upperMapperNumber}b");
            Console.WriteLine($"{ROMControlByte1}");
            Console.WriteLine($"{ROMControlByte2}");
            Console.WriteLine($"Number of RAM Banks (8KB): {NumOfRAMBanks8KB}");
            foreach (var b in Header.Skip(10))
            {
                Console.Write(b.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Press Any Key To continue");
            //Console.ReadKey();
            Console.WriteLine();
        }
    }
}
