using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public partial class CPU
    {
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
            /// Increment Memory - Zero Page
            /// </summary>
            INC_ZeroPage = 0xE6,

            /// <summary>
            /// Increment X Register
            /// </summary>
            INX_Implied = 0xE8,

            /// <summary>
            /// Branch if Equal
            /// </summary>
            BEQ_Relative = 0xF0
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
                case OpCode.BEQ_Relative:
                    return OpCodeStatus.BEQ_1_Relative;
                case OpCode.INC_ZeroPage:
                    return OpCodeStatus.INC_1_ZeroPage;
                default:
                    throw new InvalidProgramException($"invalid op code: 0x{opCode:X}.");
            }
        }

        private void ExecuteOpCode()
        {
            if (!_masterClient.TryAcquire()) return;

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
    }
}
