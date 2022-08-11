namespace TofuNET.Gates;
public interface IGate
{
    public bool Input1 { set; }
    public bool Input2 { set; }
    public bool Output { get; }
    /// <summary>
    /// Sets both the first and the second input, and returns the output of the gate
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public bool SetBoth(bool left, bool right);

}
public class AND : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => Input1 & Input2;
    public AND(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public AND() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}
public class NAND : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => !(Input1 & Input2);
    public NAND(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public NAND() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}
public class NOR : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => !(Input1 | Input2);
    public NOR(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public NOR() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}
public class OR : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => Input1 | Input2;
    public OR(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public OR() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}
public class XNOR : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => !(Input1 ^ Input2);
    public XNOR(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public XNOR() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}
public class XOR : IGate
{
    public bool Input1 { private get; set; }
    public bool Input2 { private get; set; }
    public bool Output => Input1 ^ Input2;
    public XOR(bool input1, bool input2)
    {
        Input1 = input1;
        Input2 = input2;
    }
    public XOR() { }

    public bool SetBoth(bool left, bool right)
    {
        Input1 = left;
        Input2 = right;
        return Output;
    }
}