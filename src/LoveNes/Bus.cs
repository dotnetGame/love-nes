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
        private readonly SortedList<ushort, IBusSlave> _slaves;

        /// <summary>
        /// Master 外设客户端
        /// </summary>
        public IBusMasterClient MasterClient => this;

        /// <inheritdoc/>
        public byte Value { get; set; }

        public Bus()
        {
            _slaves = new SortedList<ushort, IBusSlave>(Comparer<ushort>.Create((x, y) => y - x));
        }

        void IBusMasterClient.Read(ushort address)
        {
            var slave = FindSlave(address);
            Value = slave.slave.Read(slave.offset);
        }

        void IBusMasterClient.Write(ushort address)
        {
            var slave = FindSlave(address);
            slave.slave.Write(slave.offset, Value);
        }

        /// <summary>
        /// 添加 Slave 外设
        /// </summary>
        /// <param name="baseAddress">基地址</param>
        /// <param name="slave">Slave 外设</param>
        public void AddSlave(ushort baseAddress, IBusSlave slave)
        {
            var newRange = new Range { Start = baseAddress, End = (ushort)(baseAddress + slave.MemoryMapSize) };
            foreach (var slavePair in _slaves)
            {
                var range = new Range { Start = slavePair.Key, End = (ushort)(slavePair.Key + slavePair.Value.MemoryMapSize) };
                if (newRange.Overlaps(range))
                    throw new ArgumentOutOfRangeException($"Memory address overlaps: {range} with {newRange}.");
            }

            _slaves.Add(baseAddress, slave);
        }

        /// <summary>
        /// 片选
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>选中的 Slave 外设和内存地址偏移</returns>
        private (IBusSlave slave, ushort offset) FindSlave(ushort address)
        {
            foreach (var slave in _slaves)
            {
                if (slave.Key <= address)
                {
                    var offset = (ushort)(address - slave.Key);
                    if (offset >= slave.Value.MemoryMapSize)
                    {
                        // throw new AccessViolationException($"address: 0x{address:X} is out of slave memory range.");
                        return (DummySlave.Instance, offset);
                    }

                    return (slave.Value, offset);
                }
            }

            throw new AccessViolationException($"cannot find slave in address: 0x{address:X}.");
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
