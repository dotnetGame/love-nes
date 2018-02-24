using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes.Mappers
{
    internal class Mapper0
    {
        private readonly byte[] _rom;

        public Mapper0(byte[] rom)
        {
            _rom = rom;
        }

        public byte Read(ushort address)
        {
            return _rom[address];
        }

        public void Write(ushort address, byte value)
        {
            _rom[address] = value;
        }
    }
}
