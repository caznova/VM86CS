﻿using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x88, Count = 5)]
        [CPUFunction(OpCode = 0x8e)]
        [CPUFunction(OpCode = 0xa0, Count = 4)]
        [CPUFunction(OpCode = 0xb0, Count = 16)]
        [CPUFunction(OpCode = 0x00c6)]
        [CPUFunction(OpCode = 0x00c7)]
        public void Move(Operand dest, Operand source)
        {
            dest.Value = source.Value;
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x06)]
        [CPUFunction(OpCode = 0x0e)]
        [CPUFunction(OpCode = 0x16)]
        [CPUFunction(OpCode = 0x1e)]
        [CPUFunction(OpCode = 0x68)]
        [CPUFunction(OpCode = 0x6a)]
        [CPUFunction(OpCode = 0x50, Count=8)]
        [CPUFunction(OpCode = 0xff06)]
        public void Push(Operand dest)
        {
            StackPush(dest.Value);
        }

        [CPUFunction(OpCode = 0x07)]
        [CPUFunction(OpCode = 0x0f)]
        [CPUFunction(OpCode = 0x17)]
        [CPUFunction(OpCode = 0x1f)]
        [CPUFunction(OpCode = 0x58, Count = 8)]
        [CPUFunction(OpCode = 0x8f)]
        public void Pop(Operand dest)
        {
            dest.Value = StackPop();
            WriteOperand(dest);
        }
    }
}