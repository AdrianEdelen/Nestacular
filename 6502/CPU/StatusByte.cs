using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixtyFiveOhTwo.Exceptions;
namespace SixtyFiveOhTwo.CPU;

internal enum StatusFlags
{
    Carry,
    Zero,
    InterruptDisable,
    DecimalMode,
    BreakCommand,
    Overflow,
    Negative
}
internal class StatusFlag
{
    internal bool Carry { get; set; }
    internal bool Zero { get; set; }
    internal bool InterruptDisable { get; set; }
    internal bool DecimalMode { get; set; }
    internal bool BreakCommand { get; set; }
    internal bool Overflow { get; set; }
    internal bool Negative { get; set; }


    internal void ModifyFlag(StatusFlags flag, bool value)
    {
        _ = flag switch
        {
            StatusFlags.Carry => Carry = value,
            StatusFlags.Zero => Zero = value,
            StatusFlags.InterruptDisable => InterruptDisable = value,
            StatusFlags.DecimalMode => DecimalMode = value,
            StatusFlags.BreakCommand => BreakCommand = value,
            StatusFlags.Overflow => Overflow = value,
            StatusFlags.Negative => Negative = value,
            _ => throw new InvalidStatusBitException()
        };
    }
}


