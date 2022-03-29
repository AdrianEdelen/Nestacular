//I think that ultimately this will just generate either an image or a scan line or a pixel or whatever, and send that to some external
//renderer (js or something)

//this way we don't have to be concerned with how to actually display the image in c#

//Current Goal for the PPU is to get the pattern table rendered (or atleast represented in code)
//i think, maybe getting the pattern table represented in numbers like the wiki would be a good start. 

internal class PPU
{
    Bus _bus;
    int _cycles
    //TODO: PPU RAM
    //TODO: CHR ROM
    //TODO: Add / Emulate Registers
    //TODO: NMI Interrupt
    
    //each register is just a byte in memory (i think this is the same memory as the general RAM)
    //therefore implementation of each register is just reading//writing to that byte in memory
    //it seems like the way data is transferred between the cpu and the PPU is by the CPU writing to these shared memory locations
    //the CPU writes to these, and the PPU reads them into its memory
    byte PPUCTRL {
        get
        {
            _bus.Read(0x2000);
            
            
        }
        set 
        {
            _bus.Write(0x2000, value));
            MirrorRegisterValue(0x2000);
        }
    }

    public PPU(Bus bus) 
    {
        _bus = bus;

    }

    public void StepTo(ulong masterClockCycles)
    {
        //TODO: what is the logic for calculating PPU cycles vs the master clock
        //
        if (masterClockCycles > _cycles)
        {

        }
    }

    //need to implement register mirroring
    private void MirrorRegisterValue(short location)
    {
        //this should write the same value to the register every 8 bytes
        var val = _bus.Read(location);
        for (int i = location; i <= 0x3FFFF; 1 += 8)
        {
            _bus.Write(i, val);
        }
    }

    private void Test()
    {
        var b1 = 0b0000001;
        _bus.Read();
    }

    private void RenderScanLine() 
    { //render an individual scan line

        cycles += 341;
    }

    private void RenderFrame()
    {
        //render 262 scan lines
        for (i = 0; i < 262; i++)
        {
            if (i == 240)
            {
                NMIInterrupt();
            }
            RenderScanLine()
            
        }
    }

    private void NMIInterrupt() 
    {

    }
}