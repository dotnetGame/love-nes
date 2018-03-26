using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public partial class CPU
    {
        private enum MicroCode : byte
        {
            None,

            Nop,

            Addressing,

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
            Pop,

            ADC,

            SEI,
            CLC,
            CLD
        }

        private MicroCode ExecuteMicroCode(MicroCode code)
        {
            switch (code)
            {
                case MicroCode.Nop:
                    return MicroCode.None;

                case MicroCode.Addressing:
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.Immediate:
                    _addressState.MemoryAddress = Registers.PC++;
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.Relative:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = (ushort)(Registers.PC + (sbyte)_masterClient.Value);
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.Absolute_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.Absolute_2;
                case MicroCode.Absolute_2:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress |= (ushort)(_masterClient.Value << 8);
                    return MicroCode.Absolute_3;
                case MicroCode.Absolute_3:
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.ZeroPage_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.ZeroPage_2;
                case MicroCode.ZeroPage_2:
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.ZeroPageX_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.ZeroPageX_2;
                case MicroCode.ZeroPageX_2:
                    _addressState.MemoryAddress = (byte)(_addressState.MemoryAddress + Registers.X);
                    return MicroCode.ZeroPageX_3;
                case MicroCode.ZeroPageX_3:
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.AbsoluteX_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.AbsoluteX_2;
                case MicroCode.AbsoluteX_2:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress |= (ushort)(_masterClient.Value << 8);
                    return MicroCode.AbsoluteX_3;
                case MicroCode.AbsoluteX_3:
                    _addressState.MemoryAddress += Registers.X;
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.AbsoluteY_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.AbsoluteX_2;
                case MicroCode.AbsoluteY_2:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress |= (ushort)(_masterClient.Value << 8);
                    return MicroCode.AbsoluteX_3;
                case MicroCode.AbsoluteY_3:
                    _addressState.MemoryAddress += Registers.Y;
                    DispatchAddressing();
                    return MicroCode.None;

                case MicroCode.IndirectY_1:
                    _masterClient.Read(Registers.PC++);
                    _addressState.MemoryAddress8 = _masterClient.Value;
                    return MicroCode.IndirectY_2;
                case MicroCode.IndirectY_2:
                    _masterClient.Read(_addressState.MemoryAddress8);
                    _addressState.MemoryAddress = _masterClient.Value;
                    return MicroCode.IndirectY_3;
                case MicroCode.IndirectY_3:
                    _masterClient.Read((byte)(_addressState.MemoryAddress8 + 1));
                    _addressState.MemoryAddress |= (ushort)(_masterClient.Value << 8);
                    return MicroCode.IndirectY_4;
                case MicroCode.IndirectY_4:
                    _addressState.MemoryAddress += Registers.Y;
                    DispatchAddressing();
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
                    _masterClient.Value = _addressState.ResultA;
                    _masterClient.Write((ushort)(0x100u + Registers.S--));
                    return MicroCode.None;
                case MicroCode.Pop:
                    _masterClient.Read((ushort)(0x100u + ++Registers.S));
                    _addressState.ResultA = _masterClient.Value;
                    return MicroCode.None;
                case MicroCode.SEI:
                    Status.I = true;
                    return MicroCode.None;
                case MicroCode.CLC:
                    Status.C = false;
                    return MicroCode.None;
                case MicroCode.CLD:
                    Status.D = false;
                    return MicroCode.None;
                default:
                    throw new InvalidProgramException($"invalid micro code: 0x{code:X}.");
            }
        }
    }
}
