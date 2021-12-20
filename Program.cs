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
using System.Threading.Tasks;
using Nestacular;
using Nestacular.NES2;

class Program
{

    static void Main(string[] args)
    {
        var nes = new NES2();

        //gen logfile
        var fp = $"TestLog_{DateTime.Now:HH_mm_ss}.txt";

        using (var file = File.Create(fp))
        {
            using (var writer = new StreamWriter(file))
            {
                var totalCycles = 0;
                nes.Loader.InsertCart(@"nestest.nes");
                while (totalCycles < 8991)
                {

                    var a = nes.MasterClockAdvance();
                    if (nes._CPU._isHalted)
                    {
                        writer.WriteLine($"{a} -- Halted");
                        
                    }
                    if (!string.IsNullOrEmpty(a))
                    {
                        writer.WriteLine(a);
                        totalCycles++;
                    }
                }
            }
        }
        using (var file = File.OpenRead(fp))
        {
            var verfp = "VerificationFileNoPPU_CYC_Operands.txt";
            var verReader = new StreamReader(verfp);
            var logreader = new StreamReader(file);
            var lineCount = 0;
            while (!verReader.EndOfStream && !logreader.EndOfStream)
            {
                lineCount++;
                var logLine = logreader.ReadLine();
                var verLine = verReader.ReadLine();
                if (logLine != verLine)
                {
                    Console.WriteLine($"Mismatch found on line {lineCount}");
                    Console.WriteLine(logLine);
                    Console.WriteLine(verLine);
                    return;
                }
            }
        }
    }
}



