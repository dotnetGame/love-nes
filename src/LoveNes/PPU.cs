using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace LoveNes
{
    public class PPU : IBusSlave, IClockSink
    {
        ushort IBusSlave.MemoryMapSize => 8;

        private PPUStatus _status;
        private PPUController _controller;
        private PPUMask _mask;
        private byte _oamAddress;

        private readonly byte[] _oamMemory;

        private ushort _scanline;
        private ushort _dot;

        public PPU()
        {
            _oamMemory = new byte[64 * 4];
        }

        void IClockSink.OnPowerUp()
        {
            _status.Value = 0;

            _scanline = 0;
            _dot = 0;
        }

        void IClockSink.OnReset()
        {
            _scanline = 0;
            _dot = 0;
        }

        void IClockSink.OnTick()
        {
            if (_scanline <= 239)
            {
                DoVisibleScanline();
            }
            else if (_scanline == 240)
            {
                DoPostRenderScanline();
            }
            else if (_scanline <= 260)
            {
                DoVerticalBlankingLine();
            }
            else
            {
                DoPreRenderScanline();
            }

            if (++_dot > 340)
            {
                _dot %= 341;
                if (++_scanline > 261)
                {
                    _scanline = 0;
                }
            }
        }

        private void DoPreRenderScanline()
        {
            if (_dot == 1)
            {
                _status.V = false;
            }
        }

        private void DoVerticalBlankingLine()
        {
            if (_dot == 1)
            {
                _status.V = true;
            }
        }

        private void DoPostRenderScanline()
        {
        }

        private void DoVisibleScanline()
        {
            if (_dot >= 1 && _dot <= 256)
            {
            }
        }

        byte IBusSlave.Read(ushort address)
        {
            if (address == 0x0002)
            {
                var value = _status.Value;
                _status.V = false;
                return value;
            }

            throw new NotImplementedException();
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            if (address == 0x0000)
                _controller.Value = value;
            else if (address == 0x0001)
                _mask.Value = value;
            else if (address == 0x0003)
                _oamAddress = value;
            else if (address == 0x0004)
                _oamMemory[_oamAddress++] = value;
            else
                throw new NotImplementedException();
        }
    }

    public struct PPUStatus
    {
        private BitVector8 _value;

        public byte Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Sprite Overflow
        /// </summary>
        public bool O
        {
            get => _value[0b10_0000];
            set => _value[0b10_0000] = value;
        }

        /// <summary>
        /// Sprite 0 Hit
        /// </summary>
        public bool S
        {
            get => _value[0b100_0000];
            set => _value[0b100_0000] = value;
        }

        /// <summary>
        /// Vertical blank has started
        /// </summary>
        public bool V
        {
            get => _value[0b1000_0000];
            set => _value[0b1000_0000] = value;
        }
    }

    public struct PPUController
    {
        private BitVector8 _value;

        public byte Value
        {
            get => _value;
            set => _value = value;
        }
    }

    public struct PPUMask
    {
        private BitVector8 _value;

        public byte Value
        {
            get => _value;
            set => _value = value;
        }
    }
}
