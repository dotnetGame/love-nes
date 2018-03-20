using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LoveNes
{
    public class APU
    {
        public IBusSlave FrameController { get; }

        public APU()
        {
            FrameController = new FrameControllerSlave(this);
        }

        private class FrameControllerSlave : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 1;

            private readonly APU _apu;

            public FrameControllerSlave(APU apu)
            {
                _apu = apu;
            }

            byte IBusSlave.Read(ushort address)
            {
                throw new NotSupportedException();
            }

            void IBusSlave.Write(ushort address, byte value)
            {
                if (address == 0x00)
                    Debug.WriteLine("APU Frame Controller Write.");
                else
                    throw new NotSupportedException();
            }
        }
    }
}
