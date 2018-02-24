using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    /// <summary>
    /// 6502 CPU
    /// </summary>
    public class CPU : IClockSink
    {
        /// <summary>
        /// 寄存器组
        /// </summary>
        public ProcessorRegisters Registers;

        /// <summary>
        /// 状态
        /// </summary>
        public ProcessorStatus Status;

        private readonly IBusMasterClient _masterClient;
        private MicroCode _nextMicroCode;
        private OpCodeStatus _nextOpCodeStatus;

        /// <summary>
        /// 寻址结果
        /// </summary>
        private byte _addressResult;
        private byte _addressResult2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPU"/> class.
        /// </summary>
        /// <param name="busMaster">总线 Master 客户端</param>
        public CPU(IBusMasterClient masterClient)
        {
            _masterClient = masterClient;
        }

        private void ExecuteOpCode()
        {
            if (_nextMicroCode == MicroCode.None)
            {
                if (_nextOpCodeStatus == OpCodeStatus.None)
                {
                    _masterClient.Read(Registers.PC++);
                    ExecuteOpCode((OpCode)_masterClient.Value);
                }

                ExecuteOpCode(_nextOpCodeStatus);
            }

            ExecuteMicroCode(_nextMicroCode);
        }

        private void ExecuteOpCode(OpCode opCode)
        {
        }

        public enum OpCode : byte
        {
            ADC_Addressing_Immediate = 0x69
        }

        public enum OpCodeStatus : byte
        {
            /// <summary>
            /// 无
            /// </summary>
            None,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Immediate
            /// </summary>
            ADC_1_Addressing_Immediate,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Zero Page
            /// </summary>
            ADC_1_Addressing_ZeroPage,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Zero Page X
            /// </summary>
            ADC_1_Addressing_ZeroPageX,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Absolute
            /// </summary>
            ADC_1_Addressing_Absolute,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Absolute X
            /// </summary>
            ADC_1_Addressing_AbsoluteX,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Absolute Y
            /// </summary>
            ADC_1_Addressing_AbsoluteY,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Indexed Indirect
            /// </summary>
            ADC_1_Addressing_IndirectX,

            /// <summary>
            /// Add with Carry - Step 1 - Addressing Indirect Indexed
            /// </summary>
            ADC_1_Addressing_IndirectY,

            /// <summary>
            /// Add with Carry - Step 2
            /// </summary>
            ADC_2
        }

        private void ExecuteOpCode(OpCodeStatus code)
        {
            switch (code)
            {
                case OpCodeStatus.ADC_1_Addressing_Immediate:
                    _nextMicroCode = MicroCode.Immediate;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_ZeroPage:
                    _nextMicroCode = MicroCode.ZeroPage_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_ZeroPageX:
                    _nextMicroCode = MicroCode.ZeroPageX_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_Absolute:
                    _nextMicroCode = MicroCode.Absolute_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_AbsoluteX:
                    _nextMicroCode = MicroCode.AbsoluteX_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_AbsoluteY:
                    _nextMicroCode = MicroCode.AbsoluteY_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_IndirectX:
                    _nextMicroCode = MicroCode.IndirectX_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_1_Addressing_IndirectY:
                    _nextMicroCode = MicroCode.IndirectY_1;
                    _nextOpCodeStatus = OpCodeStatus.ADC_2;
                    break;
                case OpCodeStatus.ADC_2:
                    _nextMicroCode = MicroCode.ADC;
                    _nextOpCodeStatus = OpCodeStatus.None;
                    break;
                default:
                    throw new InvalidProgramException($"invalid op code status: 0x{code:X}.");
            }
        }

        private enum MicroCode : byte
        {
            /// <summary>
            /// 无
            /// </summary>
            None,

            /// <summary>
            /// Immediate Addressing
            /// </summary>
            Immediate,

            /// <summary>
            /// Zero Page Addressing - Step 1
            /// </summary>
            ZeroPage_1,

            /// <summary>
            /// Zero Page Addressing - Step 2
            /// </summary>
            ZeroPage_2,

            /// <summary>
            /// Zero Page X Addressing - Step 1
            /// </summary>
            ZeroPageX_1,

            /// <summary>
            /// Zero Page X Addressing - Step 2
            /// </summary>
            ZeroPageX_2,

            /// <summary>
            /// Zero Page X Addressing - Step 3
            /// </summary>
            ZeroPageX_3,

            /// <summary>
            /// Zero Page X Addressing - Step 1
            /// </summary>
            ZeroPageY_1,

            /// <summary>
            /// Zero Page Y Addressing - Step 2
            /// </summary>
            ZeroPageY_2,

            /// <summary>
            /// Zero Page Y Addressing - Step 3
            /// </summary>
            ZeroPageY_3,

            /// <summary>
            /// Absolute Addressing - Step 1
            /// </summary>
            Absolute_1,

            /// <summary>
            /// Absolute Addressing - Step 2
            /// </summary>
            Absolute_2,

            /// <summary>
            /// Absolute Addressing - Step 3
            /// </summary>
            Absolute_3,

            /// <summary>
            /// Absolute X Addressing - Step 1
            /// </summary>
            AbsoluteX_1,

            /// <summary>
            /// Absolute X Addressing - Step 2
            /// </summary>
            AbsoluteX_2,

            /// <summary>
            /// Absolute X Addressing - Step 3
            /// </summary>
            AbsoluteX_3,

            /// <summary>
            /// Absolute Y Addressing - Step 1
            /// </summary>
            AbsoluteY_1,

            /// <summary>
            /// Absolute Y Addressing - Step 2
            /// </summary>
            AbsoluteY_2,

            /// <summary>
            /// Absolute Y Addressing - Step 3
            /// </summary>
            AbsoluteY_3,

            /// <summary>
            /// Indexed Indirect - Step 1
            /// </summary>
            IndirectX_1,

            /// <summary>
            /// Indexed Indirect - Step 2
            /// </summary>
            IndirectX_2,

            /// <summary>
            /// Indexed Indirect - Step 3
            /// </summary>
            IndirectX_3,

            /// <summary>
            /// Indexed Indirect - Step 4
            /// </summary>
            IndirectX_4,

            /// <summary>
            /// Indexed Indirect - Step 5
            /// </summary>
            IndirectX_5,

            /// <summary>
            /// Indirect Indexed - Step 1
            /// </summary>
            IndirectY_1,

            /// <summary>
            /// Indirect Indexed - Step 2
            /// </summary>
            IndirectY_2,

            /// <summary>
            /// Indirect Indexed - Step 3
            /// </summary>
            IndirectY_3,

            /// <summary>
            /// Indirect Indexed - Step 4
            /// </summary>
            IndirectY_4,

            /// <summary>
            /// Add with Carry
            /// </summary>
            ADC
        }

        private void ExecuteMicroCode(MicroCode code)
        {
            switch (code)
            {
                case MicroCode.Immediate:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.ZeroPage_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.ZeroPage_2;
                    break;
                case MicroCode.ZeroPage_2:
                    _masterClient.Read(_addressResult);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.ZeroPageX_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.ZeroPageX_2;
                    break;
                case MicroCode.ZeroPageX_2:
                    _addressResult += Registers.X;
                    _nextMicroCode = MicroCode.ZeroPageX_3;
                    break;
                case MicroCode.ZeroPageX_3:
                    _masterClient.Read(_addressResult);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.ZeroPageY_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.ZeroPageY_2;
                    break;
                case MicroCode.ZeroPageY_2:
                    _addressResult += Registers.Y;
                    _nextMicroCode = MicroCode.ZeroPageY_3;
                    break;
                case MicroCode.ZeroPageY_3:
                    _masterClient.Read(_addressResult);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.Absolute_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.Absolute_2;
                    break;
                case MicroCode.Absolute_2:
                    _masterClient.Read(Registers.PC++);
                    _addressResult2 = _masterClient.Value;
                    _nextMicroCode = MicroCode.Absolute_3;
                    break;
                case MicroCode.Absolute_3:
                    _masterClient.Read((ushort)(_addressResult | _addressResult2 << 8));
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.AbsoluteX_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.AbsoluteX_2;
                    break;
                case MicroCode.AbsoluteX_2:
                    _masterClient.Read(Registers.PC++);
                    _addressResult2 = _masterClient.Value;
                    _nextMicroCode = MicroCode.AbsoluteX_3;
                    break;
                case MicroCode.AbsoluteX_3:
                    _masterClient.Read((ushort)((_addressResult | _addressResult2 << 8) + Registers.X));
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.AbsoluteY_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.AbsoluteX_2;
                    break;
                case MicroCode.AbsoluteY_2:
                    _masterClient.Read(Registers.PC++);
                    _addressResult2 = _masterClient.Value;
                    _nextMicroCode = MicroCode.AbsoluteX_3;
                    break;
                case MicroCode.AbsoluteY_3:
                    _masterClient.Read((ushort)((_addressResult | _addressResult2 << 8) + Registers.Y));
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.IndirectX_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectX_2;
                    break;
                case MicroCode.IndirectX_2:
                    _addressResult = (byte)(_addressResult + Registers.X);
                    _addressResult2 = (byte)(_addressResult + 1);
                    _nextMicroCode = MicroCode.IndirectX_3;
                    break;
                case MicroCode.IndirectX_3:
                    _masterClient.Read(_addressResult);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectX_4;
                    break;
                case MicroCode.IndirectX_4:
                    _masterClient.Read(_addressResult2);
                    _addressResult2 = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectX_5;
                    break;
                case MicroCode.IndirectX_5:
                    _masterClient.Read((ushort)(_addressResult | _addressResult2 << 8));
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;
                case MicroCode.IndirectY_1:
                    _masterClient.Read(Registers.PC++);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectY_2;
                    break;
                case MicroCode.IndirectY_2:
                    _masterClient.Read((byte)(_addressResult + 1));
                    _addressResult2 = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectY_3;
                    break;
                case MicroCode.IndirectY_3:
                    _masterClient.Read(_addressResult);
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.IndirectY_4;
                    break;
                case MicroCode.IndirectY_4:
                    _masterClient.Read((ushort)((_addressResult | _addressResult2 << 8) + Registers.Y));
                    _addressResult = _masterClient.Value;
                    _nextMicroCode = MicroCode.None;
                    break;

                case MicroCode.ADC:
                    {
                        var result = (ushort)(Registers.A + _addressResult + Status.C);
                        UpdateCVN(Registers.A, _addressResult, result);
                        Registers.A = (byte)result;
                        _nextMicroCode = MicroCode.None;
                    }

                    break;
                default:
                    throw new InvalidProgramException($"invalid micro code: 0x{code:X}.");
            }
        }

        private void UpdateCVN(byte a, byte b, ushort result)
        {
            Status.C = (byte)(result > 0xFF ? 1 : 0);
            Status.V = (byte)(~(a ^ b) & (a ^ b) & 0x80);
            Status.N = (byte)((result & 0x80) >> 7);
            Status.Z = (byte)(result == 0 ? 1 : 0);
        }

        private static readonly ushort[] _interruptVectors = new ushort[]
        {
            0xFFFA, 0xFFFC, 0xFFFE, 0xFFFE
        };

        private void Interrupt(InterruptType interrupt)
        {
            Status.I = 1;
            var vector = _interruptVectors[(byte)interrupt];
            _masterClient.Read(vector);
            Registers.PC = _masterClient.Value;
        }

        void IClockSink.OnTick()
        {
            ExecuteOpCode();
        }

        void IClockSink.OnPowerUp()
        {
            _nextOpCodeStatus = OpCodeStatus.None;
            _nextMicroCode = MicroCode.None;

            // see also: https://wiki.nesdev.com/w/index.php/CPU_power_up_state
            Status.Value = 0x34;

            Registers.A = 0;
            Registers.X = 0;
            Registers.Y = 0;
            Registers.S = 0xFD;

            _masterClient.Value = 0;
            _masterClient.Write(0x4017); // frame irq enabled
            _masterClient.Write(0x4015); // all channels disabled

            // $4000 -$400F = $00
            for (ushort i = 0x4000; i <= 0x400F; i++)
                _masterClient.Write(i);

            // TODO:
            // All 15 bits of noise channel LFSR = $0000[3].The first time the LFSR is clocked from the all-0s state, it will shift in a 1.
            Interrupt(InterruptType.Reset);
        }

        void IClockSink.OnReset()
        {
            // see also: https://wiki.nesdev.com/w/index.php/CPU_power_up_state
            Registers.S -= 3;
            Status.I = 1;

            // TODO:
            // APU mode in $4017 was unchanged
            // APU was silenced($4015 = 0)
            Interrupt(InterruptType.Reset);
        }
    }

    /// <summary>
    /// 中断类型
    /// </summary>
    public enum InterruptType : byte
    {
        NMI,
        Reset,
        IRQ,
        BRK
    }

    /// <summary>
    /// 6052 CPU 寄存器组
    /// </summary>
    public struct ProcessorRegisters
    {
        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort PC { get; set; }

        /// <summary>
        /// Stack Pointer
        /// </summary>
        public byte S { get; set; }

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// Index Register X
        /// </summary>
        public byte X { get; set; }

        /// <summary>
        /// Index Register Y
        /// </summary>
        public byte Y { get; set; }
    }

    /// <summary>
    /// 6502 CPU 状态
    /// </summary>
    public struct ProcessorStatus
    {
        private byte _value;

        /// <summary>
        /// 值
        /// </summary>
        public byte Value
        {
            get => _value;
            set => _value = (byte)(value & 0b01111111);
        }

        /// <summary>
        /// Carry Flag
        /// </summary>
        public byte C
        {
            get => (byte)(_value & 0b1);
            set => _value = (byte)((_value & ~0b1) | (value & 0b1));
        }

        /// <summary>
        /// Zero Flag
        /// </summary>
        public byte Z
        {
            get => (byte)((_value & 0b10) >> 1);
            set => _value = (byte)((_value & ~0b10) | ((value << 1) & 0b10));
        }

        /// <summary>
        /// Interrupt Disable
        /// </summary>
        public byte I
        {
            get => (byte)((_value & 0b100) >> 2);
            set => _value = (byte)((_value & ~0b100) | ((value << 2) & 0b100));
        }

        /// <summary>
        /// Decimal Mode
        /// </summary>
        public byte D
        {
            get => (byte)((_value & 0b1000) >> 3);
            set => _value = (byte)((_value & ~0b1000) | ((value << 3) & 0b1000));
        }

        /// <summary>
        /// Break Command
        /// </summary>
        public byte B
        {
            get => (byte)((_value & 0b11_0000) >> 4);
            set => _value = (byte)((_value & ~0b11_0000) | ((value << 4) & 0b11_0000));
        }

        /// <summary>
        /// Overflow Flag
        /// </summary>
        public byte V
        {
            get => (byte)((_value & 0b100_0000) >> 6);
            set => _value = (byte)((_value & ~0b100_0000) | ((value << 6) & 0b100_0000));
        }

        /// <summary>
        /// Negative Flag
        /// </summary>
        public byte N
        {
            get => (byte)((_value & 0b1000_0000) >> 7);
            set => _value = (byte)((_value & ~0b1000_0000) | ((value << 7) & 0b1000_0000));
        }
    }
}
