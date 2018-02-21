using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class PPU : IBusSlave
    {
        ushort IBusSlave.MemoryMapSize => 8;

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
