using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using SkiaSharp;
using Nestacular.NESCore.CPUCore;
using Nestacular.NESCore.BusCore;
using Nestacular.NESCore.CartCore;
using Nestacular.NESCore.PPUCore;
using Nestacular.NESCore.CPUCore.Status;

namespace Nestacular.NESCore
{


    /// <summary>
    /// The NES Object represent the entire NES as a whole. 
    /// After each frame is drawn, the NES object will spit out a Frame containing a bitmap that can be used to render the frame to the screen
    /// The Frame additionally contains metadata associated with that block of execution as well. depending on the performance impact, this includes, historical opcode and PPU execution pallete information
    /// register status and more.
    /// 
    /// The NES contains a bus, each device that is a part of the nes will recieve a reference to the bus. that is how the memory will be accessed
    /// as well as reads and writes to and from the bus will be performed.
    /// 
    /// The individual components of the NES do not know about each other, except maybe in certain scenarios
    /// instead they just know about the bus, and can read and write to and from the bus to communicate with the other components
    /// 
    /// </summary>
    public class NES
    {
        public enum ExecutionMode
        {
            FullSpeed,
            FrameStep,
            CpuStep,
            PpuStep,
            ApuStep
        }

        public CPUStatus CPUStatus => _CPU.Status;
        public InstructionStatus InstructionStatus => _CPU.InstructionStatus;
        public List<InstructionStatus> InstructionHistory = new List<InstructionStatus>();

        private BUS _bus;
        public bool Pause { get; set; }

        private PPU _PPU;
        private CPU _CPU;
        public Cartridge Cart;

        public ulong masterClock = 0;
        private bool _exiting = false;
        private bool _cpuLock = false;
        private Stopwatch _calcTimer = new Stopwatch();
        bool FrameReady = false;
        bool FrameFlushed = true; //start on true since the frame buffer is empty
        private Frame? _frameBuffer;
        public Frame? FrameBuffer
        {
            get
            {
                if (FrameReady)
                {
                    FrameReady = false;
                    FrameFlushed = true;
                    return _frameBuffer;

                }
                else return null;
            }
            private set => _frameBuffer = value;
        }
        public NES()
        {
            _bus = new BUS();
            _CPU = new CPU(_bus);
            _PPU = new PPU(_bus);
            Cart = new Cartridge(_bus);

        }

        private bool _exitManualMode = false;
        public void ExitManualMode()
        {
            _exitManualMode = true;
        }

        public AutoResetEvent cpuAR = new AutoResetEvent(false);

        public void ToggleExecutionMode()
        {
            CurrentExecutionMode++;
            if ((int)CurrentExecutionMode > 4) CurrentExecutionMode = 0;
            if (CurrentExecutionMode == 0) ExecutionBlocker.Set();
        }
        public ExecutionMode CurrentExecutionMode = ExecutionMode.FullSpeed;
        public AutoResetEvent ExecutionBlocker = new AutoResetEvent(false);
        public void GenerateFrame()
        {
            //262*341 screen size
            _calcTimer.Restart();
            if (!FrameFlushed) return;
            if (Pause) return;
            if (_cpuLock) return;
            _cpuLock = true;
            //so we don't actually care about the timing here. we just want to do work to generate a full frame.

            //in theory we can read ahead even, or generate every possible next frame.

            //frames are 262*341 = 89,342 dots, each dot is a ppu cycle (i think) and the cpu runs at 1/3 speed
            //29780 cpu cycles per frame.

            //easiest way to do this is modulo and a loop
            var nextFrame = new Frame();
            var clock = 0;
            var ppuClock = 0;
            var cpuClock = 0;
            if (CurrentExecutionMode == ExecutionMode.FrameStep) ExecutionBlocker.WaitOne();
            while (clock < 89342)
            {
                if (CurrentExecutionMode == ExecutionMode.PpuStep) ExecutionBlocker.WaitOne();
                var pix = _PPU.SingleStep();
                if (pix != null)
                    nextFrame.SetPix(pix);
                ppuClock++;
                if (clock % 3 == 0)
                {
                    //every third time
                    if (CurrentExecutionMode == ExecutionMode.CpuStep || CurrentExecutionMode == ExecutionMode.PpuStep) ExecutionBlocker.WaitOne();
                    _CPU.StepCPU();
                    InstructionHistory.Insert(0, InstructionStatus);
                    if (InstructionHistory.Count > 15)
                    {
                        InstructionHistory.RemoveAt(InstructionHistory.Count - 1);
                    }
                    cpuClock++;
                }
                clock++;
            }



            FrameFlushed = false;
            _cpuLock = false;
            _calcTimer.Stop();
            nextFrame.CalcTime = _calcTimer.ElapsedMilliseconds;
            FrameBuffer = nextFrame;
            FrameReady = true;
        }

