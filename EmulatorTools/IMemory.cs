namespace EmulatorTools.Memory;
public interface IMemory
{
    void Write(ushort addr, byte data);
    byte Read(ushort addr, bool readOnly = false);
}
