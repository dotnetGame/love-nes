using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class Cartridge : IBusSlave
    {
        ushort IBusSlave.MemoryMapSize => 0xBFE0;

        byte IBusSlave.Read(ushort address)
        {
            throw new NotImplementedException();
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
