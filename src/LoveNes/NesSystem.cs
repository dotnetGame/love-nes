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
        private readonly Bus _cpuBus;
        private readonly CPU _cpu;
        private readonly OnChipRAM _cpuOnChipRAM;

        private readonly APU _apu;

        private readonly PPU _ppu;
        private readonly Bus _ppuBus;

        public Cartridge Cartridge { get; }

        public NesSystem()
        {
            _clock = new Clock();
            _cpuBus = new Bus();

            // CPU
            _cpu = new CPU(_cpuBus.MasterClient);
            _clock.AddSink(_cpu);

            // 片上 RAM
            _cpuOnChipRAM = new OnChipRAM(OnChipRAMSize);
            _clock.AddSink(_cpuOnChipRAM);

            // APU
            _apu = new APU();

            _ppuBus = new Bus();
            _ppu = new PPU(_ppuBus, _cpu);
            _clock.AddSink(_ppu);

            // 板卡
            Cartridge = new Cartridge(_ppu);

            SetupCPUMemoryMap();
            SetupPPUMemoryMap();
        }

        private void SetupCPUMemoryMap()
        {
            // 片上 RAM
            _cpuBus.AddSlave(0x0000, _cpuOnChipRAM);
            _cpuBus.AddSlave(0x0800, _cpuOnChipRAM);
            _cpuBus.AddSlave(0x1000, _cpuOnChipRAM);
            _cpuBus.AddSlave(0x1800, _cpuOnChipRAM);

            // 板卡
            _cpuBus.AddSlave(0x4020, Cartridge.CPUSlave);

            // PPU
            for (ushort i = 0x2000; i < 0x3FFF; i += 8)
                _cpuBus.AddSlave(i, _ppu);

            // APU
            _cpuBus.AddSlave(0x4000, _apu);
            _cpuBus.AddSlave(0x4015, _apu.Status);
            _cpuBus.AddSlave(0x4017, _apu.FrameCounter, Bus.SlaveAccess.Write);

            // OAM DMA
            var oamDma = new OamDmaController(_cpuBus.MasterClient);
            _clock.AddSink(oamDma);
            _cpuBus.AddSlave(0x4014, oamDma, Bus.SlaveAccess.Write);
        }

        private void SetupPPUMemoryMap()
        {
            _ppuBus.AddSlave(0x0000, Cartridge.ChrRom);

            var nametable0 = new OnChipRAM(0x400);
            _ppuBus.AddSlave(0x2000, nametable0);
            var nametable1 = new OnChipRAM(0x400);
            _ppuBus.AddSlave(0x2400, nametable1);
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
