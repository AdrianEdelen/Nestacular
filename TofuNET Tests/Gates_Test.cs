namespace TofuNET_Tests;
[TestClass]
public class Gates_Test
{
    

    private bool[] TestGate(IGate gate)
    {
        bool F = false; //just for clarity
        bool T = true;
        var truthArr = new bool[4];
        truthArr[0] = gate.SetBoth(F, F);
        truthArr[1] = gate.SetBoth(F, T);
        truthArr[2] = gate.SetBoth(T, F);
        truthArr[3] = gate.SetBoth(T, T);
        return truthArr;
    }
    [TestMethod]
    public void AND_Test()
    {   /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 0 |
            | 0 | 1 | 0 |
            | 1 | 0 | 0 |
            | 1 | 1 | 1 |
            |-----------| */

        var result = TestGate(new AND());
        Assert.IsTrue(!result[0] && !result[1] && !result[2] && result[3]);
    }
    [TestMethod]
    public void NAND_Test() 
    {
        /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 1 |
            | 0 | 1 | 1 |
            | 1 | 0 | 1 |
            | 1 | 1 | 0 |
            |-----------|  */

        var result = TestGate(new NAND());
        Assert.IsTrue(result[0] && result[1] && result[2] && !result[3]);
    }
    [TestMethod]
    public void NOR_Test()
    {
        /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 1 |
            | 0 | 1 | 0 |
            | 1 | 0 | 0 |
            | 1 | 1 | 0 |
            |-----------|  */

        var result = TestGate(new NOR());
        Assert.IsTrue(result[0] && !result[1] && !result[2] && !result[3]);
    }

    [TestMethod]
    public void OR_Test()
    {
        /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 0 |
            | 0 | 1 | 1 |
            | 1 | 0 | 1 |
            | 1 | 1 | 1 |
            |-----------|  */

        var result = TestGate(new OR());
        Assert.IsTrue(!result[0] && result[1] && result[2] && result[3]);
    }
    [TestMethod]
    public void XNOR_Test()
    {
        /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 1 |
            | 0 | 1 | 0 |
            | 1 | 0 | 0 |
            | 1 | 1 | 1 |
            |-----------|  */

        var result = TestGate(new XNOR());
        Assert.IsTrue(result[0] && !result[1] && !result[2] && result[3]);
    }
    [TestMethod]
    public void XOR_Test()
    {
        /*   ___________
            | L | R | O |
            |---|---|---|
            | 0 | 0 | 0 |
            | 0 | 1 | 1 |
            | 1 | 0 | 1 |
            | 1 | 1 | 0 |
            |-----------|  */

        var result = TestGate(new XOR());
        Assert.IsTrue(!result[0] && result[1] && result[2] && !result[3]);
    }

    [TestMethod]
    public void NOT_Test()
    {
        var not = new NOT(false);
        Assert.IsTrue(not.Output);
        Assert.IsFalse(not.Toggle());
    }
    
}
