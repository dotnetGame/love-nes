using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public class NametableMirrorController : IBusSlave
    {
        ushort IBusSlave.MemoryMapSize => 0x400 * 4;

        public MirroringMode MirroringMode { get; set; }

        private readonly byte[] _nametable0;
        private readonly byte[] _nametable1;

        public NametableMirrorController()
        {
            _nametable0 = new byte[0x400];
            _nametable1 = new byte[0x400];
        }

        byte IBusSlave.Read(ushort address)
        {
            return MirrorNametableAddress(address);
        }

        void IBusSlave.Write(ushort address, byte value)
        {
            MirrorNametableAddress(address) = value;
        }

        private ref byte MirrorNametableAddress(ushort address)
        {
            switch (MirroringMode)
            {
                case MirroringMode.Horizontal:
                    if (Offset(address, 0xC00, out var offset))
                        return ref _nametable1[offset];
                    else if (Offset(address, 0x800, out offset))
                        return ref _nametable1[offset];
                    else if (Offset(address, 0x400, out offset))
                        return ref _nametable0[offset];
                    else
                        return ref _nametable0[address];
                case MirroringMode.Vertical:
                    if (Offset(address, 0xC00, out offset))
                        return ref _nametable1[offset];
                    else if (Offset(address, 0x800, out offset))
                        return ref _nametable0[offset];
                    else if (Offset(address, 0x400, out offset))
                        return ref _nametable1[offset];
                    else
                        return ref _nametable0[address];
                default:
                    throw new ArgumentOutOfRangeException(nameof(MirroringMode));
            }
        }

        private static bool Offset(ushort address, ushort baseAddress, out ushort offset)
        {
            offset = (ushort)(address - baseAddress);
            return address >= baseAddress;
        }
    }
}
