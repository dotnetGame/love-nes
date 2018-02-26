using System;
using System.Collections.Generic;
using System.Text;
using LoveNes.IO;
using LoveNes.Mappers;

namespace LoveNes
{
    public class Cartridge : IBusSlave
    {
        ushort IBusSlave.MemoryMapSize => 0xBFE0;

        private Mapper0 _mapper;

        public Cartridge()
        {
        }

        public void InsertNesFile(NesFile nesFile)
        {
            _mapper = new Mapper0(nesFile);
        }

        byte IBusSlave.Read(ushort address)
        {
            return _mapper.Read(address);
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            _mapper.Write(address, value);
        }
    }
}