        public void RunEngine(bool startState = false)
        {
            Pause = !startState;
            Task.Run(() =>
            {
                while (!_exiting) GenerateFrame();
            });
        }
        public void StopEngine() { _exiting = true; }

    }

    public class Frame
    {
        SKBitmap bmp;
        int height = 342;
        int width = 262;
        string metadata = "";
        public long CalcTime { get; set; }
        public List<SKColor> colors = new List<SKColor>();
        public Frame()
        {
            bmp = new SKBitmap(width, height);
        }

        public void UpdateFrame(int mockData)
        {
            bmp = new SKBitmap();
            height = 100;
            width = 100;
            metadata = "MetadataAssociatedwithframe, such as frame count, cpu instruction batch, register values, etc.";
        }
        public void SetPix(NESCore.PPUCore.Pixel pixel)
        {
            bmp.SetPixel(pixel.x, pixel.y, pixel.color);
            colors.Add(pixel.color);
        }
        public SKBitmap GetFrame()
        {
            return bmp;
        }
        public List<SKColor> GetPixelColors()
        {
            return colors;
        }
        public byte[] GetPixelData()
        {
            List<byte> data = new List<byte>();
            foreach (var color in colors)
            {
                data.Add(color.Red);
                data.Add(color.Blue);
                data.Add(color.Green);
                data.Add(color.Alpha);
            }
            return data.ToArray();
        }
    }


    #region old code
    //since we don't have fine grained control over the timing, we can base everything off of a frame rendering.
    //this means we have a 17ms timing window to accomplish one frames worth of stuff.

    //so we will have an output of the nes which is 1 frames worth of content (a framebuffer so to speak)
    //we will do all the work. push to the frame buffer, and then do the next work, if the frame buffer is not ready, we will be blocking basically.


    //1.79 MHz the CPU executes 1,790,000 cyles per second.
    //Time between each cycle 0.000000558659217877095 seconds or
    //1s = 1000 ms            0.000001
    //.001s = 1ms
    //.000001 = 1 microsecond
    //.000000001 = 1 nanosecond
    //.000000000001 1 picosecond.

    //558 nanoseconds per cycle OR .558 microseconds.
    //a .NET tick is 100 nanoseconds, which can get us accuracy of within 50ish nanoseconds.
    //basically we need to execute a clock cycle every 5 ticks.
    //public bool MasterClockAdvance() 
    //{   var sw = new Stopwatch();
    //    sw.Start();

    //    //sleep/block appropriate time for clock speed
    //    //this stopwatch should take into consideration the time it takes to process everything on host
    //    //so if the host takes 3 ticks to process, we should only NOP for 2.
    //    masterClock ++;
    //    //var logString = _CPU.StepTo(masterClock);
    //    bool didWork = _CPU.StepTo(masterClock);
    //    //NOP(0.000000558659217877095, sw.ElapsedTicks); //TODO: does this NOP even work? 
    //    return didWork; //Temporary, this only works since there is only one thing doing work.

    //}
    #endregion

    //This is all old, and will probably change.
    //ultimately a frame is a bitmap with additional 'header' information. I am thinking that a frame will contain all cpu status (in a readonly format)
    //so any given frame can be serialized as a jpg as well as json or something with things like frameNum, cpu status, ppu status, apu status.
    //this could potentially be cool for speedrunners and stuff, if a powerful enough computer uses it, you can play and serialize ALL the data, like a full memory
    //dump almost.
    //public class Frame
    //{
    //    SKBitmap bmp; //262*341

