namespace Nestacular.NESCore.ClockCore;
class Clock
{
    //depending on how long it takes to overflow the clock, we may want a way to handle that (by incrementing a second clock on each overflow.) I would accept 64 days of continuous running before an overflow
    //less than that and I may consider keeping track
    private ulong _count = 0;
    public ulong Count { get; set; }
    private long _overflowTracker = 0;


    public void Increment()
    {

    }

    public void Decrement()
    {

    }

}