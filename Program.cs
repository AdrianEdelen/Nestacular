//https://www.qmtpro.com/~nes/misc/nestest.log
//load file
//set filepath
//TODO: change to filedialog
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nestacular;
using Nestacular.NES2;
using Nestacular.NESCore;

class Program
{
    static void Main(string[] args)
    {
        var nes = new NES2();
        nes._CPU.Clock();
        //nes.loader.LoadCart();
        var verFp = @"VerificationFile.txt";
        var sr = new StreamReader(verFp);
        string line;
        List<string> logOutput = new List<string>();
        int logIndex = 0;
        while ((line = sr.ReadLine()) != null)
        {
            logOutput.Add(line);
        }

        //create an ascii encoder for parsing bytes to strings
        ASCIIEncoding ascii = new ASCIIEncoding();


        
        while (true)
        {
            mismatch();
            //nes.Step();

            logIndex++; //move to next log line
        }
        void mismatch()
        {
            var isMismatch = false;
            //if (nes.cpu.IsNewOP)
            //{
            //    Trace.WriteLine($"{logOutput[logIndex - 1]}");
            //    Console.Read();
            //}
            //var expectedMemLocation = logOutput[logIndex].Substring(0, 4);

            //var expectedOpCodeAndParams = logOutput[logIndex].Substring(6, 8).Trim().Split(" ");

            //var expectedAccumulatorValue = logOutput[logIndex].Substring(50, 2);
            //var expectedXRegValue = logOutput[logIndex].Substring(55, 2);
            //var expectedYRegValue = logOutput[logIndex].Substring(60, 2);
            //var expectedPValue = logOutput[logIndex].Substring(65, 2);
            //var expectedCPUCycle = logOutput[logIndex].Substring(90);

            //if (expectedMemLocation != nes.cpu.PC.ToString("X4")) isMismatch = true;
            //for (var i = 0; i < expectedOpCodeAndParams.Count(); i++)
            //    if (expectedOpCodeAndParams[i] != nes.cpu.Memory[nes.cpu.PC + i].ToString("X2"))
            //        isMismatch = true;
            //if (expectedXRegValue != nes.cpu.RegisterX.ToString("X2")) isMismatch = true;
            //if (expectedYRegValue != nes.cpu.RegisterY.ToString("X2")) isMismatch = true;
            //if (expectedAccumulatorValue != nes.cpu.Accumulator.ToString("X2")) isMismatch = true;
            //if (expectedPValue != nes.cpu.Status.ToString("X2")) isMismatch = true;
            //if (isMismatch) Trace.WriteLine($"Expected = {logOutput[logIndex]}");
            //if (isMismatch) Trace.WriteLine($"Actual   = {nes.cpu.PC.ToString("X2")}  {nes.cpu.Memory[nes.cpu.PC].ToString("X2")} {nes.cpu.Memory[nes.cpu.PC + 1].ToString("X2")}     XXX $XXXX                       A:{nes.cpu.Accumulator.ToString("X2")} X:{nes.cpu.RegisterX.ToString("X2")} Y:{nes.cpu.RegisterY.ToString("X2")} P:{nes.cpu.Status.ToString("X2")} SP:{nes.cpu.StackPointer.ToString("X2")} PPU:  X,  X CYC:XXX");
            //if (isMismatch) Console.Read();
        }

    }
    
}