    //    const int height = 1000;
    //    const int width = 1000;
    //    byte[] frameData = new byte[width * height];
    //    public int row { get; set; }
    //    public int col { get; set; }
    //    public Frame()
    //    {

    //        bmp = new SKBitmap(width, height);
    //    }

    //    public void SetPix(int col, int row, SKColor color)
    //    {
    //        bmp.SetPixel(col, row, color);
    //    }


    //    //dot 0,0 should be index 0
    //    //dot 0,1 should be index 1
    //    //dot 1,0 should be index 263?


    //    public void Test()
    //    {
    //        for (int col = 0; col < height; col++)
    //        {
    //            for (int row = 0; row < width; row++)
    //            {
    //                if (col % 2 == 0)
    //                {
    //                    SetPix(row, col, SKColors.Black);

    //                }
    //                else
    //                {
    //                    SetPix(row, col, SKColors.Purple);
    //                }
    //            }
    //        }
    //        SaveBMPtoPNG();
    //    }

    //    public void SaveBMPtoPNG()
    //    {

    //        var png = bmp.Encode(SKEncodedImageFormat.Png, 10).ToArray();
    //        File.WriteAllBytes($"Images/{DateTime.Now:HH_mm_ss}.png", png);


    //    }
    //    //262*341
    //    //Console.SetCursorPosition(0, 0);
    //    //        Console.WriteLine(nes.CPUStatus);

    //    public void GenerateTile(byte[] patternData)
    //    {
    //        byte b1 = patternData[0];
    //        byte b1a = patternData[8];
    //        byte b2 = patternData[1];
    //        byte b2a = patternData[9];
    //        byte b3 = patternData[2];
    //        byte b3a = patternData[10];
    //        byte b4 = patternData[3];
    //        byte b4a = patternData[11];
    //        byte b5 = patternData[4];
    //        byte b5a = patternData[12];
    //        byte b6 = patternData[5];
    //        byte b6a = patternData[13];
    //        byte b7 = patternData[6];
    //        byte b7a = patternData[14];
    //        byte b8 = patternData[7];
    //        byte b8a = patternData[15];


    //        Console.WriteLine(Convert.ToString(b1, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b2, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b3, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b4, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b5, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b6, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b7, 2).PadLeft(8, '0'));
    //        Console.WriteLine(Convert.ToString(b8, 2).PadLeft(8, '0'));



    //        var a = true;
    //    }

    //    public void GeneratePatternTable(byte[] patternData)
    //    {
    //        //CHR starts at 16+16384*PRGSIZE and is 8192*CHRSIZE long.



    //        var tiles = patternData.Chunk(16).ToList();
    //        var split = tiles.Chunk(256).ToList();
    //        var left = split[0];
    //        var right = split[1];
    //        int rowOffset = 0;
    //        int offsetAmount = 8;
    //        int maxRow = 262 / 8;
    //        int maxCol = 341 / 8;
    //        // 0 0 = 0
    //        // 1 0 = 1
    //        // 0 1 = 2
    //        // 1 1 = 3
    //        var color0 = SKColors.White;
    //        var color1 = SKColors.Gray;
    //        var color2 = SKColors.DarkGray;
    //        var color3 = SKColors.Black;


    //        var data = patternData;

    //        //b0/b8 b1/b9 b2/b10 b3/b11/ b4/b12 b5/13 b6/14 b7/b15 
    //        //b16/24
    //        //32 tiles high 
    //        //16 tiles wide
    //        var width = 128;
    //        var height = 256;

    //        var offset = 8;
    //        var row = 0;
    //        var colCounter = 0;
    //        var colOffset = 8 * colCounter;

    //        //for each group of 16 bytes - makes a tiles worth of data
    //        //combine the two groups to create a list of 8 rows
    //        //for each row
    //        //for each column



    //        for (int i = 0; i < data.Length; i += 16)
    //        {
    //            for (int j = 0; j < 8; j++)
    //            { //j represents the row without offset.
    //                //here we iterate through the rows of that tile

