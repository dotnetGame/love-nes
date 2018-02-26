using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    /// <summary>
    /// NES 系统
    /// </summary>
    public class NesSystem
    {
        /// <summary>
        /// 片上 RAM 大小 (2KB)
        /// </summary>
        public const ushort OnChipRAMSize = 0x800;

        private readonly Clock _clock;
        private readonly Bus _bus;
        private readonly CPU _cpu;
        private readonly OnChipRAM _onChipRAM;

        public Cartridge Cartridge { get; }

        public NesSystem()
        {
            _clock = new Clock();
            _bus = new Bus();

            // CPU
            _cpu = new CPU(_bus.MasterClient);
            _clock.AddSink(_cpu);

            // 片上 RAM
            _onChipRAM = new OnChipRAM(OnChipRAMSize);
            _clock.AddSink(_onChipRAM);
            _bus.AddSlave(0x0000, _onChipRAM);
            _bus.AddSlave(0x0800, _onChipRAM);
            _bus.AddSlave(0x1000, _onChipRAM);
            _bus.AddSlave(0x1800, _onChipRAM);

            // 板卡
            Cartridge = new Cartridge();
            _bus.AddSlave(0x4020, Cartridge);
        }

        /// <summary>
        /// 上电
        /// </summary>
        public void PowerUp()
        {
            _clock.PowerUp();
        }

        /// <summary>
        /// 复位
        /// </summary>
        public void Reset()
        {
            _clock.Reset();
        }
    }
}
