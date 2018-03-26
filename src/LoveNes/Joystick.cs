using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class Joystick : IBusSlave
    {
        ushort IBusSlave.MemoryMapSize => 1;

        byte IBusSlave.Read(ushort address)
        {
            return 0;
        }

        void IBusSlave.Write(ushort address, byte value)
        {
        }
    }
}
