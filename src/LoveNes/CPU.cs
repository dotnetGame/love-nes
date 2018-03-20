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
        private ushort _tempValue16;

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
                        var opCode = (OpCode)_masterClient.Value;

                        Console.WriteLine(opCode);
                        _nextOpCodeStatus = ExecuteOpCode(opCode);
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
                case OpCode.LDA_Immediate:
                    return OpCodeStatus.LDA_1_Immediate;
                case OpCode.LDA_Absolute:
                    return OpCodeStatus.LDA_1_Absolute;
                case OpCode.JSR_Absolute:
                    return OpCodeStatus.JSR_1_Absolute;
                case OpCode.BIT_Absolute:
                    return OpCodeStatus.BIT_1_Absolute;
                case OpCode.BPL_Relative:
                    return OpCodeStatus.BPL_1_Relative;
                case OpCode.TXA_Implied:
                    return OpCodeStatus.TXA_1_Implied;
                case OpCode.STA_ZeroPageX:
                    return OpCodeStatus.STA_1_ZeroPageX;
                case OpCode.STA_AbsoluteX:
                    return OpCodeStatus.STA_1_AbsoluteX;
                case OpCode.BNE_Relative:
                    return OpCodeStatus.BNE_1_Relative;
                case OpCode.LDY_Immediate:
                    return OpCodeStatus.LDY_1_Immediate;
                case OpCode.CMP_Absolute:
                    return OpCodeStatus.CMP_1_Absolute;
                case OpCode.STA_Absolute:
                    return OpCodeStatus.STA_1_Absolute;
                case OpCode.STY_Absolute:
                    return OpCodeStatus.STY_1_Absolute;
                case OpCode.STA_ZeroPage:
                    return OpCodeStatus.STA_1_ZeroPage;
                case OpCode.LDA_ZeroPage:
                    return OpCodeStatus.LDA_1_ZeroPage;
                case OpCode.AND_Immediate:
                    return OpCodeStatus.AND_1_Immediate;
                case OpCode.TAX_Implied:
                    return OpCodeStatus.TAX_1_Implied;
                case OpCode.STX_ZeroPage:
                    return OpCodeStatus.STX_1_ZeroPage;
                case OpCode.RTS_Implied:
                    return OpCodeStatus.RTS_1_Implied;
                case OpCode.LDA_AbsoluteX:
                    return OpCodeStatus.LDA_1_AbsoluteX;
                case OpCode.CPX_Immediate:
                    return OpCodeStatus.CPX_1_Immediate;
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
            /// Logical AND - Immediate
            /// </summary>
            AND_Immediate = 0x29,

            /// <summary>
            /// Bit Test
            /// </summary>
            BIT_Absolute = 0x2C,

            /// <summary>
            /// Return from Subroutine
            /// </summary>
            RTS_Implied = 0x60,

            ADC_Immediate = 0x69,

            /// <summary>
            /// Set Interrupt Disable
            /// </summary>
            SEI_Implied = 0x78,

            /// <summary>
            /// Store A Register - Zero Page
            /// </summary>
            STA_ZeroPage = 0x85,

            /// <summary>
            /// Store X Register - Zero Page
            /// </summary>
            STX_ZeroPage = 0x86,

            /// <summary>
            /// Transfer X to Accumulator
            /// </summary>
            TXA_Implied = 0x8A,

            /// <summary>
            /// Store Y Register - Absolute
            /// </summary>
            STY_Absolute = 0x8C,

            /// <summary>
            /// Store A Register - Absolute
            /// </summary>
            STA_Absolute = 0x8D,

            /// <summary>
            /// Store X Register - Absolute
            /// </summary>
            STX_Absolute = 0x8E,

            /// <summary>
            /// Store Accumulator - Zero Page X
            /// </summary>
            STA_ZeroPageX = 0x95,

            /// <summary>
            /// Transfer X to Stack Pointer
            /// </summary>
            TXS_Implied = 0x9A,

            /// <summary>
            /// Store Accumulator - Absolute X
            /// </summary>
            STA_AbsoluteX = 0x9D,

            /// <summary>
            /// Load Y Register - Immediate
            /// </summary>
            LDY_Immediate = 0xA0,

            /// <summary>
            /// Load X Register - Immediate
            /// </summary>
            LDX_Immediate = 0xA2,

            /// <summary>
            /// Load Accumulator - Zero Page
            /// </summary>
            LDA_ZeroPage = 0xA5,

            /// <summary>
            /// Load Accumulator - Immediate
            /// </summary>
            LDA_Immediate = 0xA9,

            /// <summary>
            /// Transfer Accumulator to X
            /// </summary>
            TAX_Implied = 0xAA,

            /// <summary>
            /// Load Accumulator - Absolute
            /// </summary>
            LDA_Absolute = 0xAD,

            /// <summary>
            /// Load Accumulator - Absolute X
            /// </summary>
            LDA_AbsoluteX = 0xBD,

            /// <summary>
            /// Compare - Absolute
            /// </summary>
            CMP_Absolute = 0xCD,

            /// <summary>
            /// Branch if Not Equal
            /// </summary>
            BNE_Relative = 0xD0,

            /// <summary>
            /// Clear Decimal Mode
            /// </summary>
            CLD_Implied = 0xD8,

            /// <summary>
            /// Compare X Register - Immediate
            /// </summary>
            CPX_Immediate = 0xE0,

            /// <summary>
            /// Increment X Register
            /// </summary>
            INX_Implied = 0xE8
        }

        public enum OpCodeStatus : byte
        {
            None,

            Relative_Jump,

            BNE_1_Relative,

            BPL_1_Relative,

            JSR_1_Absolute,
            JSR_2_Absolute,
            JSR_3_Absolute,

            BIT_1_Absolute,

            RTS_1_Implied,
            RTS_2_Implied,
            RTS_3_Implied,
            RTS_4_Implied,
            RTS_5_Implied,

            ADC_1_Addressing_Immediate,
            ADC_1_Addressing_ZeroPage,
            ADC_1_Addressing_ZeroPageX,
            ADC_1_Addressing_Absolute,
            ADC_1_Addressing_AbsoluteX,
            ADC_1_Addressing_AbsoluteY,
            ADC_1_Addressing_IndirectX,
            ADC_1_Addressing_IndirectY,
            ADC_2,

            AND_1_Immediate,

            SEI_1_Implied,

            TXA_1_Implied,

            STY_1_Absolute,

            STA_1_ZeroPage,
            STA_1_ZeroPageX,
            STA_1_Absolute,
            STA_1_AbsoluteX,

            STX_1_ZeroPage,
            STX_1_Absolute,

            TXS_1_Implied,

            LDX_1_Immediate,

            LDY_1_Immediate,

            TAX_1_Implied,

            LDA_1_Immediate,
            LDA_1_Absolute,
            LDA_1_AbsoluteX,
            LDA_1_ZeroPage,

            CMP_1_Absolute,

            CLD_1_Implied,

            CPX_1_Immediate,

            INX_1_Implied
        }

        private (MicroCode nextMicroCode, OpCodeStatus nextOpCodeStatus) ExecuteOpCode(OpCodeStatus code)
        {
            switch (code)
            {
                case OpCodeStatus.Relative_Jump:
                    _addressDst = AddressDestination.PC;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Jump;
                    return (MicroCode.Relative, OpCodeStatus.None);

                case OpCodeStatus.SEI_1_Implied:
                    return (MicroCode.SEI, OpCodeStatus.None);
                case OpCodeStatus.STY_1_Absolute:
                    _addressDst = AddressDestination.Y;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_ZeroPage:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_ZeroPageX:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.ZeroPageX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_AbsoluteX:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_Absolute:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.STX_1_ZeroPage:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STX_1_Absolute:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Write;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.CLD_1_Implied:
                    return (MicroCode.CLD, OpCodeStatus.None);
                case OpCodeStatus.LDY_1_Immediate:
                    _addressDst = AddressDestination.Y;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.LDX_1_Immediate:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.TAX_1_Implied:
                    return (MicroCode.TAX, OpCodeStatus.None);
                case OpCodeStatus.TXA_1_Implied:
                    return (MicroCode.TXA, OpCodeStatus.None);
                case OpCodeStatus.TXS_1_Implied:
                    return (MicroCode.TXS, OpCodeStatus.None);
                case OpCodeStatus.INX_1_Implied:
                    _addressDst = AddressDestination.X;
                    _addressOper = AddressOperation.Inc;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Register, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_Immediate:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_Absolute:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_AbsoluteX:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_ZeroPage:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.None;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
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
                    if (Status.N)
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }
                    else
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }

                case OpCodeStatus.BNE_1_Relative:
                    if (Status.Z)
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }
                    else
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }

                case OpCodeStatus.CMP_1_Absolute:
                    _addressDst = AddressDestination.None;
                    _addressOper = AddressOperation.Compare;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.CPX_1_Immediate:
                    _addressDst = AddressDestination.None;
                    _addressOper = AddressOperation.CompareX;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.AND_1_Immediate:
                    _addressDst = AddressDestination.A;
                    _addressOper = AddressOperation.And;
                    _addressDir = AddressDirection.Read;
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.RTS_1_Implied:
                    return (MicroCode.Pop, OpCodeStatus.RTS_2_Implied);
                case OpCodeStatus.RTS_2_Implied:
                    _tempValue16 = _tempValue;
                    return (MicroCode.Pop, OpCodeStatus.RTS_3_Implied);
                case OpCodeStatus.RTS_3_Implied:
                    _tempValue16 |= (ushort)(_tempValue << 8);
                    return (MicroCode.Nop, OpCodeStatus.RTS_4_Implied);
                case OpCodeStatus.RTS_4_Implied:
                    _tempValue16++;
                    return (MicroCode.Nop, OpCodeStatus.RTS_5_Implied);
                case OpCodeStatus.RTS_5_Implied:
                    Registers.PC = _tempValue16;
                    return (MicroCode.Nop, OpCodeStatus.None);
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
            AbsoluteX_4,

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
            Pop,

            ADC,

            SEI,

            TAX,
            TXA,
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

                case MicroCode.ZeroPage_1:
                    _masterClient.Read(Registers.PC++);
                    _tempValue = _masterClient.Value;
                    return MicroCode.ZeroPage_2;
                case MicroCode.ZeroPage_2:
                    DispatchMemoryAddressing(_tempValue);
                    return MicroCode.None;

                case MicroCode.ZeroPageX_1:
                    _masterClient.Read(Registers.PC++);
                    _tempValue = _masterClient.Value;
                    return MicroCode.ZeroPageX_2;
                case MicroCode.ZeroPageX_2:
                    _tempValue += Registers.X;
                    return MicroCode.ZeroPageX_3;
                case MicroCode.ZeroPageX_3:
                    DispatchMemoryAddressing(_tempValue);
                    return MicroCode.None;

                case MicroCode.AbsoluteX_1:
                    _masterClient.Read(Registers.PC++);
                    _tempValue = _masterClient.Value;
                    return MicroCode.AbsoluteX_2;
                case MicroCode.AbsoluteX_2:
                    _masterClient.Read(Registers.PC++);
                    _tempValue16 = (ushort)((_masterClient.Value << 8) | (_tempValue & 0xFF));
                    return MicroCode.AbsoluteX_3;
                case MicroCode.AbsoluteX_3:
                    _tempValue16 += Registers.X;
                    return MicroCode.AbsoluteX_4;
                case MicroCode.AbsoluteX_4:
                    DispatchMemoryAddressing(_tempValue16);
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
                case MicroCode.Pop:
                    _masterClient.Read((ushort)(0x100u + ++Registers.S));
                    _tempValue = _masterClient.Value;
                    return MicroCode.None;
                case MicroCode.SEI:
                    Status.I = true;
                    return MicroCode.None;
                case MicroCode.TAX:
                    Registers.X = Registers.A;
                    UpdateNZ(Registers.X);
                    return MicroCode.None;
                case MicroCode.TXA:
                    Registers.A = Registers.X;
                    UpdateNZ(Registers.A);
                    return MicroCode.None;
                case MicroCode.TXS:
                    Registers.S = Registers.X;
                    return MicroCode.None;
                case MicroCode.CLD:
                    Status.D = false;
                    return MicroCode.None;
                default:
                    throw new InvalidProgramException($"invalid micro code: 0x{code:X}.");
            }
        }

        private enum AddressDestination
        {
            None,
            X,
            Y,
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
            BitTest,
            Compare,
            CompareX,
            And
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
                        case AddressDestination.Y:
                            Registers.Y = DoOperation(_masterClient.Value, true);
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
                        case AddressDestination.Y:
                            _masterClient.Value = Registers.Y;
                            break;
                        case AddressDestination.A:
                            _masterClient.Value = Registers.A;
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
                        Status.Z = (Registers.A & value) == 0;
                        Status.N = ((value & 0x80) >> 7) != 0;
                        Status.V = ((value & 0x40) >> 6) != 0;
                    }

                    return value;
                case AddressOperation.Compare:
                    if (affectFlag)
                    {
                        Status.C = Registers.A >= value;
                        Status.Z = Registers.A == value;
                        Status.N = Registers.A - value < 0;
                    }

                    return value;
                case AddressOperation.CompareX:
                    if (affectFlag)
                    {
                        Status.C = Registers.X >= value;
                        Status.Z = Registers.X == value;
                        Status.N = Registers.X - value < 0;
                    }

                    return value;
                case AddressOperation.And:
                    value &= Registers.A;
                    if (affectFlag)
                    {
                        Status.Z = Registers.A == 0;
                        Status.N = ((value & 0x80) >> 7) != 0;
                    }

                    return value;
                default:
                    throw new ArgumentException(nameof(_addressOper));
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

        private void Interrupt(InterruptType interrupt)
        {
            Status.I = true;
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
            Status.I = true;

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
}
