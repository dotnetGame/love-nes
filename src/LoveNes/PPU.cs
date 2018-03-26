using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using LoveNes.Host;

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
        private ushort _cntTile;

        private ushort _nametableBaseAddr;
        private ushort _bgPatternTableBaseAddr;

        private ushort _ppuAddr;
        private bool _writingPPUAddrLow;

        private byte _cameraPosX;
        private byte _cameraPosY;
        private bool _writingCameraPosY;

        private readonly IBusMasterClient _masterClient;
        private readonly IInterruptReceiver _interruptReceiver;
        private readonly IHostGraphics _hostGraphics;

        public PPU(IBusMasterClient busMasterClient, IInterruptReceiver interruptReceiver, IHostGraphics hostGraphics)
        {
            _masterClient = busMasterClient;
            _interruptReceiver = interruptReceiver;
            _hostGraphics = hostGraphics;
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
            UpdateBgPatternTableBaseAddress();
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
            if (_dot == 1 && _scanline == 241)
            {
                _cntTile = 0;
                _status.V = true;
                if (_controller.V)
                    _interruptReceiver.Interrupt(InterruptType.NMI);

                _hostGraphics.Flip();
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
        private byte _attribute;
        private byte _bitmapLow;
        private byte _bitmapHigh;

        private void DoVisibleScanline()
        {
            if (_dot >= 1 && _dot <= 256)
            {
                var tX = (byte)(_cntTile % 32);
                var tY = (byte)(_cntTile / 32 / 8);
                var tileId = tY * 32 + tX;
                var bX = tX / 2;
                var bY = tY / 2;
                var aX = bX / 2;
                var aY = bY / 2;
                var aId = aY * 8 + aX;
                var attrShift = (bY % 2 * 2 + bX % 2) * 2;

                switch (_nextTileFetchStatus)
                {
                    case TileFetchStatus.Nametable_1:
                        _masterClient.Read((ushort)(_nametableBaseAddr + tileId));
                        _nextTileFetchStatus = TileFetchStatus.Nametable_2;
                        break;
                    case TileFetchStatus.Nametable_2:
                        _nametable = _masterClient.Value;
                        _nextTileFetchStatus = TileFetchStatus.Attribute_1;
                        break;
                    case TileFetchStatus.Attribute_1:
                        _masterClient.Read((ushort)(0x23C0 + aId));
                        _nextTileFetchStatus = TileFetchStatus.Attribute_2;
                        break;
                    case TileFetchStatus.Attribute_2:
                        _attribute = (byte)((_masterClient.Value & (0b11 << attrShift)) >> attrShift);
                        _nextTileFetchStatus = TileFetchStatus.BitmapLow_1;
                        break;
                    case TileFetchStatus.BitmapLow_1:
                        _masterClient.Read(GetBgPatternTableAddress());
                        _nextTileFetchStatus = TileFetchStatus.BitmapLow_2;
                        break;
                    case TileFetchStatus.BitmapLow_2:
                        _bitmapLow = _masterClient.Value;
                        _nextTileFetchStatus = TileFetchStatus.BitmapHigh_1;
                        break;
                    case TileFetchStatus.BitmapHigh_1:
                        _masterClient.Read((ushort)(GetBgPatternTableAddress() + 8));
                        _nextTileFetchStatus = TileFetchStatus.BitmapHigh_2;
                        break;
                    case TileFetchStatus.BitmapHigh_2:
                        _bitmapHigh = _masterClient.Value;
                        OutputPixel();
                        _cntTile++;
                        _nextTileFetchStatus = TileFetchStatus.Nametable_1;
                        break;
                }
            }
        }

        private static readonly uint[] _nesRgb = new uint[]
        {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
            0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
            0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
            0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
            0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
            0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
            0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };

        private void OutputPixel()
        {
            var tX = (byte)(_cntTile % 32);
            var tY = (byte)(_cntTile / 32 / 8);
            var pY = (byte)(_cntTile / 32);

            for (byte x = 0; x < 8; x++)
            {
                var shift = 7 - x;
                var mask = 1 << shift;
                var paletteId = ((_bitmapLow & mask) | ((_bitmapHigh & mask) << 1)) >> shift;

                _masterClient.Read((ushort)(0x3F00 + _attribute * 4 + paletteId));
                var palette = _masterClient.Value;

                _hostGraphics.DrawPixel((byte)(tX * 8 + x), pY, (uint)(_nesRgb[palette] | 0xFF000000));
            }
        }

        private ushort GetBgPatternTableAddress()
        {
            var pY = (byte)(_cntTile / 32);
            pY %= 8;
            return (ushort)(_bgPatternTableBaseAddr + _nametable * 16 + pY);
        }

        byte IBusSlave.Read(ushort address)
        {
            if (address == 0x0002)
            {
                var value = _status.Value;
                _status.V = false;
                _writingPPUAddrLow = false;
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
                UpdateBgPatternTableBaseAddress();
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
                    _cameraPosY = value;
                else
                    _cameraPosX = value;
                _writingCameraPosY = !_writingCameraPosY;
            }
            else if (address == 0x0006)
            {
                if (_writingPPUAddrLow)
                    _ppuAddr |= value;
                else
                    _ppuAddr = (ushort)(value << 8);

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

        private void UpdateBgPatternTableBaseAddress()
        {
            _bgPatternTableBaseAddr = _controller.B ? (ushort)0x1000 : (ushort)0;
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
            get => _value[0b100];
            set => _value[0b100] = value;
        }

        public bool S
        {
            get => _value[0b1000];
            set => _value[0b1000] = value;
        }

        public bool B
        {
            get => _value[0b1_0000];
            set => _value[0b1_0000] = value;
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
