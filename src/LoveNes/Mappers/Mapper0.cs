using System;
using System.Collections.Generic;
using System.Text;
using LoveNes.IO;

namespace LoveNes.Mappers
{
    internal class Mapper0
    {
        private readonly NesFile _nesFile;

        public Mapper0(NesFile nesFile)
        {
            _nesFile = nesFile;
        }

        public byte Read(ushort address)
        {
            // Last 16 KB of ROM
            if (Offset(address, 0x7FE0, out var offset))
            {
                if (_nesFile.PrgRomSize == 0x8000)
                    return _nesFile.PrgRom[offset + 0x4000];
                else if (_nesFile.PrgRomSize == 0x4000)
                    return _nesFile.PrgRom[offset];
            }

            // First 16 KB of ROM
            else if (Offset(address, 0x3FE0, out offset))
            {
                return _nesFile.PrgRom[offset];
            }

            throw new NotSupportedException();
        }

        private static bool Offset(ushort address, ushort baseAddress, out ushort offset)
        {
            offset = (ushort)(address - baseAddress);
            return address >= baseAddress;
        }

        public void Write(ushort address, byte value)
        {
            throw new NotSupportedException();
        }

        public byte ReadPPU(ushort address)
        {
            return _nesFile.ChrRom[address];
        }

        public void WritePPU(ushort address, byte value)
        {
            throw new NotSupportedException();
        }
    }
}
