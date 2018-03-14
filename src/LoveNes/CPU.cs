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
        private bool _readingOpCode = false;

        private AddressDestination _addressDst;
        private AddressDirection _addressDir;
        private AddressOperation _addressOper;

        private byte _tempValue;

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
                    if (!_readingOpCode)
                    {
                        _readingOpCode = true;
                        _masterClient.Read(Registers.PC++);
                        return;
                    }
                    else
                    {
                        _readingOpCode = false;
                        _nextOpCodeStatus = ExecuteOpCode((OpCode)_masterClient.Value);
                    }
                }

                (_nextMicroCode, _nextOpCodeStatus) = ExecuteOpCode(_nextOpCodeStatus);
            }

            _nextMicroCode = ExecuteMicroCode(_nextMicroCode);
        }

        private OpCodeStatus ExecuteOpCode(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.SEI_Implied:
                    return OpCodeStatus.SEI_1_Implied;
                case OpCode.CLD_Implied:
                    return OpCodeStatus.CLD_1_Implied;
                case OpCode.LDX_Immediate:
                    return OpCodeStatus.LDX_1_Immediate;
                case OpCode.STX_Absolute:
                    return OpCodeStatus.STX_1_Absolute;
                case OpCode.TXS_Implied:
                    return OpCodeStatus.TXS_1_Implied;
                case OpCode.INX_Implied:
                    return OpCodeStatus.INX_1_Implied;
                case OpCode.LDA_Absolute:
                    return OpCodeStatus.LDA_1_Absolute;
                case OpCode.JSR_Absolute:
                    return OpCodeStatus.JSR_1_Absolute;
                case OpCode.BIT_Absolute:
                    return OpCodeStatus.BIT_1_Absolute;
                case OpCode.BPL_Relative:
                    return OpCodeStatus.BPL_1_Relative;
                default:
                    throw new InvalidProgramException($"invalid op code: 0x{opCode:X}.");
            }
        }

        public enum OpCode : byte
        {
            /// <summary>
            /// Branch if Positive
            /// </summary>
            BPL_Relative = 0x10,

            /// <summary>
            /// Jump to Subroutine
            /// </summary>
            JSR_Absolute = 0x20,

            /// <summary>
            /// Bit Test
            /// </summary>
            BIT_Absolute = 0x2C,

            ADC_Immediate = 0x69,

            /// <summary>
            /// Set Interrupt Disable
            /// </summary>
            SEI_Implied = 0x78,

            /// <summary>
            /// Store X Register - Absolute
            /// </summary>
            STX_Absolute = 0x8E,

            /// <summary>
            /// Transfer X to Stack Pointer
            /// </summary>
            TXS_Implied = 0x9A,

            /// <summary>
            /// Load X Register - Immediate
            /// </summary>
            LDX_Immediate = 0xA2,

            /// <summary>
            /// Load Accumulator
            /// </summary>
            LDA_Absolute = 0xAD,

            /// <summary>
            /// Clear Decimal Mode
            /// </summary>
            CLD_Implied = 0xD8,

            /// <summary>
            /// Increment X Register
            /// </summary>
            INX_Implied = 0xE8
        }

        public enum OpCodeStatus : byte
        {
            None,

            BPL_1_Relative,
            BPL_2_Relative,

            JSR_1_Absolute,
            JSR_2_Absolute,
            JSR_3_Absolute,

            BIT_1_Absolute,

            ADC_1_Addressing_Immediate,
            ADC_1_Addressing_ZeroPage,
            ADC_1_Addressing_ZeroPageX,
            ADC_1_Addressing_Absolute,
            ADC_1_Addressing_AbsoluteX,
            ADC_1_Addressing_AbsoluteY,
            ADC_1_Addressing_IndirectX,
            ADC_1_Addressing_IndirectY,
            ADC_2,

            SEI_1_Implied,

            STX_1_Absolute,

            TXS_1_Implied,

            LDX_1_Immediate,

            LDA_1_Absolute,

            CLD_1_Implied,

            INX_1_Implied
        }

        private (MicroCode nextMicroCode, OpCodeStatus nextOpCodeStatus) ExecuteOpCode(OpCodeStatus code)
        {
            switch (code)
            {
                /*
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
                    break;*/
                case OpCodeStatus.SEI_1_Implied:
                    return (MicroCode.SEI, OpCodeStatus.None);
                case OpCodeStatus.STX_1_Absolute:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.CLD_1_Implied:
                    return (MicroCode.CLD, OpCodeStatus.None);
                case OpCodeStatus.LDX_1_Immediate:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.TXS_1_Implied:
                    return (MicroCode.TXS, OpCodeStatus.None);
                case OpCodeStatus.INX_1_Implied:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.Inc;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Register, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_Absolute:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.JSR_1_Absolute:
                    _tempValue = (byte)((Registers.PC + 1) >> 8);
                    return (MicroCode.Push, OpCodeStatus.JSR_2_Absolute);
                case OpCodeStatus.JSR_2_Absolute:
                    _tempValue = (byte)((Registers.PC + 1) & 0xFF);
                    return (MicroCode.Push, OpCodeStatus.JSR_3_Absolute);
                case OpCodeStatus.JSR_3_Absolute:
                    _addressDst = AddressDestination.PC;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Jump;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.BIT_1_Absolute:
                    _addressDst = AddressDestination.None;
                    _addressOper = AddressOperation.BitTest;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.BPL_1_Relative:
                    return (MicroCode.Nop, Status.N == 0 ? OpCodeStatus.BPL_2_Relative : OpCodeStatus.None);
                case OpCodeStatus.BPL_2_Relative:
                    _addressDst = AddressDestination.PC;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Jump;
                    return (MicroCode.Relative, OpCodeStatus.None);
                default:
                    throw new InvalidProgramException($"invalid op code status: 0x{code:X}.");
            }
        }

        private enum MicroCode : byte
        {
            None,

            Nop,

            Register,

            Immediate,

            Relative,

            ZeroPage_1,
            ZeroPage_2,

            ZeroPageX_1,
            ZeroPageX_2,
            ZeroPageX_3,

            ZeroPageY_1,
            ZeroPageY_2,
            ZeroPageY_3,

            Absolute_1,
            Absolute_2,
            Absolute_3,

            AbsoluteX_1,
            AbsoluteX_2,
            AbsoluteX_3,

            AbsoluteY_1,
            AbsoluteY_2,
            AbsoluteY_3,

            IndirectX_1,
            IndirectX_2,
            IndirectX_3,
            IndirectX_4,
            IndirectX_5,

            IndirectY_1,
            IndirectY_2,
            IndirectY_3,
            IndirectY_4,

            Push,

            ADC,

            SEI,

            TXS,

            CLD
        }

        private MicroCode ExecuteMicroCode(MicroCode code)
        {
            switch (code)
            {
                case MicroCode.Nop:
                    return MicroCode.None;

                case MicroCode.Register:
                    DispatchRegisterAddressing();
                    return MicroCode.None;

                case MicroCode.Immediate:
                    DispatchMemoryAddressing(Registers.PC++);
                    return MicroCode.None;

                case MicroCode.Relative:
                    _masterClient.Read(Registers.PC++);
                    DispatchMemoryAddressing((ushort)(Registers.PC + (sbyte)_masterClient.Value));
                    return MicroCode.None;

                case MicroCode.Absolute_1:
                    _masterClient.Read(Registers.PC++);
                    _tempValue = _masterClient.Value;
                    return MicroCode.Absolute_2;
                case MicroCode.Absolute_2:
                    _masterClient.Read(Registers.PC++);
                    return MicroCode.Absolute_3;
                case MicroCode.Absolute_3:
                    DispatchMemoryAddressing((ushort)((_masterClient.Value << 8) | (_tempValue & 0xFF)));
                    return MicroCode.None;
                /*
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
                    */
                case MicroCode.Push:
                    _masterClient.Value = _tempValue;
                    _masterClient.Write((ushort)(0x100u + Registers.S--));
                    return MicroCode.None;
                case MicroCode.SEI:
                    Status.I = 1;
                    return MicroCode.None;
                case MicroCode.TXS:
                    Registers.S = Registers.X;
                    return MicroCode.None;
                case MicroCode.CLD:
                    Status.D = 0;
                    return MicroCode.None;
                default:
                    throw new InvalidProgramException($"invalid micro code: 0x{code:X}.");
            }
        }

        private enum AddressDestination
        {
            None,
            X,
            A,
            PC
        }

        private enum AddressDirection
        {
            Read,
            Write,
            Jump
        }

        private enum AddressOperation
        {
            None,
            Inc,
            BitTest
        }

        private void DispatchRegisterAddressing()
        {
            switch (_addressDst)
            {
                case AddressDestination.X:
                    Registers.X = DoOperation(Registers.X, true);
                    break;
                case AddressDestination.A:
                    Registers.A = DoOperation(Registers.A, true);
                    break;
                default:
                    throw new ArgumentException(nameof(_addressDst));
            }
        }

        private void DispatchMemoryAddressing(ushort address)
        {
            switch (_addressDir)
            {
                case AddressDirection.Read:
                    _masterClient.Read(address);
                    switch (_addressDst)
                    {
                        case AddressDestination.None:
                            DoOperation(_masterClient.Value, true);
                            break;
                        case AddressDestination.X:
                            Registers.X = DoOperation(_masterClient.Value, true);
                            break;
                        case AddressDestination.A:
                            Registers.A = DoOperation(_masterClient.Value, true);
                            break;
                        default:
                            throw new ArgumentException(nameof(_addressDst));
                    }

                    break;
                case AddressDirection.Write:
                    switch (_addressDst)
                    {
                        case AddressDestination.X:
                            _masterClient.Value = Registers.X;
                            break;
                        default:
                            throw new ArgumentException(nameof(_addressDst));
                    }

                    _masterClient.Write(address);
                    break;
                case AddressDirection.Jump:
                    switch (_addressDst)
                    {
                        case AddressDestination.PC:
                            Registers.PC = address;
                            break;
                        default:
                            throw new ArgumentException(nameof(_addressDst));
                    }

                    break;
                default:
                    throw new ArgumentException(nameof(_addressDir));
            }
        }

        private byte DoOperation(byte value, bool affectFlag)
        {
            switch (_addressOper)
            {
                case AddressOperation.None:
                    if (affectFlag) UpdateNZ(value);
                    return value;
                case AddressOperation.Inc:
                    value += 1;
                    if (affectFlag) UpdateNZ(value);
                    return value;
                case AddressOperation.BitTest:
                    if (affectFlag)
                    {
                        Status.Z = (byte)((Registers.A & value) == 0 ? 1 : 0);
                        Status.N = (byte)((value & 0x80) >> 7);
                        Status.V = (byte)((value & 0x40) >> 6);
                    }

                    return value;
                default:
                    throw new ArgumentException(nameof(_addressOper));
            }
        }

        private void UpdateCV(byte a, byte b, ushort result)
        {
            Status.C = (byte)(result > 0xFF ? 1 : 0);
            Status.V = (byte)(~(a ^ b) & (a ^ b) & 0x80);
        }

        private void UpdateNZ(byte result)
        {
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
            Registers.PC = ReadUShort(vector);
        }

        private ushort ReadUShort(ushort address)
        {
            _masterClient.Read(address);
            ushort value = _masterClient.Value;
            _masterClient.Read(++address);
            value = (ushort)(value | (_masterClient.Value << 8));
            return value;
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
        public ushort PC;

        /// <summary>
        /// Stack Pointer
        /// </summary>
        public byte S;

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A;

        /// <summary>
        /// Index Register X
        /// </summary>
        public byte X;

        /// <summary>
        /// Index Register Y
        /// </summary>
        public byte Y;
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
