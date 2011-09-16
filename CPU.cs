﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace x86CS
{
    public enum SegmentRegister
    {
        ES = 0,
        CS,
        SS,
        DS,
        FS,
        GS
    }

    public enum CPURegister
    {
        EAX,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI,
        EIP,
    }

    [Flags]
    public enum CPUFlags : uint
    {
        CF = 0x0001,
        Spare = 0x0002,
        PF = 0x0004,
        Spare2 = 0x0008,
        AF = 0x0010,
        Spare3 = 0x0020,
        ZF = 0x0040,
        SF = 0x0080,
        TF = 0x0100,
        IF = 0x0200,
        DF = 0x0400,
        OF = 0x0800,
        IOPL = 0x1000,
        NT = 0x4000,
        Spare4 = 0x9000,
        RF = 0x10000,
        VM = 0x20000,
        AC = 0x40000,
        VIF = 0x00080000,
        VIP = 0x00100000,
        ID = 0x00200000,
        Spare5 = 0x00400000,
        Spare6 = 0x00800000,
        Spare7 = 0x01000000,
        Spare8 = 0x02000000,
        Spare9 = 0x04000000,
        Spare10 = 0x08000000,
        Spare11 = 0x10000000,
        Spare12 = 0x20000000,
        Spare13 = 0x40000000,
        Spare14 = 0x80000000,
    }

    public struct Segment
    {
        private ushort value;
        private int virtualAddr;

        public int Addr
        {
            get { return value; }
            set
            {
                this.value = (ushort)value;
                virtualAddr = value << 4;
            }
        }

        public int VirtualAddr
        {
            get { return virtualAddr; }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register
    {
        [FieldOffset(0)]
        public uint DWord;
        [FieldOffset(0)]
        public ushort Word;
        [FieldOffset(0)]
        public byte Byte;
    }

    public class CPU
    {
        private Segment[] segments;
        private Register[] registers;
        private CPUFlags eFlags;
        private SegmentRegister dataSegment;
        private bool repPrefix = false;
        private bool debug = false;
        public event EventHandler<TextEventArgs> DebugText;
        public event EventHandler<IntEventArgs> InteruptFired;
        private StreamWriter logFile = File.CreateText("cpulog.txt");

        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        #region Registers

        public uint EAX
        {
            get { return registers[(int)CPURegister.EAX].DWord; }
            set { registers[(int)CPURegister.EAX].DWord = value; }
        }

        public ushort AX
        {
            get { return registers[(int)CPURegister.EAX].Word; }
            set { registers[(int)CPURegister.EAX].Word = value; }
        }

        public byte AL
        {
            get { return registers[(int)CPURegister.EAX].Byte; }
            set { registers[(int)CPURegister.EAX].Byte = value; }
        }

        public byte AH
        {
            get { return (byte)((registers[(int)CPURegister.EAX].Word.GetHigh())); }
            set { registers[(int)CPURegister.EAX].Word = registers[(int)CPURegister.EAX].Word.SetHigh(value); }
        }

        public uint EBX
        {
            get { return registers[(int)CPURegister.EBX].DWord; }
            set { registers[(int)CPURegister.EBX].DWord = value; }
        }

        public ushort BX
        {
            get { return registers[(int)CPURegister.EBX].Word; }
            set { registers[(int)CPURegister.EBX].Word = value; }
        }

        public byte BL
        {
            get { return registers[(int)CPURegister.EBX].Byte; }
            set { registers[(int)CPURegister.EBX].Byte = value; }
        }

        public byte BH
        {
            get { return (byte)((registers[(int)CPURegister.EBX].Word.GetHigh())); }
            set { registers[(int)CPURegister.EBX].Word = registers[(int)CPURegister.EBX].Word.SetHigh(value); }
        }

        public uint ECX
        {
            get { return registers[(int)CPURegister.ECX].DWord; }
            set { registers[(int)CPURegister.ECX].DWord = value; }
        }

        public ushort CX
        {
            get { return registers[(int)CPURegister.ECX].Word; }
            set { registers[(int)CPURegister.ECX].Word = value; }
        }

        public byte CL
        {
            get { return registers[(int)CPURegister.ECX].Byte; }
            set { registers[(int)CPURegister.ECX].Byte = value; }
        }

        public byte CH
        {
            get { return (byte)((registers[(int)CPURegister.ECX].Word.GetHigh())); }
            set { registers[(int)CPURegister.ECX].Word = registers[(int)CPURegister.ECX].Word.SetHigh(value); }
        }

        public uint EDX
        {
            get { return registers[(int)CPURegister.EDX].DWord; }
            set { registers[(int)CPURegister.EDX].DWord = value; }
        }

        public ushort DX
        {
            get { return registers[(int)CPURegister.EDX].Word; }
            set { registers[(int)CPURegister.EDX].Word = value; }
        }

        public byte DL
        {
            get { return registers[(int)CPURegister.EDX].Byte; }
            set { registers[(int)CPURegister.EDX].Byte = value; }
        }

        public byte DH
        {
            get { return (byte)((registers[(int)CPURegister.EDX].Word.GetHigh())); }
            set { registers[(int)CPURegister.EDX].Word = registers[(int)CPURegister.EDX].Word.SetHigh(value); }
        }

        public uint ESI
        {
            get { return registers[(int)CPURegister.ESI].DWord; }
            set { registers[(int)CPURegister.ESI].DWord = value; }
        }

        public ushort SI
        {
            get { return registers[(int)CPURegister.ESI].Word; }
            set { registers[(int)CPURegister.ESI].Word = value; }
        }

        public uint EDI
        {
            get { return registers[(int)CPURegister.EDI].DWord; }
            set { registers[(int)CPURegister.EDI].DWord = value; }
        }

        public ushort DI
        {
            get { return registers[(int)CPURegister.EDI].Word; }
            set { registers[(int)CPURegister.EDI].Word = value; }
        }

        public uint EBP
        {
            get { return registers[(int)CPURegister.EBP].DWord; }
            set { registers[(int)CPURegister.EBP].DWord = value; }
        }

        public ushort BP
        {
            get { return registers[(int)CPURegister.EBP].Word; }
            set { registers[(int)CPURegister.EBP].Word = value; }
        }

        public ushort IP
        {
            get { return registers[(int)CPURegister.EIP].Word; }
            set { registers[(int)CPURegister.EIP].Word = value; }
        }

        public uint ESP
        {
            get { return registers[(int)CPURegister.ESP].DWord; }
            set { registers[(int)CPURegister.ESP].DWord = value; }
        }

        public ushort SP
        {
            get { return registers[(int)CPURegister.ESP].Word; }
            set { registers[(int)CPURegister.ESP].Word = value; }
        }

        #endregion

        #region Segments
        public ushort CS
        {
            get { return (ushort)segments[(int)SegmentRegister.CS].Addr; }
            set { segments[(int)SegmentRegister.CS].Addr = value; }
        }

        public ushort DS
        {
            get { return (ushort)segments[(int)SegmentRegister.DS].Addr; }
            set { segments[(int)SegmentRegister.DS].Addr = value; }
        }
        public ushort ES
        {
            get { return (ushort)segments[(int)SegmentRegister.ES].Addr; }
            set { segments[(int)SegmentRegister.ES].Addr = value; }
        }
        public ushort SS
        {
            get { return (ushort)segments[(int)SegmentRegister.SS].Addr; }
            set { segments[(int)SegmentRegister.SS].Addr = value; }
        }
        public ushort FS
        {
            get { return (ushort)segments[(int)SegmentRegister.FS].Addr; }
            set { segments[(int)SegmentRegister.FS].Addr = value; }
        }
        public ushort GS
        {
            get { return (ushort)segments[(int)SegmentRegister.GS].Addr; }
            set { segments[(int)SegmentRegister.GS].Addr = value; }
        }
        #endregion

        #region Flags
        public bool CF
        {
            get { return GetFlag(CPUFlags.CF); }
            set { SetFlag(CPUFlags.CF, value); }
        }

        public bool PF
        {
            get { return GetFlag(CPUFlags.PF); }
            set { SetFlag(CPUFlags.PF, value); }
        }

        public bool AF
        {
            get { return GetFlag(CPUFlags.AF); }
            set { SetFlag(CPUFlags.AF, value); }
        }

        public bool ZF
        {
            get { return GetFlag(CPUFlags.ZF); }
            set { SetFlag(CPUFlags.ZF, value); }
        }

        public bool SF
        {
            get { return GetFlag(CPUFlags.SF); }
            set { SetFlag(CPUFlags.SF, value); }
        }

        public bool TF
        {
            get { return GetFlag(CPUFlags.TF); }
            set { SetFlag(CPUFlags.TF, value); }
        }

        public bool IF
        {
            get { return GetFlag(CPUFlags.IF); }
            set { SetFlag(CPUFlags.OF, value); }
        }

        public bool DF
        {
            get { return GetFlag(CPUFlags.DF); }
            set { SetFlag(CPUFlags.DF, value); }
        }

        public bool OF
        {
            get { return GetFlag(CPUFlags.OF); }
            set { SetFlag(CPUFlags.OF, value); }
        }

        public byte IOPL
        {
            get { return (byte)(((int)eFlags & 0x3000) >> 12); }
            set { eFlags = (CPUFlags)(value & 0x3000); }
        }

        public bool NT
        {
            get { return GetFlag(CPUFlags.NT); }
            set { SetFlag(CPUFlags.NT, value); }
        }

        public bool RF
        {
            get { return GetFlag(CPUFlags.RF); }
            set { SetFlag(CPUFlags.RF, value); }
        }

        public bool VM
        {
            get { return GetFlag(CPUFlags.VM); }
            set { SetFlag(CPUFlags.VM, value); }
        }

        public bool AC
        {
            get { return GetFlag(CPUFlags.AC); }
            set { SetFlag(CPUFlags.AC, value); }
        }

        public bool VIF
        {
            get { return GetFlag(CPUFlags.VIF); }
            set { SetFlag(CPUFlags.VIF, value); }
        }

        public bool VIP
        {
            get { return GetFlag(CPUFlags.VIP); }
            set { SetFlag(CPUFlags.VIP, value); }
        }

        public bool ID
        {
            get { return GetFlag(CPUFlags.ID); }
            set { SetFlag(CPUFlags.ID, value); }
        }

        #endregion

        public CPU()
        {
            segments = new Segment[6];
            registers = new Register[9];

            dataSegment = SegmentRegister.DS;
            eFlags = CPUFlags.ZF | CPUFlags.IF;
        }

        private void FireInterrupt(int intNum)
        {
            EventHandler<IntEventArgs> intEvent = InteruptFired;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(intNum));
        }

        private void DebugWrite(string text)
        {
            logFile.Write(text);
            logFile.Flush();

            if (!debug)
                return;
                    
            EventHandler<TextEventArgs> textEvent = DebugText;

            if (textEvent != null)
                textEvent(this, new TextEventArgs(text));
        }

        private void DebugWriteLine(string text)
        {
            DebugWrite(text + '\n');
        }

        private bool GetFlag(CPUFlags flag)
        {
            return (eFlags & flag) == flag;
        }

        private void SetFlag(CPUFlags flag, bool value)
        {
            if (value)
                eFlags |= flag;
            else
                eFlags &= ~flag;
        }

        private byte SegReadByte(SegmentRegister segment, int offset)
        {
            int virtAddr;
            int seg = segments[(int)segment].VirtualAddr;
            byte ret;

            virtAddr = seg + offset;

            ret = Memory.ReadByte(virtAddr);

            if(segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Byte {0:X8} {1:X2}", virtAddr, ret)); 

            return ret;
        }

        private byte ReadByte()
        {
            byte ret;

            ret = SegReadByte(SegmentRegister.CS, IP);

            IP++;

            return ret;
        }

        private byte DataReadByte(int offset)
        {
            return SegReadByte(dataSegment, offset);
        }

        private ushort SegReadWord(SegmentRegister segment, int offset)
        {
            int virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;
            ushort ret;

            virtAddr = segPtr + offset;

            ret = Memory.ReadWord(virtAddr);

            if(segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Word {0:X8} {1:X4}", virtAddr, ret)); 

            return ret;
        }

        private ushort DataReadWord(int offset)
        {
            return SegReadWord(dataSegment, offset);
        }

        private ushort ReadWord()
        {
            ushort ret;

            ret = SegReadWord(SegmentRegister.CS, IP);
            IP += 2;

            return ret;
        }

        private void SegWriteByte(SegmentRegister segment, int offset, byte value)
        {
            int virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;

            virtAddr = segPtr + offset;

            logFile.WriteLine(String.Format("Memory Write Byte {0:X8} {1:X2}", virtAddr, value)); 

            Memory.WriteByte(virtAddr, value);
        }

        private void SegWriteWord(SegmentRegister segment, int offset, ushort value)
        {
            int virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;

            virtAddr = segPtr + offset;

            logFile.WriteLine(String.Format("Memory Write word {0:X8} {1:X4}", virtAddr, value)); 

            Memory.WriteWord(virtAddr, value);
        }

        private void DataWriteByte(int offset, byte value)
        {
            SegWriteByte(dataSegment, offset, value);
        }

        private void DataWriteWord(int offset, ushort value)
        {
            SegWriteWord(dataSegment, offset, value);
        }

        private ushort StackPop()
        {
            ushort ret;

            ret = SegReadWord(SegmentRegister.SS, SP);
            SP += 2;

            return ret;
        }

        private void StackPush(ushort value)
        {
            SP -= 2;

            SegWriteWord(SegmentRegister.SS, SP, value);
        }

        private byte GetOpCode()
        {
            byte op = ReadByte();
            bool getNextOp = true;

            // Check if this is a prefix and do the prefix work and read the next opcode
            switch (op)
            {
                case 0x26:
                    dataSegment = SegmentRegister.ES;
                    break;
                case 0x2e:
                    dataSegment = SegmentRegister.CS;
                    break;
                case 0x36:
                    dataSegment = SegmentRegister.SS;
                    break;
                case 0xf3:
                    repPrefix = true;
                    break;
                default:
                    getNextOp = false;
                    break;
            }

            if(getNextOp)
                op = GetOpCode();

            return op;
        }

        private string GetRegStr(int offset)
        {
            switch (offset)
            {
                case 0x0:
                    return "AX";
                case 0x1:
                    return "CX";
                case 0x2:
                    return "DX";
                case 0x3:
                    return "BX";
                case 0x4:
                    return "SP";
                case 0x5:
                    return "BP";
                case 0x6:
                    return "SI";
                case 0x7:
                    return "DI";
                default:
                    return "";
            }
        }

        private bool ReadRM(out byte code, out ushort opAddr, out string opStr)
        {
            byte mode, rm, modRegRm;
            bool isReg = false;

            modRegRm = ReadByte();

            mode = (byte)(modRegRm >> 6);
            code = (byte)((modRegRm >> 3) & 0x7);
            rm = (byte)(modRegRm & 0x07);

            switch (mode)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    int addr = 0;

                    switch (rm)
                    {
                        case 0x0:
                            addr = BX + SI;
                            opStr = "[BX + SI";
                            break;
                        case 0x1:
                            addr = BX + DI;
                            opStr = "[BX + DI";
                            break;
                        case 0x2:
                            dataSegment = SegmentRegister.SS;
                            addr = BP + SI;
                            opStr = "[BP + SI";
                            break;
                        case 0x3:
                            dataSegment = SegmentRegister.SS;
                            addr = BP + DI;
                            opStr = "[BP + DI";
                            break;
                        case 0x4:
                            addr = SI;
                            opStr = "[SI";
                            break;
                        case 0x5:
                            addr = DI;
                            opStr = "[DI";
                            break;
                        case 0x6:
                            if (mode == 0x1 || mode == 0x2)
                            {
                                addr = BP;
                                opStr = "[BP + ";
                            }
                            else
                                opStr = "[";
                            break;
                        case 0x7:
                            addr = BX;
                            opStr = "[BX";
                            break;
                        default:
                            addr = 0xffff;
                            opStr = "INVALID";
                            break;
                    }
                    if (mode == 0x1)
                    {
                        byte byteOp;

                        byteOp = ReadByte();
                        addr += (sbyte)byteOp;

                        if ((sbyte)byteOp < 0)
                            opStr += " - " + ((sbyte)-byteOp).ToString() + "]";
                        else
                            opStr += " + " + byteOp.ToString() + "]";
                    }
                    else if (mode == 0x2)
                    {
                        ushort wordOp;

                        wordOp = ReadWord();
                        addr += wordOp;

                        opStr += " + " + wordOp.ToString() + "]";
                    }
                    else if (rm == 0x6)
                    {
                        addr = ReadWord();
                        opStr += addr.ToString("X4") + "]";
                    }
                    else
                        opStr += "]";

                    opAddr = (ushort)addr;

                    break;
                case 0x03:
                    opAddr = rm;
                    isReg = true;
                    opStr = GetRegStr(rm);
                    break;
                default:
                    opAddr = 0;
                    opStr = "INVALID";
                    break;
            }

            return isReg;
        }

        private void SetParity(int value)
        {
            BitArray bitCount = new BitArray(new int[] { value });

            if (bitCount.CountSet() % 2 == 0)
                PF = true;
            else
                PF = false;
        }

        private byte Sub(byte src, byte dst)
        {
            return DoSub(src, dst, false);
        }

        private ushort Sub(ushort src, ushort dst)
        {
            return DoSub(src, dst, false);
        }

        private byte Sbb(byte src, byte dst)
        {
            return DoSub(src, dst, true);
        }

        private ushort Sbb(ushort src, ushort dst)
        {
            return DoSub(src, dst, true);
        }

        private byte DoSub(byte src, byte dst, bool borrow)
        {
            sbyte result;
            
            if(borrow && CF)
                result = (sbyte)(dst - (src + 1));
            else
                result = (sbyte)(dst - src);

            if ((byte)result > byte.MaxValue)
                CF = true;
            else
                CF = false;
      
            if (result == 0)
            {
                ZF = true;
                SF = false;
                OF = false;
            }
            else if (result > 0)
            {
                ZF = false;
                SF = false;
            }
            else
            {
                ZF = false;
                SF = true;
            }

            SetParity(result);

            return (byte)result;
        }

        private ushort DoSub(ushort src, ushort dst, bool borrow)
        {
            short result;

            if (borrow && CF)
                result = (short)(dst - (src + 1));
            else
                result = (short)(dst - src);

            if (dst < src)
                CF = true;
            else
                CF = false;

            if(((short)dst > 0 && (short)src < 0 && result > 0) || (short)dst < 0 && (short)src > 0 && result < 0)
                OF = true;
            else
                OF = false;

            if (result == 0)
                ZF = true;
            else
                ZF = false;

            if (result < 0)
                SF = true;
            else
                SF = false;

            SetParity(result);

            return (ushort)result;
        }

        private byte Add(byte src, byte dst)
        {
            return DoAdd(src, dst, false);
        }

        private ushort Add(ushort src, ushort dst)
        {
            return DoAdd(src, dst, false);
        }

        private byte Adc(byte src, byte dst)
        {
            return DoAdd(src, dst, true);
        }

        private ushort Adc(ushort src, ushort dst)
        {
            return DoAdd(src, dst, true);
        }

        private byte DoAdd(byte src, byte dst, bool carry)
        {
            short ret;

            ret = (short)(src + dst);
            if (carry)
                ret += CF ? (short)1 : (short)0;

            if (ret == 0)
                ZF = true;
            else
                ZF = false;

            if (ret > sbyte.MaxValue)
                OF = true;
            else
                OF = false;

            if (ret > byte.MaxValue)
                CF = true;
            else
                CF = false;

            if ((sbyte)ret < 0)
                SF = true;
            else
                SF = false;

            SetParity(ret);

            return (byte)ret;
        }

        private ushort DoAdd(ushort src, ushort dst, bool carry)
        {
            int ret;

            ret = (short)(src + dst);
            if (carry)
                ret += CF ? 1 : 0;

            if (ret == 0)
                ZF = true;
            else
                ZF = false;

            if (ret > short.MaxValue)
                OF = true;
            else
                OF = false;

            if (ret > ushort.MaxValue)
                CF = true;
            else
                CF = false;

            if ((short)ret < 0)
                SF = true;
            else
                SF = false;

            SetParity(ret);

            return (ushort)ret;
        }

        private void Mul(ushort src, int dst)
        {
            uint mulResult = (uint)dst * src;

            AX = (ushort)(mulResult & 0x0000FFFF);
            DX = (ushort)(mulResult >> 16);

            if (DX == 0)
            {
                OF = false;
                CF = false;
            }
            else
            {
                OF = true;
                CF = true;
            }
        }

        private void Mul(byte src, ushort dst)
        {
            ushort mulResult = (ushort)(dst * src);

            AL = (byte)(mulResult & 0x00FF);
            AH = (byte)(mulResult >> 8);

            if (AH == 0)
            {
                OF = false;
                CF = false;
            }
            else
            {
                OF = true;
                CF = true;
            }
        }

        private void Div(ushort src, int dst)
        {
            uint dividend = (uint)(((DX << 8) & 0xFFFF0000) + AX);

            if (dividend / src > ushort.MaxValue)
                throw new Exception("Division Error");

            AX = (ushort)(dividend / src);
            DX = (ushort)(dividend % src);
        }

        private byte GetByteReg(byte offset)
        {
            string tmp;

            return GetByteReg(offset, out tmp);
        }

        private byte GetByteReg(byte offset, out string regStr)
        {
            byte byteOp = 0;
            regStr = "INVALID";

            switch (offset)
            {
                case 0x00:
                    byteOp = AL;
                    regStr = "AL";
                    break;
                case 0x01:
                    byteOp = CL;
                    regStr = "CL";
                    break;
                case 0x2:
                    byteOp = DL;
                    regStr = "DL";
                    break;
                case 0x3:
                    byteOp = BL;
                    regStr = "BL";
                    break;
                case 0x4:
                    byteOp = AH;
                    regStr = "AH";
                    break;
                case 0x5:
                    byteOp = CH;
                    regStr = "CH";
                    break;
                case 0x6:
                    byteOp = DH;
                    regStr = "DH";
                    break;
                case 0x7:
                    byteOp = BH;
                    regStr = "BH";
                    break;
            }

            return byteOp;
        }

        private void SetByteReg(byte offset, byte byteOp)
        {
            string tmp;

            SetByteReg(offset, byteOp, out tmp);
        }

        private void SetByteReg(byte offset, byte byteOp, out string regStr)
        {
            regStr = "INVALID";

            switch (offset)
            {
                case 0x00:
                    AL = byteOp;
                    regStr = "AL";
                    break;
                case 0x01:
                    CL = byteOp;
                    regStr = "CL";
                    break;
                case 0x2:
                    DL = byteOp;
                    regStr = "DL";
                    break;
                case 0x3:
                    BL = byteOp;
                    regStr = "BL";
                    break;
                case 0x4:
                    AH = byteOp;
                    regStr = "AH";
                    break;
                case 0x5:
                    CH = byteOp;
                    regStr = "CH";
                    break;
                case 0x6:
                    DH = byteOp;
                    regStr = "DH";
                    break;
                case 0x7:
                    BH = byteOp;
                    regStr = "BH";
                    break;
            }
        }

        public void Cycle()
        {
            ushort wordOp = 0, wordOp2 = 0;
            byte byteOp, byteOp2 = 0;
            byte op, reg, segment, opCode;
            ushort addr;
            bool isReg;
            string opStr, regStr = "", regStr2 = "", grpStr = "";
            string debugStr = "";

            debugStr = String.Format("{0:X4}:{1:X4}    ", CS, IP);

            op = GetOpCode();

            #region OpCodes
            switch (op)
            {
                case 0x00:          /* ADD reg/mem8, reg8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    byteOp = GetByteReg(reg, out regStr);

                    if (isReg)
                        byteOp2 = GetByteReg((byte)addr);
                    else
                        byteOp2 = DataReadByte(addr);

                    byteOp2 = Add(byteOp, byteOp2);
                    DataWriteByte(addr, byteOp2);

                    debugStr += String.Format("ADD {0:X2}, {1}", opStr, regStr);
                    break;
                case 0x01:          /* ADD reg/mem16, reg */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    wordOp = Add(registers[reg].Word, wordOp);
                    DataWriteWord(addr, wordOp);

                    debugStr += (String.Format("ADD {0}, {1}", opStr, GetRegStr(reg)));
                    break;
                case 0x02:          /* ADD reg8, reg/mem8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    byteOp = GetByteReg(reg, out regStr);

                    if (isReg)
                        byteOp2 = GetByteReg((byte)addr);
                    else
                        byteOp2 = DataReadByte(addr);

                    SetByteReg(reg, Add(byteOp2, byteOp));

                    debugStr += (String.Format("ADD {0:X2}, {1}", opStr, regStr));
                    break;
                case 0x03:          /* ADD reg, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    registers[reg].Word = Add(wordOp, registers[reg].Word);

                    debugStr += String.Format("ADD {0}, {1}", GetRegStr(reg), opStr, wordOp);
                    break;
                case 0x04:          /* ADD AL, imm8 */
                    byteOp = ReadByte();

                    AL = Add(byteOp, AL);

                    debugStr += String.Format("ADD AL, {0:X2}", byteOp);
                    break;
                case 0x05:          /* ADD AX, imm16 */
                    wordOp = ReadWord();

                    AX = Add(wordOp, AX);

                    debugStr += String.Format("ADD AX, {0:X4}", wordOp);
                    break;
                case 0x06:          /* PUSH ES */
                    StackPush(ES);
                    debugStr += ("PUSH ES");
                    break;
                case 0x07:          /* POP ES */
                    ES = StackPop();
                    debugStr += ("POP ES");
                    break;
                case 0x0a:          /* OR reg8, reg8/mem8 */
                    isReg = ReadRM(out reg, out addr, out opStr);
                    
                    byteOp = GetByteReg(reg, out regStr);

                    if (isReg)
                        byteOp2 = GetByteReg((byte)addr, out regStr2);
                    else
                        byteOp2 = DataReadByte(addr);

                    byteOp2 = (byte)(byteOp | byteOp2);

                    SetByteReg(reg, byteOp2);

                    OF = CF = false;

                    if (byteOp2 == 0)
                        ZF = true;
                    else
                        ZF = false;

                    if ((sbyte)byteOp2 < 0)
                        SF = true;
                    else
                        SF = false;

                    SetParity(byteOp2);

                    debugStr += (String.Format("OR {0}, {1}", regStr, isReg ? regStr2 : opStr));
                    break;
                case 0x0e:          /* PUSH CS */
                    StackPush(CS);
                    debugStr += "PUSH CS";
                    break;
                case 0x13:          /* ADC reg, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    registers[reg].Word = Adc(wordOp, registers[reg].Word);

                    debugStr += (String.Format("ADC {0}, {1}", GetRegStr(reg), opStr, wordOp));
                    break;
                case 0x16:          /* PUSH SS */
                    StackPush(SS);
                    debugStr += ("PUSH SS");
                    break;
                case 0x1e:          /* PUSH DS */
                    StackPush(DS);
                    debugStr += ("PUSH DS");
                    break;
                case 0x1f:          /* POP DS */
                    DS = StackPop();
                    debugStr += ("POP DS");
                    break;
                case 0x2b:          /* SUB reg, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    registers[reg].Word = Sub(wordOp, registers[reg].Word);

                    debugStr += String.Format("SUB {0}, {1}", GetRegStr(reg), opStr);
                    break;
                case 0x32:          /* XOR reg8, reg8/imm8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        byteOp = GetByteReg((byte)addr, out regStr);
                    else
                        byteOp = DataReadByte(addr);

                    byteOp2 = GetByteReg((byte)reg, out regStr2);
                    byteOp2 ^= byteOp;

                    SetByteReg(reg, byteOp2);

                    debugStr += (String.Format("XOR {0}, {1}", regStr2, isReg ? regStr : opStr));
                    break;
                case 0x33:          /* XOR reg, reg/imm16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    registers[reg].Word ^= wordOp;
       
                    debugStr += (String.Format("XOR {0}, {1}", GetRegStr(reg), opStr));
                    break;
                case 0x3b:          /* CMP reg, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if(isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    Sub(wordOp, registers[reg].Word);

                    debugStr += (String.Format("CMP {0}, {1}", GetRegStr(reg), opStr, wordOp));
                    break;
                case 0x3c:          /* CMP AL, imm8 */
                    byteOp = ReadByte();

                    Sub(byteOp, AL);
                    debugStr += (String.Format("CMP AL, {0:X2}", byteOp));
                    break;
                case 0x39:          /* CMP reg/mem16, reg */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    Sub(registers[reg].Word, wordOp);

                    debugStr += String.Format("CMP {0}, {1}", opStr, GetRegStr(reg));
                    break;
                case 0x40:          /* INC reg */
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x46:
                case 0x47:
                    registers[op - 0x40].Word++;

                    debugStr += (String.Format("INC {0}", GetRegStr(op - 0x40)));
                    break;
                case 0x48:          /* DEC reg */
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x4c:
                case 0x4d:
                case 0x4e:
                case 0x4f:
                    registers[op - 0x48].Word--;

                    debugStr += (String.Format("DEC {0}", GetRegStr(op - 0x48)));
                    break;
                case 0x50:          /* PUSH reg */
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x56:
                    StackPush(registers[op - 0x50].Word);

                    debugStr += (String.Format("PUSH {0}", GetRegStr(op - 0x50)));
                    break;
                case 0x58:          /* POP reg */
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                    registers[op - 0x58].Word = StackPop();

                    debugStr += (string.Format("POP {0}", GetRegStr(op - 0x58)));
                    break;
                case 0x72:          /* JB rel8 */
                    byteOp = ReadByte();

                    addr = (ushort)(IP + (byte)((sbyte)byteOp));

                    if(CF)
                        IP = addr;
                   
                    debugStr += (String.Format("JB {0:X4}", addr));
                    break;
                case 0x73:          /* JNB rel8 */
                    byteOp = ReadByte();

                    addr = (ushort)(IP + (byte)((sbyte)byteOp));

                    if (!CF)
                        IP = addr;

                    debugStr += String.Format("JNB {0:X4}", addr);
                    break;
                case 0x74:          /* JE rel8 */
                    byteOp = ReadByte();

                    addr = (ushort)(IP + (byte)((sbyte)byteOp));

                    if(ZF)
                        IP = addr;

                    debugStr += String.Format("JE {0:X4}", addr);
                    break;
                case 0x75:          /* JNZ rel8 */
                    byteOp = ReadByte();

                    addr = (ushort)(IP + (byte)((sbyte)byteOp));

                    if (!ZF)
                        IP = addr;

                    debugStr += String.Format("JNE {0:X4}", addr);
                    break;
                case 0x7c:          /* JL rel8 */
                    byteOp = ReadByte();
                    break;
                case 0x80:          /* GRP reg/imm8, imm8 */ 
                    isReg = ReadRM(out opCode, out addr, out opStr);
                    byteOp = ReadByte();

                    if (isReg)
                        byteOp2 = GetByteReg((byte)addr);
                    else
                        byteOp2 = DataReadByte(addr);

                    switch (opCode)
                    {
                        case 0x7:   /* CMP */
                            grpStr = "CMP";
                            Sub(byteOp, byteOp2);
                            break;

                        default:
                            break;
                    }
                        
                    debugStr += (String.Format("{0} {1}, {2:X2})", grpStr, opStr, byteOp, byteOp2));
                    break;
                case 0x81:          /* GRP reg/mem16, imm16 */
                    isReg = ReadRM(out opCode, out addr, out opStr);

                    wordOp = ReadWord();

                    if (isReg)
                        wordOp2 = registers[addr].Word;
                    else
                        wordOp2 = DataReadWord(addr);

                    switch (opCode)
                    {
                        case 0x2:
                            grpStr = "ADC";
                            if (isReg)
                                registers[addr].Word = Adc((ushort)wordOp2, wordOp);
                            else
                            {
                                wordOp = Adc((ushort)wordOp2, wordOp);
                                DataWriteWord(addr, wordOp);
                            }
                            break;
                        case 0x7:
                            grpStr = "CMP";
                            Sub(wordOp2, wordOp);
                            break;
                        default:
                            break;
                    }

                    debugStr += (String.Format("{0} {1}, {2:X4}", grpStr, opStr, wordOp));
                    break;
                case 0x83:          /* GRP reg/mem16, imm8 */
                    isReg = ReadRM(out opCode, out addr, out opStr);

                    byteOp = ReadByte();

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    switch (opCode)
                    {
                        case 0x2:
                            grpStr = "ADC";
                            if (isReg)
                                registers[addr].Word = Adc((ushort)byteOp, wordOp);
                            else
                            {
                                wordOp = Adc((ushort)byteOp, wordOp);
                                DataWriteWord(addr, wordOp);
                            }
                            break;
                        case 0x3:
                            grpStr = "SBB";

                            if (isReg)
                                registers[addr].Word = Sbb((ushort)(short)byteOp, registers[addr].Word);
                            else
                            {
                                wordOp = Sbb((ushort)(short)byteOp, wordOp);
                                DataWriteWord(addr, wordOp);
                            }

                            break;
                        case 0x7:
                            grpStr = "CMP";

                            if (isReg)
                                Sub((ushort)(short)byteOp, registers[addr].Word);
                            else
                                Sub((ushort)(short)byteOp, wordOp);

                            break;
                        default:
                            break;
                    }

                    debugStr += (String.Format("{0} {1}, {2:X2}", grpStr, isReg ? GetRegStr(addr) : opStr, byteOp));
                    break;
                case 0x86:          /* XCHG reg8, reg/mem8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    byteOp2 = GetByteReg((byte)reg, out regStr2);

                    if (isReg)
                    {
                        byteOp = GetByteReg((byte)addr, out regStr);
                        SetByteReg((byte)addr, byteOp2);
                    }
                    else
                    {
                        byteOp = DataReadByte(addr);
                        DataWriteByte(addr, byteOp2);
                    }

                    SetByteReg(reg, byteOp);

                    debugStr += (String.Format("XCHG {0}, {1}", regStr2, isReg ? regStr : opStr));
                    break;
                case 0x88:          /* MOV reg8/mem8, reg8 */
                    isReg = ReadRM(out reg, out addr, out opStr);
                    
                    byteOp = GetByteReg(reg, out regStr);

                    if (isReg)
                        SetByteReg((byte)addr, byteOp);
                    else
                        DataWriteByte(addr, byteOp);
                    
                    debugStr += (String.Format("MOV {0}, {1}", opStr, regStr));
                    break;
                case 0x89:          /* MOV reg/mem16, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);
                    wordOp = registers[reg].Word;

                    if (isReg)
                        registers[reg].Word = registers[addr].Word;
                    else
                        DataWriteWord(addr, wordOp);

                    debugStr += (String.Format("MOV {0}, {1}", opStr, GetRegStr(reg)));
                    break;
                case 0x8a:          /* MOV reg8, reg8/mem8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        byteOp = GetByteReg((byte)addr, out regStr);
                    else
                        byteOp = DataReadByte(addr);

                    SetByteReg(reg, byteOp, out regStr2);

                    debugStr += (String.Format("MOV {0}, {1}", regStr2, isReg ? regStr : opStr));
                    break;
                case 0x8b:          /* MOV reg, reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    registers[reg].Word = wordOp;

                    debugStr += String.Format("MOV {0}, {1}", GetRegStr(reg), opStr, wordOp);
                    break;
                case 0x8c:          /* MOV reg/mem16, seg */
                    isReg = ReadRM(out segment, out addr, out opStr);

                    if (isReg)
                        registers[addr].Word = (ushort)segments[segment].Addr;
                    else
                        DataWriteWord(addr, (ushort)segments[segment].Addr);

                    debugStr += String.Format("MOV {0}{1}, {2}", dataSegment == SegmentRegister.DS ? "" : dataSegment.ToString() + ":", opStr, Enum.GetName(typeof(SegmentRegister), segment));
                    break;
                case 0x8d:          /* LEA reg, eff16 */
                    ReadRM(out reg, out addr, out opStr);

                    registers[reg].Word = addr;

                    debugStr += String.Format("LEA {0}, {1}", GetRegStr(reg), opStr);
                    break;
                case 0x8e:          /* MOV seg, reg/mem16 */
                    isReg = ReadRM(out segment, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    segments[segment].Addr = wordOp;

                    debugStr += (String.Format("MOV {0}, {1}", Enum.GetName(typeof(SegmentRegister), segment), opStr));
                    break;
                case 0x8f:          /* POP reg/mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    if (isReg)
                    {
                    }
                    else
                    {
                        DataWriteWord(addr, StackPop());
                    }

                    debugStr += (string.Format("POP {0}", opStr, DataReadWord(addr)));
                    break;
                case 0xa0:          /* MOV AL, moffs8 */
                    wordOp = ReadWord();

                    AL = DataReadByte(wordOp);

                    debugStr += (String.Format("MOV AL, [{0:X4}]", wordOp));
                    break;
                case 0xa1:          /* MOV AX, moffs16 */
                    wordOp = ReadWord();

                    AX = DataReadWord(wordOp);

                    debugStr += (String.Format("MOV AX, [{0:X4}]", wordOp));
                    break;
                case 0xa3:          /* MOV moffs16, AX */
                    wordOp = ReadWord();

                    DataWriteWord(wordOp, AX);

                    debugStr += (String.Format("MOV [{0:X4}], AX", wordOp));
                    break;
                case 0xa4:          /* MOVSB */
                    int count;

                    if (repPrefix)
                        count = CX;
                    else
                        count = 1;

                    for (int i = 0; i < count; i++)
                    {
                        SegWriteByte(SegmentRegister.ES, DI, DataReadByte(SI));
                        if (DF)
                        {
                            SI--;
                            DI--;
                        }
                        else
                        {
                            SI++;
                            DI++;
                        }
                    }

                    debugStr += (String.Format("{0}MOVSB", repPrefix ? "REP " : ""));
                    break;
                case 0xa6:          /* CMPSB */
                    if (repPrefix)
                        count = DX;
                    else
                        count = 1;

                    while (count > 0)
                    {
                        byteOp = DataReadByte(SI);
                        byteOp2 = SegReadByte(SegmentRegister.ES, DI);

                        Sub(byteOp2, byteOp);

                        if (DF)
                        {
                            SI--;
                            DI--;
                        }
                        else
                        {
                            SI++;
                            DI++;
                        }

                        count--;
                    }

                    if (repPrefix)
                        DX = (ushort)count;

                    debugStr += ("CMPSB");
                    break;
                case 0xac:          /* LODSB */
                    AL = SegReadByte(SegmentRegister.DS, SI);

                    if (DF)
                        SI--;
                    else
                        SI++;
                    debugStr += (String.Format("LODSB"));
                    break;
                case 0xb0:          /* MOV reg8, imm8 */
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                    byteOp = ReadByte();
                    SetByteReg((byte)(op - 0xb0), byteOp, out regStr);

                    debugStr += (String.Format("MOV {0}, {1:X2}", regStr, byteOp));
                    break;
                case 0xb8:          /* MOV reg, imm16 */
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    wordOp = ReadWord();
                    registers[op - 0xb8].Word = wordOp;

                    debugStr += (String.Format("MOV {0}, {1:X4}", GetRegStr(op - 0xb8), wordOp));
                    break;
                case 0xc3:          /* RET */
                    IP = StackPop();
                    debugStr += (String.Format("RET"));
                    break;
                case 0xc5:          /* LDS reg/mem16, mem16 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    registers[reg].Word = DataReadWord(addr);
                    DS = DataReadWord(addr + 2);
                    debugStr += (String.Format("LDS {0}, {1}{2}", GetRegStr(reg), dataSegment == SegmentRegister.DS ? "" : Enum.GetName(typeof(SegmentRegister), dataSegment) + ":", opStr));
                    break;
                case 0xc6:          /* MOV reg/mem8, imm8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    byteOp = ReadByte();

                    if (isReg)
                        SetByteReg((byte)addr, byteOp);
                    else
                        DataWriteByte(addr, byteOp);

                    debugStr += String.Format("MOV {0}, {1:X2}", opStr, byteOp);
                    break;
                case 0xc7:          /* MOV reg/mem16, imm16 */
                    isReg = ReadRM(out reg, out addr, out opStr);
                    wordOp = ReadWord();

                    if (isReg)
                        registers[reg].Word = wordOp;
                    else
                        DataWriteWord(addr, wordOp);

                    debugStr += String.Format("MOV {0}, {1:X4}", opStr, wordOp);
                    break;
                case 0xcb:          /* RETF */
                    wordOp = ReadWord();

                    IP = StackPop();
                    CS = StackPop();

                    SP += wordOp;

                    debugStr += String.Format("RETF {0:X4}", wordOp);

                    break;
                case 0xcd:          /* INT imm8 */
                    byteOp = ReadByte();

                    FireInterrupt(byteOp);

                    debugStr += (String.Format("INT {0:X2}", byteOp));
                    break;
                case 0xd2:          /* GRP reg8/mem8, CL */
                    isReg = ReadRM(out opCode, out addr, out opStr);

                    if (isReg)
                        byteOp = GetByteReg((byte)addr, out regStr);
                    else
                        byteOp = DataReadByte(addr);

                    switch (opCode)
                    {
                        case 0x4:   /* SHL */
                            opStr = "SHL";
                            CL = (byte)(CL << byteOp);
                            break;
                        default:
                            break;
                    }

                    debugStr += (String.Format("{0} {1}, CL", opStr, isReg ? regStr : opStr));
                    break;
                case 0xd3:          /* GRP reg/mem16, CL */
                    isReg = ReadRM(out opCode, out addr, out opStr);

                    if (isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    switch (opCode)
                    {
                        case 0x4:   /* SHL */
                            grpStr = "SHL";
                            CL = (byte)(CL << wordOp);
                            break;
                        case 0x5:
                            grpStr = "SHR";
                            CL = (byte)(CL >> wordOp);
                            break;
                        default:
                            break;
                    }

                    debugStr += String.Format("{0} {1}, CL", grpStr, isReg ? GetRegStr(addr) : addr.ToString("X4"));
                    break;
                case 0xe8:          /* CALL rel16 */
                    wordOp = ReadWord();
                    StackPush(IP);
                    IP += (ushort)((short)wordOp);
                    debugStr += (String.Format("CALL {0:X4}", IP));
                    break;
                case 0xe9:          /* JMP rel16 */
                    wordOp = ReadWord();
                    IP += (ushort)((short)wordOp);
                    debugStr += (String.Format("JMP {0:X4}", IP));
                    break;
                case 0xea:          /* JMP FAR ptr16:16*/
                    addr = ReadWord();
                    wordOp = ReadWord();

                    CS = wordOp;
                    IP = addr;

                    debugStr += String.Format("JMP {0:X4}:{1:X4}", wordOp, addr);
                    break;
                case 0xeb:          /* JMP rel8 */
                    sbyte relOffs;
                    byteOp = ReadByte();

                    relOffs = (sbyte)byteOp;
                    if (relOffs < 0)
                        IP -= (ushort)-relOffs;
                    else
                        IP += byteOp;

                    debugStr += String.Format("JMP {0:X4}", IP);
                    break;
                case 0xe2:          /* LOOP CX rel8 */
                    byteOp = ReadByte();

                    addr = (ushort)(IP + ((sbyte)byteOp));

                    if (CX != 0)
                        IP = addr; 
                    
                    CX--;

                    debugStr += String.Format("LOOP {0:X4}", addr);
                    break;
                case 0xf7:          /* GRP DX:AX, reg/mem16 */
                    isReg = ReadRM(out opCode, out addr, out opStr);

                    if(isReg)
                        wordOp = registers[addr].Word;
                    else
                        wordOp = DataReadWord(addr);

                    switch (opCode)
                    {
                        case 0x4:   /* MUL */
                            Mul(wordOp, (int)AX);
                            grpStr = "MUL";
                            break;
                        case 0x6:   /* DIV */
                            Div(wordOp, (int)AX);
                            grpStr = "DIV";
                            break;
                        default:
                            break;
                    }

                    debugStr += (String.Format("{0} {1}", grpStr, opStr, wordOp));
                    break;
                case 0xf8:          /* CLC */
                    CF = false;

                    debugStr += ("CLC");
                    break;
                case 0xf9:          /* STC */
                    CF = true;

                    debugStr += ("STC");
                    break;  
                case 0xfa:          /* CLI */
                    IF = false;

                    debugStr += ("CLI");
                    break;
                case 0xfb:          /* STI */
                    IF = true;

                    debugStr += ("STI");
                    break;
                case 0xfc:          /* CLD */
                    DF = false;

                    debugStr += ("CLD");
                    break;
                case 0xfe:          /* INC reg8/mem8 */
                    isReg = ReadRM(out reg, out addr, out opStr);

                    byteOp = GetByteReg((byte)addr, out regStr);
                    if (isReg)
                        SetByteReg((byte)addr, (byte)(byteOp + 1));
                    else
                        DataWriteByte((byte)addr, (byte)(byteOp + 1));

                    debugStr += (String.Format("INC {0}", regStr));
                    break;
                default:
                    debugStr += (String.Format("Invalid opcode! '{0}'", op));
                    break;
            #endregion
            }
            dataSegment = SegmentRegister.DS;
            repPrefix = false;
            DebugWriteLine(debugStr);
        }

        public void SetSegment(SegmentRegister register, int addr)
        {
            segments[(int)register].Addr = addr;
        }
    }
}