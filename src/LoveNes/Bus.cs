using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    /// <summary>
    /// 总线
    /// </summary>
    public class Bus : IBusMasterClient
    {
        private readonly SortedList<ushort, (IBusSlave slave, ushort size)> _slavesRead;
        private readonly SortedList<ushort, (IBusSlave slave, ushort size)> _slavesWrite;

        private bool _dirty = true;
        private readonly (IBusSlave slave, ushort offset)?[] _readMap;
        private readonly (IBusSlave slave, ushort offset)?[] _writeMap;

        /// <summary>
        /// Master 外设客户端
        /// </summary>
        public IBusMasterClient MasterClient => this;

        /// <inheritdoc/>
        public byte Value { get; set; }

        private bool _isUsed;

        public Bus()
        {
            _slavesRead = new SortedList<ushort, (IBusSlave slave, ushort size)>(Comparer<ushort>.Create((x, y) => y - x));
            _slavesWrite = new SortedList<ushort, (IBusSlave slave, ushort size)>(Comparer<ushort>.Create((x, y) => y - x));

            _readMap = new (IBusSlave slave, ushort offset)?[0x10000];
            _writeMap = new (IBusSlave slave, ushort offset)?[0x10000];
        }

        void IBusMasterClient.Acquire()
        {
            _isUsed = true;
        }

        bool IBusMasterClient.TryAcquire()
        {
            if (!_isUsed)
                return true;

            return false;
        }

        void IBusMasterClient.Release()
        {
            _isUsed = false;
        }

        void IBusMasterClient.Read(ushort address)
        {
            var slave = FindSlave(address, SlaveAccess.Read);
            Value = slave.slave.Read(slave.offset);
        }

        void IBusMasterClient.Write(ushort address)
        {
            var slave = FindSlave(address, SlaveAccess.Write);
            slave.slave.Write(slave.offset, Value);
        }

        /// <summary>
        /// 添加 Slave 外设
        /// </summary>
        /// <param name="baseAddress">基地址</param>
        /// <param name="slave">Slave 外设</param>
        public void AddSlave(ushort baseAddress, IBusSlave slave, SlaveAccess slaveAccess = SlaveAccess.Read | SlaveAccess.Write, ushort? memoryMapSize = null)
        {
            var newRange = new Range { Start = baseAddress, End = (ushort)(baseAddress + (memoryMapSize ?? slave.MemoryMapSize)) };

            void AddSlave(SortedList<ushort, (IBusSlave slave, ushort size)> slaves)
            {
                foreach (var slavePair in slaves)
                {
                    var range = new Range { Start = slavePair.Key, End = (ushort)(slavePair.Key + slavePair.Value.size) };
                    if (newRange.Overlaps(range))
                        throw new ArgumentOutOfRangeException($"Memory address overlaps: {range} with {newRange}.");
                }

                slaves.Add(baseAddress, (slave, memoryMapSize ?? slave.MemoryMapSize));
            }

            if (slaveAccess.HasFlag(SlaveAccess.Read))
                AddSlave(_slavesRead);
            if (slaveAccess.HasFlag(SlaveAccess.Write))
                AddSlave(_slavesWrite);

            _dirty = true;
        }

        [Flags]
        public enum SlaveAccess
        {
            Read = 0x1,
            Write = 0x2
        }

        private (IBusSlave slave, ushort offset) FindSlave(ushort address, SlaveAccess slaveAccess)
        {
            if (_dirty)
            {
                for (int i = 0; i <= 0xFFFF; i++)
                {
                    _readMap[i] = FindSlaveRaw((ushort)i, SlaveAccess.Read);
                    _writeMap[i] = FindSlaveRaw((ushort)i, SlaveAccess.Write);
                }

                _dirty = false;
            }

            if (slaveAccess == SlaveAccess.Read)
                return _readMap[address] ?? throw new AccessViolationException($"cannot find slave in address: 0x{address:X}.");
            else
                return _writeMap[address] ?? throw new AccessViolationException($"cannot find slave in address: 0x{address:X}.");
        }

        /// <summary>
        /// 片选
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>选中的 Slave 外设和内存地址偏移</returns>
        private (IBusSlave slave, ushort offset)? FindSlaveRaw(ushort address, SlaveAccess slaveAccess)
        {
            var slaves = slaveAccess == SlaveAccess.Read ? _slavesRead : _slavesWrite;
            foreach (var slave in slaves)
            {
                if (slave.Key <= address)
                {
                    var offset = (ushort)(address - slave.Key);
                    if (offset < slave.Value.size)
                        return (slave.Value.slave, offset);
                }
            }

            return null;
        }

        private class DummySlave : IBusSlave
        {
            public static DummySlave Instance { get; } = new DummySlave();

            ushort IBusSlave.MemoryMapSize => ushort.MaxValue;

            byte IBusSlave.Read(ushort address)
            {
                return 0;
            }

            void IBusSlave.Write(ushort address, byte value)
            {
            }
        }

        private struct Range
        {
            public ushort Start;

            public ushort End;

            public bool Overlaps(Range other)
            {
                return (Start >= other.Start && other.End > Start) || (other.Start >= Start && End > other.Start);
            }

            public override string ToString()
            {
                return $"0x{Start:X} - 0x{End:X}";
            }
        }
    }

    /// <summary>
    /// 总线 Master 客户端
    /// </summary>
    public interface IBusMasterClient
    {
        void Acquire();

        bool TryAcquire();

        void Release();

        /// <summary>
        /// 值
        /// </summary>
        byte Value { get; set; }

        /// <summary>
        /// 读
        /// </summary>
        /// <param name="address">地址</param>
        void Read(ushort address);

        /// <summary>
        /// 写
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        void Write(ushort address);
    }

    /// <summary>
    /// 总线 Slave
    /// </summary>
    public interface IBusSlave
    {
        /// <summary>
        /// 内存映射大小
        /// </summary>
        ushort MemoryMapSize { get; }

        /// <summary>
        /// 读
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>值</returns>
        byte Read(ushort address);

        /// <summary>
        /// 写
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        void Write(ushort address, byte value);
    }
}
