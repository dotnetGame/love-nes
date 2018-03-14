using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class PPU : IBusSlave, IClockSink
    {
        ushort IBusSlave.MemoryMapSize => 8;

        void IClockSink.OnPowerUp()
        {
        }

        void IClockSink.OnReset()
        {
        }

        void IClockSink.OnTick()
        {
        }

        byte IBusSlave.Read(ushort address)
        {
            return 0;
        }

        void IBusSlave.Write(ushort address, byte value)
        {
        }
    }
}
