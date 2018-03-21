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

        public MirroringMode MirroringMode { get; set; }

        private PPUStatus _status;
        private PPUController _controller;
        private PPUMask _mask;
        private byte _oamAddress;

        private readonly byte[] _oamMemory;

        private ushort _scanline;
        private ushort _dot;
        private ushort _cntTile;

        private ushort _nametableBaseAddr;

        private ushort _ppuAddr;
        private bool _writingPPUAddrLow;

        private byte _cameraPosX;
        private byte _cameraPosY;
        private bool _writingCameraPosY;

        private readonly IBusMasterClient _masterClient;
        private readonly IInterruptReceiver _interruptReceiver;

        public PPU(IBusMasterClient busMasterClient, IInterruptReceiver interruptReceiver)
        {
            _masterClient = busMasterClient;
            _interruptReceiver = interruptReceiver;
            _oamMemory = new byte[64 * 4];
        }

        void IClockSink.OnPowerUp()
        {
            _status.Value = 0;

            _scanline = 0;
            _dot = 0;
            _cntTile = 0;
            _nextTileFetchStatus = TileFetchStatus.Nametable_1;

            UpdateNametableBaseAddress();
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
                _nextTileFetchStatus = TileFetchStatus.Nametable_1;

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
                if (_controller.V)
                    _interruptReceiver.Interrupt(InterruptType.NMI);
            }
        }

        private void DoPostRenderScanline()
        {
        }

        private enum TileFetchStatus
        {
            Nametable_1,
            Nametable_2,
            Attribute_1,
            Attribute_2,
            BitmapLow_1,
            BitmapLow_2,
            BitmapHigh_1,
            BitmapHigh_2
        }

        private TileFetchStatus _nextTileFetchStatus;
        private byte _nametable;
        /*private byte _attribute;
        private byte _bitmapLow;
        private byte _bitmapHigh;*/

        private void DoVisibleScanline()
        {
            if (_dot >= 1 && _dot <= 256)
            {
                switch (_nextTileFetchStatus)
                {
                    case TileFetchStatus.Nametable_1:
                        _masterClient.Read(MirrorNametableAddress((ushort)(_nametableBaseAddr + _cntTile)));
                        _nextTileFetchStatus = TileFetchStatus.Nametable_2;
                        break;
                    case TileFetchStatus.Nametable_2:
                        _nametable = _masterClient.Value;
                        _nextTileFetchStatus = TileFetchStatus.Attribute_1;
                        break;
                    case TileFetchStatus.Attribute_1:
                        break;
                    case TileFetchStatus.Attribute_2:
                        break;
                    case TileFetchStatus.BitmapLow_1:
                        break;
                    case TileFetchStatus.BitmapLow_2:
                        break;
                    case TileFetchStatus.BitmapHigh_1:
                        break;
                    case TileFetchStatus.BitmapHigh_2:
                        break;
                    default:
                        break;
                }
            }
        }

        private ushort MirrorNametableAddress(ushort address)
        {
            switch (MirroringMode)
            {
                case MirroringMode.Horizontal:
                    return (ushort)(((address / 2) & 0x400) + (address % 0x400) + 0x2000);
                case MirroringMode.Vertical:
                    return (ushort)(address % 0x800 + 0x2000);
                default:
                    throw new ArgumentOutOfRangeException(nameof(MirroringMode));
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
            {
                _controller.Value = value;
                UpdateNametableBaseAddress();
            }
            else if (address == 0x0001)
            {
                _mask.Value = value;
            }
            else if (address == 0x0003)
            {
                _oamAddress = value;
            }
            else if (address == 0x0004)
            {
                _oamMemory[_oamAddress++] = value;
            }
            else if (address == 0x0005)
            {
                if (_writingCameraPosY)
                    _cameraPosX = value;
                else
                    _cameraPosY = value;
                _writingCameraPosY = !_writingCameraPosY;
            }
            else if (address == 0x0006)
            {
                if (_writingPPUAddrLow)
                    _ppuAddr = (ushort)((_ppuAddr & 0xFF00) | value);
                else
                    _ppuAddr = (ushort)((_ppuAddr & 0xFF) | (value << 8));
                _writingPPUAddrLow = !_writingPPUAddrLow;
            }
            else if (address == 0x0007)
            {
                _masterClient.Value = value;
                _masterClient.Write(_ppuAddr);
                _ppuAddr += _controller.I ? (byte)32 : (byte)1;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void UpdateNametableBaseAddress()
        {
            switch (_controller.N)
            {
                case 0:
                    _nametableBaseAddr = 0x2000;
                    break;
                case 1:
                    _nametableBaseAddr = 0x2400;
                    break;
                case 2:
                    _nametableBaseAddr = 0x2800;
                    break;
                case 3:
                    _nametableBaseAddr = 0x2C00;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_controller.N));
            }
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

        public bool V
        {
            get => _value[0b1000_0000];
            set => _value[0b1000_0000] = value;
        }

        public byte N
        {
            get => (byte)(_value & 0b11);
            set => _value = (byte)((_value & ~0b11) | (value & 0b11));
        }

        public bool I
        {
            get => _value[0b1000];
            set => _value[0b1000] = value;
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

    public enum MirroringMode
    {
        Horizontal,
        Vertical
    }
}
