using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class OamDmaController : IBusSlave, IClockSink
    {
        ushort IBusSlave.MemoryMapSize => 1;

        private IBusMasterClient _masterClient;
        private byte _writeCount;
        private bool _enabled;
        private ushort _srcAddr;

        public OamDmaController(IBusMasterClient masterClient)
        {
            _masterClient = masterClient;
        }

        byte IBusSlave.Read(ushort address)
        {
            throw new NotSupportedException();
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            if (address == 0x00)
                StartOMADMA(value);
            else
                throw new NotSupportedException();
        }

        private void StartOMADMA(byte page)
        {
            _writeCount = 0;
            _srcAddr = (ushort)(page << 8);
            _masterClient.Acquire();
            _enabled = true;
        }

        void IClockSink.OnTick()
        {
            if (_enabled)
            {
                _masterClient.Read(_srcAddr++);
                _masterClient.Write(0x2014);
                _writeCount++;

                if (_writeCount == 0)
                {
                    _enabled = false;
                    _masterClient.Release();
                }
            }
        }

        void IClockSink.OnPowerUp()
        {
        }

        void IClockSink.OnReset()
        {
        }
    }
}
