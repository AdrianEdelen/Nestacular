#include "BUS.h"
using namespace NES;

void NES::BUS::Write(unsigned short address, unsigned char data)
{
    if (address >= 0x0000 && address <= 0xFFFF)
    {
        RAM[address] = data;
    }
}

unsigned char NES::BUS::Read(unsigned short address, unsigned char data, bool readOnly = false)
{
    if (address >= 0x0000 && address <= 0xFFFF)
    {
        return RAM[address];
    }
    else
    {
        return 0x00;
    }
}