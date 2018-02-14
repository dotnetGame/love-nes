using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    /// <summary>
    /// 片上 RAM
    /// </summary>
    public class OnChipRAM : IBusSlave, IClockSink
    {
        ushort IBusSlave.MemoryMapSize => _size;

        private readonly ushort _size;
        private readonly byte[] _ram;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChipRAM"/> class.
        /// </summary>
        /// <param name="size">内存大小</param>
        public OnChipRAM(ushort size)
        {
            _size = size;
            _ram = new byte[size];
        }

        void IClockSink.OnPowerUp()
        {
            Array.Clear(_ram, 0, _ram.Length);
        }

        void IClockSink.OnReset()
        {
        }

        void IClockSink.OnTick()
        {
        }

        byte IBusSlave.Read(ushort address)
        {
            return _ram[address];
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            _ram[address] = value;
        }
    }
}
