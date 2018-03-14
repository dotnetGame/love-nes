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
            throw new NotImplementedException();
        }

        void IClockSink.OnReset()
        {
            throw new NotImplementedException();
        }

        void IClockSink.OnTick()
        {
            throw new NotImplementedException();
        }

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
