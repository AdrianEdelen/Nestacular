#pragma once
#include "BUS.h"
namespace NES
{
    class CPU
    {
        private: 
        BUS _bus;
        //Registers
        unsigned short PC = 0x0000;
        unsigned char SP = 0xFD;
        unsigned char A = 0x00;
        unsigned char x = 0x00;
        unsigned char Y = 0x00;
        unsigned char P;
        //flags
        bool C = false;
        bool Z = false;
        bool I = false;
        bool D = false;
        bool B = false;
        bool N = false;
        
        //String _pString;


        //internal flags and helper vars, these do not have real world equivalents
        public:
        bool _isHalted;

        private:
        bool AccumMode = false;
        unsigned char fetchedByte = 0x00;
        unsigned short fetchedAddress = 0x0000;
        unsigned long internalClock = 0;


        //constructor
        public:
        CPU(BUS bus);

        void StepTop(unsigned long masterClockCycles);
        void NMI();

        private:
        unsigned char _opCode;

        public: 
        void Clock();

        private:
        void Startup();
        void ShutDown();
        void Reset();

        //opcodes
        void JAM();


        

    };
}