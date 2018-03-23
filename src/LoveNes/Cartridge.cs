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
        private readonly NametableMirrorController _nametableMirrorController;

        public IBusSlave CPUSlave { get; }

        public IBusSlave ChrRom { get; }

        public Cartridge(NametableMirrorController nametableMirrorController)
        {
            _nametableMirrorController = nametableMirrorController;
            CPUSlave = new CPUSlaveProvider(this);
            ChrRom = new ChrRomProvider(this);
        }

        public void InsertNesFile(NesFile nesFile)
        {
            _nametableMirrorController.MirroringMode = nesFile.MirroringMode;
            _mapper = new Mapper0(nesFile);
        }

        private class CPUSlaveProvider : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 0xBFE0;

            private readonly Cartridge _cartridge;

            public CPUSlaveProvider(Cartridge cartridge)
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

        private class ChrRomProvider : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 0x2000;

            private readonly Cartridge _cartridge;

            public ChrRomProvider(Cartridge cartridge)
            {
                _cartridge = cartridge;
            }

            byte IBusSlave.Read(ushort address)
            {
                return _cartridge._mapper.ReadPPU(address);
            }

            void IBusSlave.Write(ushort address, byte value)
            {
                _cartridge._mapper.WritePPU(address, value);
            }
        }
    }
}
