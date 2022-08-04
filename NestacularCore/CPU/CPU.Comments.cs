using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore.CPUCore
{
    internal partial class CPU
    {
        #region CPU
        /* 
           Names of variables match the documented names of the components
           this makes it easier to compare against established Documentation 

           The CPU reads and writes to the bus to communicate with the rest of the NES, 
           it should also function as a regular 6502 just without decimal mode
           as the NES RICOH 2A0C did not have decimal mode enabled 
         
           On reset, the processor will read address $FFFC and $FFFD (called the reset vector) and load the program counter (PC) with their content.
           For example, if $FFFC = $00 and $FFFD = $10, then the PC will get loaded with $1000 and execution will start there. However, most 6502 systems contain ROM in the upper address region, say $E000-$FFFF so it's likely that the reset vector will point to a location there.
           Most systems have an OS of some sorts - ranging from a simple machine language monitor, BASIC interpreter, even GUI interfaces such as Contiki.
           Your OS must have a method of loading the programs generated from an assembler or compiler into RAM. It must also have a method of executing code in RAM.
           For simplicity, lets say you have a simple command line promt and you can load a program using the "LOAD Example.obj, $1000" command.
           This will load the program named Example.obj into RAM at address $1000.
           Next, from the command prompt, you would type "Exec $1000" which would move the address $1000 into the PC register and begin executing your program.
           You must have some sort of OS capable of doing these two steps in order to load and execute programs.

           I think that anything that is going to modify the operand qill just require a write as the last step,
           so basically we will read the value, do all our operations and then write it back into that position.
           so I believe there will be precarious manipulation of the PC for this.

         */
        #endregion

        #region Constructor
        /*
         This constructor is kind of complicated, so I moved it into its own file for readability.

            Some Explanation:
            Instructions with odd timings:
            0x11;
            0x10 **
            0x50 **
            0x19 *
            0xEB is actually USBC but the log files treat it as SBC
            M is a method to create a new INSTR object, this is just to squeeze the size of the table a little smaller it is like a psuedo factory
            we are assigning delegates for the opcodes and addressing modes for each of the possible 256 instructions available on the cpu
            this is basically a giant list of object initialization formatted in a table to make finding the an opcode actually easier believe it or not.

            to search the table as a human, just look at the strings and how they match up to the x and y and that is your byte for the opcode
            Additionally you can ctrl+F and search for a 3 character opcode mnemonic e.g. ADC and see everywhere in the table that opcode is referenced.
            to search the table as a computer, just index based off of the read byte in memory, the table is formatted so that the index lines up with the hex opcode value
            eg opcode 0x00 == _opcodes[0] and opcode 0xFF == _opcodes[255]

            //*****THE ORDER OF THIS LIST IS CRITICAL, IF THE ORDER IS CHANGED THE PC WILL FIND THE WRONG OPCODE******
            There are 256 total opcodes, some are legal some are not. a lot of them just halt the CPU, but hey, if the programmer wants to halt the cpu she can.
        */
        #endregion

        #region Helpers
        /*
         
        */
        #endregion

        #region Instruction
        /*
         
        */
        #endregion

        #region Instructions
        /*
         
        */
        #endregion

        #region InstructionStatus
        /*
         
        */
        #endregion

        #region Registers
        /*
         
        */
        #endregion

        #region CPUStatus
        /*
         
        */
        #endregion
    }
}
