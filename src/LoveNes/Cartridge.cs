using System;
using System.Collections.Generic;
using System.Text;
using LoveNes.IO;
using LoveNes.Mappers;

namespace LoveNes
{
    public class Cartridge
    {
        private Mapper0 _mapper;

        public IBusSlave CPUSlave { get; }

        public IBusSlave PPUSlave { get; }

        public Cartridge()
        {
            CPUSlave = new CPUSlaveProvidere(this);
            PPUSlave = new PPUSlaveProvidere(this);
        }

        public void InsertNesFile(NesFile nesFile)
        {
            _mapper = new Mapper0(nesFile);
        }

        private class CPUSlaveProvidere : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 0xBFE0;

            private readonly Cartridge _cartridge;

            public CPUSlaveProvidere(Cartridge cartridge)
            {
                _cartridge = cartridge;
            }

            byte IBusSlave.Read(ushort address)
            {
                return _cartridge._mapper.Read(address);
            }

            void IBusSlave.Write(ushort address, byte value)
            {
                _cartridge._mapper.Write(address, value);
            }
        }

        private class PPUSlaveProvidere : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 0x2000;

            private readonly Cartridge _cartridge;

            public PPUSlaveProvidere(Cartridge cartridge)
            {
                _cartridge = cartridge;
            }

            byte IBusSlave.Read(ushort address)
            {
                return _cartridge._mapper.Read(address);
            }

            void IBusSlave.Write(ushort address, byte value)
            {
                _cartridge._mapper.Write(address, value);
            }
        }
    }
}
