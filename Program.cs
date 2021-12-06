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
class Program
{
    static void Main(string[] args)
    {



        
        var verFp = @"VerificationFile.txt";

        var loadedRom = CartridgeLoader.LoadCart();
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


        var CPU = new CPU();
        CPU.LoadRomIntoMemory(loadedRom);

        while (true)
        {
            if (CPU.IsNewOP)
            {
                Trace.WriteLine($"{logOutput[logIndex - 1]}");
                Console.Read();
            }
            var expectedMemLocation = logOutput[logIndex].Substring(0, 4);

            var expectedOpCodeAndParams = logOutput[logIndex].Substring(6, 8).Trim().Split(" ");

            var expectedAccumulatorValue = logOutput[logIndex].Substring(50, 2);
            var expectedXRegValue = logOutput[logIndex].Substring(55, 2);
            var expectedYRegValue = logOutput[logIndex].Substring(60, 2);
            var expectedCPUCycle = logOutput[logIndex].Substring(90);

            if (expectedMemLocation != CPU.PC.ToString("X2"))
                Trace.WriteLine($"Mismatch in log file line: {logIndex}: Current PC Address: {CPU.PC.ToString("X2")}. Log Memory address: {expectedMemLocation}");
            for (var i = 0; i < expectedOpCodeAndParams.Count(); i++)
                if (expectedOpCodeAndParams[i] != CPU.Memory[CPU.PC + i].ToString("X2"))
                {

                    Trace.WriteLine($"Mismatch in log file line (Wrong Opcode or param): {logIndex}: Current Memory Value: {CPU.Memory[CPU.PC + i].ToString("X2")}. Log Memory Value: {expectedOpCodeAndParams[i]}");
                }

            if (expectedXRegValue != CPU.RegisterX.ToString("X2"))
            {
                Trace.WriteLine($"Mismatch in log file line (X Register Mismatch): {logIndex}: Current X Value: {CPU.RegisterX.ToString("X2")}. Log X Value: {expectedXRegValue}");
            }
            if (expectedYRegValue != CPU.RegisterY.ToString("X2"))
            {
                Trace.WriteLine($"Mismatch in log file line (Y Register Mismatch): {logIndex}: Current Y Value: {CPU.RegisterY.ToString("X2")}. Log Y Value: {expectedYRegValue}");
            }
            if (expectedAccumulatorValue != CPU.Accumulator.ToString("X2"))
            {
                Trace.WriteLine($"Mismatch in log file line (Accumulator Mismatch): {logIndex}: Current A Value: {CPU.Accumulator.ToString("X2")}. Log A Value: {expectedAccumulatorValue}");
            }

            CPU.CycleCPU();

            logIndex++; //move to next log line
        }

    }
}



