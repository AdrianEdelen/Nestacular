//I think that ultimately this will just generate either an image or a scan line or a pixel or whatever, and send that to some external
//renderer (js or something)

//this way we don't have to be concerned with how to actually display the image in c#

//Current Goal for the PPU is to get the pattern table rendered (or atleast represented in code)
//i think, maybe getting the pattern table represented in numbers like the wiki would be a good start. 

internal class PPU
{
    Bus _bus;

    public PPU(Bus bus) 
    {
        _bus = bus;

    }

    private void Test()
    {
        _bus.Read();
    }


}