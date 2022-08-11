namespace TofuNET.Gates;
public class NOT
{
    public bool Input { private get; set; }
    public bool Output => !Input;
    public bool Toggle()
    {
        Input = !Input;
        return Output;
    }
    public NOT(bool startingValue)
    {
        Input = startingValue;
    }
    public NOT() { }
}
