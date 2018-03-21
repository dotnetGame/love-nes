using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LoveNes
{
    public class APU : IBusSlave
    {
        public IBusSlave FrameCounter { get; }

        public IBusSlave Status { get; }

        ushort IBusSlave.MemoryMapSize => 0x14;

        public APU()
        {
            FrameCounter = new APURegisterSlave(null, v => Debug.WriteLine("APU Frame Counter"));
            Status = new APURegisterSlave(
                () =>
            {
                Debug.WriteLine("Read APU Status");
                return 0;
            }, v => Debug.WriteLine("Write APU Status"));
        }

        byte IBusSlave.Read(ushort address)
        {
            if (Offset(address, 0x10, out var offset))
                return ReadDMC(offset);
            else if (Offset(address, 0x0C, out offset))
                return ReadNoise(offset);
            else if (Offset(address, 0x08, out offset))
                return ReadTriangle(offset);
            else if (Offset(address, 0x04, out offset))
                return ReadPulse(1, offset);
            else
                return ReadPulse(0, address);
        }

        private byte ReadPulse(byte id, ushort offset)
        {
            throw new NotImplementedException();
        }

        private byte ReadTriangle(ushort offset)
        {
            throw new NotImplementedException();
        }

        private byte ReadNoise(ushort offset)
        {
            throw new NotImplementedException();
        }

        private byte ReadDMC(ushort offset)
        {
            throw new NotImplementedException();
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            if (Offset(address, 0x10, out var offset))
                WriteDMC(offset, value);
            else if (Offset(address, 0x0C, out offset))
                WriteNoise(offset, value);
            else if (Offset(address, 0x08, out offset))
                WriteTriangle(offset, value);
            else if (Offset(address, 0x04, out offset))
                WritePulse(1, offset, value);
            else
                WritePulse(0, address, value);
        }

        private void WritePulse(byte id, ushort offset, byte value)
        {
            Debug.WriteLine($"Write APU Pulse {id}");
        }

        private void WriteTriangle(ushort offset, byte value)
        {
            Debug.WriteLine("Write APU Triangle");
        }

        private void WriteNoise(ushort offset, byte value)
        {
            Debug.WriteLine("Write APU Noise");
        }

        private void WriteDMC(ushort offset, byte value)
        {
            Debug.WriteLine("Write APU DMC");
        }

        private static bool Offset(ushort address, ushort baseAddress, out ushort offset)
        {
            offset = (ushort)(address - baseAddress);
            return address >= baseAddress;
        }

        private class APURegisterSlave : IBusSlave
        {
            ushort IBusSlave.MemoryMapSize => 1;

            private readonly Func<byte> _getter;
            private readonly Action<byte> _setter;

            public APURegisterSlave(Func<byte> getter, Action<byte> setter)
            {
                _getter = getter;
                _setter = setter;
            }

            byte IBusSlave.Read(ushort address)
                => (_getter ?? throw new NotSupportedException())();

            void IBusSlave.Write(ushort address, byte value)
            {
                if (address != 0 || _setter == null)
                    throw new NotSupportedException();
                _setter(value);
            }
        }
    }
}
