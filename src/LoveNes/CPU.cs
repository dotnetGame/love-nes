using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace LoveNes
{
    /// <summary>
    /// 6502 CPU
    /// </summary>
    public partial class CPU : IClockSink, IInterruptReceiver
    {
        /// <summary>
        /// 寄存器组
        /// </summary>
        public ProcessorRegisters Registers;

        /// <summary>
        /// 状态
        /// </summary>
        public ProcessorStatus Status;

        private IBusMasterClient _masterClient;
        private MicroCode _nextMicroCode;
        private OpCodeStatus _nextOpCodeStatus;
        private bool _readingOpCode = false;

        private AddressState _addressState;
        private InterruptType? _interruptType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPU"/> class.
        /// </summary>
        /// <param name="busMaster">总线 Master 客户端</param>
        public CPU(IBusMasterClient masterClient)
        {
            _masterClient = masterClient;
        }

        private enum AddressOperand
        {
            None,
            Memory,
            X,
            Y,
            A,
            S,
            PC
        }

        private enum AddressOperation
        {
            None,
            Inc,
            Dec,
            BitTest,
            Compare,
            And,
            Or,
            Adc
        }

        private struct AddressState
        {
            public AddressOperand SourceA;

            public AddressOperand SourceB;

            public AddressOperand Destination;

            public AddressOperation Operation;

            public bool AffectFlags;

            public ushort MemoryAddress;
            public byte MemoryAddress8;

            public byte ResultA;
            public byte ResultB;

            public void Set(AddressOperand sourceA, AddressOperand sourceB, AddressOperand destination, AddressOperation operation, bool affectFlags)
            {
                SourceA = sourceA;
                SourceB = sourceB;
                Destination = destination;
                Operation = operation;
                AffectFlags = affectFlags;
            }
        }

        private void DispatchAddressing()
        {
            void ReadSource(AddressOperand addressOperand, ref byte result)
            {
                switch (addressOperand)
                {
                    // In Result
                    case AddressOperand.None:
                        break;
                    case AddressOperand.Memory:
                        if (_addressState.Destination != AddressOperand.PC)
                        {
                            _masterClient.Read(_addressState.MemoryAddress);
                            result = _masterClient.Value;
                        }

                        break;
                    case AddressOperand.X:
                        result = Registers.X;
                        break;
                    case AddressOperand.Y:
                        result = Registers.Y;
                        break;
                    case AddressOperand.A:
                        result = Registers.A;
                        break;
                    default:
                        throw new ArgumentException(nameof(_addressState.SourceA));
                }
            }

            // Read Source
            ReadSource(_addressState.SourceA, ref _addressState.ResultA);
            ReadSource(_addressState.SourceB, ref _addressState.ResultB);

            // Do Operation
            switch (_addressState.Operation)
            {
                case AddressOperation.None:
                    if (_addressState.AffectFlags)
                        UpdateNZ(_addressState.ResultA);
                    break;
                case AddressOperation.Inc:
                    _addressState.ResultA++;
                    if (_addressState.AffectFlags)
                        UpdateNZ(_addressState.ResultA);
                    break;
                case AddressOperation.Dec:
                    _addressState.ResultA--;
                    if (_addressState.AffectFlags)
                        UpdateNZ(_addressState.ResultA);
                    break;
                case AddressOperation.BitTest:
                    if (_addressState.AffectFlags)
                    {
                        Status.Z = (_addressState.ResultA & _addressState.ResultB) == 0;
                        Status.N = ((_addressState.ResultB & 0x80) >> 7) != 0;
                        Status.V = ((_addressState.ResultB & 0x40) >> 6) != 0;
                    }

                    break;
                case AddressOperation.Compare:
                    if (_addressState.AffectFlags)
                    {
                        Status.C = _addressState.ResultA >= _addressState.ResultB;
                        UpdateNZ((byte)(_addressState.ResultA - _addressState.ResultB));
                    }

                    break;
                case AddressOperation.And:
                    _addressState.ResultA &= _addressState.ResultB;
                    if (_addressState.AffectFlags)
                        UpdateNZ(_addressState.ResultA);
                    break;
                case AddressOperation.Or:
                    _addressState.ResultA |= _addressState.ResultB;
                    if (_addressState.AffectFlags)
                        UpdateNZ(_addressState.ResultA);
                    break;
                case AddressOperation.Adc:
                    {
                        var value = (ushort)(_addressState.ResultA + _addressState.ResultB + (Status.C ? 1 : 0));
                        if (_addressState.AffectFlags)
                            UpdateCV(_addressState.ResultA, _addressState.ResultB, value);
                        _addressState.ResultA = (byte)value;
                        if (_addressState.AffectFlags)
                            UpdateNZ(_addressState.ResultA);
                    }

                    break;
                default:
                    throw new ArgumentException(nameof(_addressState.Operation));
            }

            // Write Destination
            switch (_addressState.Destination)
            {
                case AddressOperand.None:
                    break;
                case AddressOperand.Memory:
                    _masterClient.Value = _addressState.ResultA;
                    _masterClient.Write(_addressState.MemoryAddress);
                    break;
                case AddressOperand.X:
                    Registers.X = _addressState.ResultA;
                    break;
                case AddressOperand.Y:
                    Registers.Y = _addressState.ResultA;
                    break;
                case AddressOperand.A:
                    Registers.A = _addressState.ResultA;
                    break;
                case AddressOperand.S:
                    Registers.S = _addressState.ResultA;
                    break;
                case AddressOperand.PC:
                    Registers.PC = _addressState.MemoryAddress;
                    break;
                default:
                    throw new ArgumentException(nameof(_addressState.Destination));
            }
        }

        private void UpdateCV(byte a, byte b, ushort result)
        {
            Status.C = result > 0xFF;
            Status.V = (~(a ^ b) & (a ^ b) & 0x80) != 0;
        }

        private void UpdateNZ(byte result)
        {
            Status.N = ((result & 0x80) >> 7) != 0;
            Status.Z = result == 0;
        }

        private static readonly ushort[] _interruptVectors = new ushort[]
        {
            0xFFFA, 0xFFFC, 0xFFFE, 0xFFFE
        };

        public void Interrupt(InterruptType interrupt)
        {
            _interruptType = interrupt;
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

            // _masterClient.Value = 0;
            // _masterClient.Write(0x4017); // frame irq enabled
            // _masterClient.Write(0x4015); // all channels disabled

            // $4000 -$400F = $00
            // for (ushort i = 0x4000; i <= 0x400F; i++)
            //    _masterClient.Write(i);

            // TODO:
            // All 15 bits of noise channel LFSR = $0000[3].The first time the LFSR is clocked from the all-0s state, it will shift in a 1.
            Interrupt(InterruptType.Reset);
        }

        void IClockSink.OnReset()
        {
            // see also: https://wiki.nesdev.com/w/index.php/CPU_power_up_state
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
        private BitVector8 _value;

        /// <summary>
        /// 值
        /// </summary>
        public byte Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Carry Flag
        /// </summary>
        public bool C
        {
            get => _value[0b1];
            set => _value[0b1] = value;
        }

        /// <summary>
        /// Zero Flag
        /// </summary>
        public bool Z
        {
            get => _value[0b10];
            set => _value[0b10] = value;
        }

        /// <summary>
        /// Interrupt Disable
        /// </summary>
        public bool I
        {
            get => _value[0b100];
            set => _value[0b100] = value;
        }

        /// <summary>
        /// Decimal Mode
        /// </summary>
        public bool D
        {
            get => _value[0b1000];
            set => _value[0b1000] = value;
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
        public bool V
        {
            get => _value[0b100_0000];
            set => _value[0b100_0000] = value;
        }

        /// <summary>
        /// Negative Flag
        /// </summary>
        public bool N
        {
            get => _value[0b1000_0000];
            set => _value[0b1000_0000] = value;
        }
    }

    public interface IInterruptReceiver
    {
        void Interrupt(InterruptType interruptType);
    }
}
