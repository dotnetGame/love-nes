using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public partial class CPU
    {
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

            INC_1_ZeroPage,

            INX_1_Implied,

            BEQ_1_Relative
        }

        private (MicroCode nextMicroCode, OpCodeStatus nextOpCodeStatus) ExecuteOpCode(OpCodeStatus code)
        {
            switch (code)
            {
                case OpCodeStatus.Relative_Jump:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Relative, OpCodeStatus.None);

                case OpCodeStatus.SEI_1_Implied:
                    return (MicroCode.SEI, OpCodeStatus.None);
                case OpCodeStatus.CLD_1_Implied:
                    return (MicroCode.CLD, OpCodeStatus.None);

                case OpCodeStatus.STY_1_Absolute:
                    _addressState.Set(AddressOperand.Y, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.STA_1_ZeroPage:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_ZeroPageX:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPageX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_AbsoluteX:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.STX_1_ZeroPage:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STX_1_Absolute:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.LDY_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.Y, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.LDX_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.X, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.TAX_1_Implied:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.X, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.TXA_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.TXS_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.S, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.INC_1_ZeroPage:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, true);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);

                case OpCodeStatus.INX_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.X, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.LDA_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_Absolute:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_AbsoluteX:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_ZeroPage:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);

                case OpCodeStatus.JSR_1_Absolute:
                    _addressState.ResultA = (byte)((Registers.PC + 1) >> 8);
                    return (MicroCode.Push, OpCodeStatus.JSR_2_Absolute);
                case OpCodeStatus.JSR_2_Absolute:
                    _addressState.ResultA = (byte)((Registers.PC + 1) & 0xFF);
                    return (MicroCode.Push, OpCodeStatus.JSR_3_Absolute);
                case OpCodeStatus.JSR_3_Absolute:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.BIT_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.BitTest, true);
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

                case OpCodeStatus.BEQ_1_Relative:
                    if (Status.Z)
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }
                    else
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }

                case OpCodeStatus.CMP_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.CPX_1_Immediate:
                    _addressState.Set(AddressOperand.X, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.AND_1_Immediate:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.RTS_1_Implied:
                    return (MicroCode.Pop, OpCodeStatus.RTS_2_Implied);
                case OpCodeStatus.RTS_2_Implied:
                    _addressState.MemoryAddress = _addressState.ResultA;
                    return (MicroCode.Pop, OpCodeStatus.RTS_3_Implied);
                case OpCodeStatus.RTS_3_Implied:
                    _addressState.MemoryAddress |= (ushort)(_addressState.ResultA << 8);
                    return (MicroCode.Nop, OpCodeStatus.RTS_4_Implied);
                case OpCodeStatus.RTS_4_Implied:
                    _addressState.MemoryAddress++;
                    return (MicroCode.Nop, OpCodeStatus.RTS_5_Implied);
                case OpCodeStatus.RTS_5_Implied:
                    _addressState.Set(AddressOperand.None, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                default:
                    throw new InvalidProgramException($"invalid op code status: 0x{code:X}.");
            }
        }
    }
}
