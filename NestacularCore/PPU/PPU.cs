using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nestacular.NESCore.BusCore;
using SkiaSharp;

namespace Nestacular.NESCore.PPUCore;

public class PPU
{
    public ulong cycles { get; private set; }

    BUS _bus;
    private Pixel? _pixel;
    private bool doRender;
    #region Registers
    //PPUCTRL =   0x2000;
    //PPUMASK =   0x2001;
    //PPUSTATUS = 0x2002;
    //OAMADDR =   0x2003;
    //OAMDATA =   0x2004;
    //PPUSCROLL = 0x2005;
    //PPUADDR =   0x2006;
    //PPUDATA =   0x2007;
    //OAMDATA =   0x4014;

    //These are controlled by writing to the bus to these addresses.

    #endregion

    //PPU is preprogrammed, so each frame it does the same thing,
    //https://www.nesdev.org/wiki/PPU_rendering
    //so internally we will keep track of what line we are on and what dot in the line
    //then each cycle will iterate through,
    //I think the right way to do this is with a semaphore,
    //basically pause execution untill we get the signal to step forward maybe.

    //First things first, determine what the ppu is doing in each cycle
    //determine a way to keep track of where in rendering we are.



    public PPU(BUS bus)
    {
        _bus = bus;


        //RAM[0x2000] = 0x00;
        //RAM[0x2001] = 0x00;
        //if CPU before clock cycle 29658
        //ignore writes to PPUCTRL, PPUMASK, PPUSCROLL, PPUADDR

        //PPUSTATUS, OAMADDR, OAMDATA will work immediately


    }

    private int currentPixel;
    private int currentScanLine;
    private SkiaSharp.SKColor renderColor;

    //The PPU renders 262 scanlines per frame.Each scanline lasts for 341 PPU clock cycles(113.667 CPU clock cycles; 1 CPU cycle = 3 PPU cycles),
    //with each clock cycle producing one pixel.The line numbers given here correspond to how the internal PPU frame counters count lines.
    public Pixel? SingleStep()
    {
        doRender = false;
        Clock();
        //PerformScanlineStep();
        cycles++;
        if (doRender)
        {
            return new Pixel(currentScanLine, currentPixel, renderColor);
        }
        else return null;
    }

    void PerformCycleTask()
    {
        ScanLineTypes lineType = DetermineScanLine();
        DoPixelWork(lineType);
    }

    ScanLineTypes DetermineScanLine()
    {
        return currentScanLine switch
        {
            0 => ScanLineTypes.LineZero,
            > 0 and <= 239 => ScanLineTypes.VisibleScanLine,
            240 => ScanLineTypes.PostRenderLine,
            241 => ScanLineTypes.SetVBlankLine,
            > 241 and <= 260 => ScanLineTypes.BlankLine,
            -1 => ScanLineTypes.PreRenderLine,
            _ => ScanLineTypes.Undefined
        };
    }

    void DoPixelWork(ScanLineTypes sl)
    {
        var px = currentPixel;
        switch (sl)
        {
            case ScanLineTypes.LineZero:
                LineZero(px);
                break;
            case ScanLineTypes.VisibleScanLine:
                doRender = true;
                VisibleScanLine(px);
                break;
            case ScanLineTypes.PostRenderLine:
                PostRenderLine(px);
                break;
            case ScanLineTypes.SetVBlankLine:
                SetVblankLine(px);
                break;
            case ScanLineTypes.BlankLine:
                BlankLine(px);
                break;
            case ScanLineTypes.PreRenderLine:
                PreRenderLine(px);
                break;
            default:
                break;

        }




    }
    void LineZero(int px) {
        switch (px)
        {
            case 0:
                SkippedOnBGODD();
                break;
            case 1 or 2: 
                NTByte();
                break;

        }
    }
    void VisibleScanLine(int px) 
    {
        renderColor =  GenerateRandomColor();
    }
    void PostRenderLine(int px) { }
    void SetVblankLine(int px) { }
    void BlankLine(int px) { }
    void PreRenderLine(int px) { }

    #region Operations

    void SkippedOnBGODD() { }
    void NTByte() { }
    void ATByte() { }
    void LowBGTileByte() { }

    #endregion

    void Clock()
    {

        PerformCycleTask();
        IncrementClock();
        //Console.WriteLine($"{currentScanLine} | {currentPixel}");
    }

    /// <summary>
    /// Returns true if this increment marks the end of a frame;
    /// </summary>
    bool IncrementClock()
    {
        currentPixel++;

        if (currentPixel > 341)
        {
            currentPixel = 0;
            currentScanLine++;
        }
        if (currentScanLine > 260)
        {
            currentScanLine = -1;
            return true;
        }
        return false;
    }

     SKColor GenerateRandomColor()
    {
        SkiaSharp.SKColors colors = new SkiaSharp.SKColors();
        var t = typeof(SkiaSharp.SKColors).GetFields(); // get all fields

        Random r = new Random();
        int rInt = r.Next(0, t.Length); // create a random number in range of property count

        var res = colors.GetType().GetField(t[rInt].Name); // get random property name

        return (SkiaSharp.SKColor) res.GetValue(colors);
    }


}

enum ScanLineTypes
{
    LineZero,
    VisibleScanLine,
    PostRenderLine,
    SetVBlankLine,
    BlankLine,
    PreRenderLine,
    Undefined
}
