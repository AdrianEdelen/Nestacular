using System.Diagnostics;
using Nestacular.NESCore.BusCore;
using Nestacular.NESCore.CartCore;
using Nestacular.NESCore.PPUCore;
using Microsoft.Xna.Framework;
using SixtyFiveOhTwo;
using SixtyFiveOhTwo.Status;
namespace Nestacular.NESCore;


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

//There should possibly be another class for the middleman between the CORE and the display,
//the Monogame portion should ONLY handler rendering, and marshalling inputs. / displaying information.
//but the NES class probably should not handle stuff like execution modes.
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

    public Status CPUStatus => _CPU.Status;
    public InstructionStatus InstructionStatus => _CPU.InstructionStatus;
    public List<InstructionStatus> InstructionHistory = new List<InstructionStatus>();

    private BUS _bus;
    public bool Pause { get; set; }

    private PPU _PPU;
    private CPU _CPU;
    public Cartridge Cart;

    public ulong MasterClock { get; private set; }
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
        _CPU = new CPU(_bus, false);
        _PPU = new PPU(_bus);
        Cart = new Cartridge(_bus);
        MasterClock = 0;

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

    //TODO: how to get the COMPLETE recoverable state of the emulator every cycle, this can give perfect save states as well as perfect rewinds (I will accept CPU cycle)
    //I guess cpu cycle is not hard, we just need to know 
    //A. what rom is loaded,
    //B. The state of the ram,
    //C. the state of all registers,
    //D. the state of all pointers.
    //E. the state of all clocks in the system, this will tell us how far into a frame we are etc.
    //F> need to determine the PPU State.
    public async void GenerateFrame()
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
        //Task<int> task = Method1();
        //Method2();
        //int count = await task;
        //Method3(count);
        //easiest way to do this is modulo and a loop
        var nextFrame = new Frame();
        var frameClock = 0;
        if (CurrentExecutionMode == ExecutionMode.FrameStep) ExecutionBlocker.WaitOne();
        while (frameClock < 89342)
        {
            if (CurrentExecutionMode == ExecutionMode.PpuStep) ExecutionBlocker.WaitOne();
            var color = _PPU.Step(MasterClock);
            if (color != null)
                nextFrame.AddRandomColor();
            //nextFrame.AddColor((Color)color);
            if (frameClock % 3 == 0)
            {
                //every third time
                if (CurrentExecutionMode == ExecutionMode.CpuStep || CurrentExecutionMode == ExecutionMode.PpuStep) ExecutionBlocker.WaitOne();
                _CPU.Step(MasterClock);
                
            }
            frameClock++;
        }

        FrameFlushed = false;
        _cpuLock = false;

        _calcTimer.Stop();
        nextFrame.CalcTime = _calcTimer.ElapsedMilliseconds;
        FrameBuffer = nextFrame;
        FrameReady = true;
        MasterClock++;
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
    public const int Height = 240;
    public const int Width = 341;
    string metadata = "";
    private int index = 0;
    public long CalcTime { get; set; }
    public Color[] colorArr = new Color[Height * Width];
    public Frame()
    {
    }
    public void AddColor(Color color)
    {
        colorArr[index] = color;
        index++;
    }
    public void AddRandomColor()
    {
        Random rand = new Random();
        colorArr[index] = new Color(rand.Next(256), rand.Next(256), rand.Next(256), 255);
        index++;
    }


}

