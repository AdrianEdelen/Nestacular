namespace SixtyFiveOhTwo.Exceptions;
[Serializable]
public class CPUHaltedException : Exception
{
    public CPUHaltedException() : base() { }
    public CPUHaltedException(string message) : base(message) { }
    public CPUHaltedException(string message, Exception inner) : base(message, inner) { }
    protected CPUHaltedException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