    //                //i = the starting data block point (increases by 16 each tile
    //                //j = our iteration through each data block point
    //                //j+8 = the right/bottom half data

    //                var top = i + j;
    //                var bot = i + j + 8;

    //                top = data[top];
    //                bot = data[bot];

    //                var bitsTop = new BitArray(new byte[] { data[i + j] });
    //                var bitsBot = new BitArray(new byte[] { data[i + j + 8] });
    //                //now we have the data to go through each column of the table.

    //                //here we need to offset the row by using the row offset above
    //                //the column needs to 
    //                for (var k = 0; k < bitsBot.Length; k++)
    //                { //k represents the col without offset
    //                    //k * j shoulf give
    //                    //Thread.Sleep(100);

    //                    var colu = k + colOffset;


    //                    var leftBit = bitsTop[k];
    //                    var rightBit = bitsBot[k];
    //                    SKColor color;
    //                    if (leftBit == true && rightBit == false) color = color3;
    //                    else if (leftBit == false && rightBit == true) color = color2;
    //                    else if (leftBit == true && rightBit == false) color = color1;
    //                    else if (leftBit == false && rightBit == false) color = color0;
    //                    else color = color0; //ERROR CONDITION

    //                    //Console.WriteLine($"{row + j},{colu}"); //this should make sense
    //                    SetPix(colu, row + j, color);
    //                }



    //                var adr = true;


    //            }
    //            //TODO: ShortCicruit
    //            colCounter++;
    //            colOffset = 8 * colCounter;
    //            if (i % 256 == 0 && i != 0) //every 16 tiles move down 8 rows
    //            {
    //                row += 1 * offset; //move down 8 rows each time
    //                colCounter = 0; //reset to the left side
    //            }
    //        }









    //        //now we need to get the next offset group of 16 for each spot (right now we just loop over the same thing)
    //        //var leftT = left[0];
    //        //var bits = new List<BitArray>();
    //        //for (var i = 0; i < leftT.Length / 2; i++)
    //        //{
    //        //    byte b1 = leftT[i];
    //        //    byte b2 = leftT[i + 8];

    //        //    var result = (byte)(b1 | b2);
    //        //    var ba = new BitArray(new byte[] { result });
    //        //    bits.Add(ba);
    //        //}

    //        //var sideOffset = 0;
    //        //var sideOffsetAmount = 400;
    //        //for (var side = 0; side < 2; side++)
    //        //{
    //        //    var half = split[side];
    //        //    foreach (var thing in half)
    //        //    {
    //        //        var bits = new List<BitArray>();
    //        //        for (var i = 0; i < thing.Length / 2; i++)
    //        //        {
    //        //            byte b1 = thing[i];
    //        //            byte b2 = thing[i + 8];

    //        //            var result = (byte)(b1 | b2);
    //        //            var ba = new BitArray(new byte[] { result });
    //        //            bits.Add(ba);
    //        //        }
    //        //        //creates one tile

    //        //        while (rowOffset < maxRow)
    //        //        {
    //        //            colOffset = 0;
    //        //            while (colOffset < maxCol)
    //        //            {
    //        //                for (var i = 0; i < 8; i++) //row
    //        //                {
    //        //                    for (var j = 0; j < 8; j++) //column
    //        //                    {
    //        //                        var localbits = bits[i];
    //        //                        //Console.SetCursorPosition(j, i);
    //        //                        var isSet1 = (localbits[j] == true) ? Color.Black : Color.White;
    //        //                        var isSet2 = (localbits[j] == true) ? Convert.ToChar(219) : ' ';
    //        //                        var calcedRow = i + rowOffset * offsetAmount;
    //        //                        var caledCol = j + colOffset * offsetAmount;
    //        //                        SetPix(calcedRow, caledCol, isSet1);
    //        //                        //Console.Write(isSet2);

    //        //                    }
    //        //                }
    //        //                colOffset++;
    //        //            }
    //        //            rowOffset++;
    //        //        }
    //        //    }

    //        //}


    //        //creates one tile




    //        SaveBMPtoPNG();
    //        Environment.Exit(0);



    //    }


    //}


}
