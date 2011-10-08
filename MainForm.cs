﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using log4net;
using x86CS.Properties;
using System.Collections.Generic;

namespace x86CS
{
    public partial class MainForm : Form
    {
        private readonly Machine machine;
        private readonly Thread machineThread;
        private readonly Breakpoints breakpoints = new Breakpoints();
        private double frequency = 100000.0f;
        private ulong timerTicks;
        private bool running;

        public MainForm()
        {
            timerTicks = 0;
            machine = new Machine();
            Application.ApplicationExit += ApplicationApplicationExit;

            breakpoints.ItemAdded += BreakpointsItemAdded;
            breakpoints.ItemDeleted += BreakpointsItemDeleted;

            InitializeComponent();

            PrintRegisters();

            machine.FloppyDrive.MountImage(@"C:\fdboot.img");
            
            machineThread = new Thread(RunMachine);
            running = true;
            machineThread.Start();
        }

        void ApplicationApplicationExit(object sender, EventArgs e)
        {
            LogManager.Shutdown();
        }

        void BreakpointsItemDeleted(object sender, IntEventArgs e)
        {
            machine.ClearBreakpoint(e.Number);
        }

        void BreakpointsItemAdded(object sender, IntEventArgs e)
        {
            machine.SetBreakpoint(e.Number);
        }

        private void RunMachine()
        {
            var stopwatch = new Stopwatch();
            double lastSeconds = 0;

            stopwatch.Start();

            while (running)
            {
                timerTicks++;

                if(timerTicks % 100000 == 0)
                {
                    frequency = 100000 / (stopwatch.Elapsed.TotalSeconds - lastSeconds);
                    lastSeconds = stopwatch.Elapsed.TotalSeconds;
                    if(Created)
                        Invoke((MethodInvoker)delegate { tpsLabel.Text = frequency.ToString("F2") + "TPS"; }); 
                }
                if (!machine.Running || (machine.Stepping && machine.CPU.InterruptLevel == 0))
                {
                    if(!stepButton.Enabled && Created)
                        Invoke((MethodInvoker) delegate { stepButton.Enabled = true; });
                    continue;
                }
                try
                {
                    if (stepButton.Enabled && Created)
                        Invoke((MethodInvoker)delegate { stepButton.Enabled = false; });
                    machine.RunCycle(frequency, timerTicks);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.ErrorTitle);
                    machine.FlushLog();
                    return;
                }

                if (machine.CheckBreakpoint())
                {
                    string opStr = String.Format("{0:X}:{1:X}  {2}", machine.CPU.CS, machine.CPU.EIP, machine.CPU.DecodeOpString(machine.OPCode, machine.Operands));
                    machine.Stepping = true;

                    Invoke((MethodInvoker)(() =>
                                               {
                                                   PrintRegisters();
                                                   SetCPULabel(opStr);
                                               }));
                }
            }
        }

        private void SetCPULabel(string text)
        {
            cpuLabel.Text = text;
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            machine.Stop();
            running = false;
            Application.Exit();
        }

        private void PrintRegisters()
        {
            CPU.CPU cpu = machine.CPU;

            EAX.Text = cpu.EAX.ToString("X8");
            EBX.Text = cpu.EBX.ToString("X8");
            ECX.Text = cpu.ECX.ToString("X8");
            EDX.Text = cpu.EDX.ToString("X8");
            ESI.Text = cpu.ESI.ToString("X8");
            EDI.Text = cpu.EDI.ToString("X8");
            EBP.Text = cpu.EBP.ToString("X8");
            ESP.Text = cpu.ESP.ToString("X8");
            CS.Text = cpu.CS.ToString("X4");
            DS.Text = cpu.DS.ToString("X4");
            ES.Text = cpu.ES.ToString("X4");
            FS.Text = cpu.FS.ToString("X4");
            GS.Text = cpu.GS.ToString("X4");
            SS.Text = cpu.SS.ToString("X4");

            CF.Text = cpu.CF ? "CF" : "cf";
            PF.Text = cpu.PF ? "PF" : "pf";
            AF.Text = cpu.AF ? "AF" : "af";
            ZF.Text = cpu.ZF ? "ZF" : "zf";
            SF.Text = cpu.SF ? "SF" : "sf";
            TF.Text = cpu.TF ? "TF" : "tf";
            IF.Text = cpu.IF ? "IF" : "if";
            DF.Text = cpu.DF ? "DF" : "df";
            OF.Text = cpu.OF ? "OF" : "of";
            IOPL.Text = cpu.IOPL.ToString("X2");
            AC.Text = cpu.AC ? "AC" : "ac";
            NT.Text = cpu.NT ? "NT" : "nt";
            RF.Text = cpu.RF ? "RF" : "rf";
            VM.Text = cpu.VM ? "VM" : "vm";
            VIF.Text = cpu.VIF ? "VIF" : "vif";
            VIP.Text = cpu.VIP ? "VIP" : "vip";   
        }

        private void RunToolStripMenuItemClick(object sender, EventArgs e)
        {
            runToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            machine.Start();
        }

        private void StopToolStripMenuItemClick(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = true;
            machine.Stop();
        }

        private void MountToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (floppyOpen.ShowDialog() != DialogResult.OK)
                return;

            machine.FloppyDrive.MountImage(floppyOpen.FileName);
        }

        private void StepButtonClick(object sender, EventArgs e)
        {
            machine.Stepping = true;
            string opStr;

            if (!machine.Running)
            {
                machine.Start();
                opStr = String.Format("{0:X}:{1:X}  {2}", machine.CPU.CS, machine.CPU.EIP, machine.CPU.DecodeOpString(machine.OPCode, machine.Operands));
                SetCPULabel(opStr);
                PrintRegisters();
                return;
            }

            machine.RunCycle(frequency, timerTicks);
            opStr = String.Format("{0:X}:{1:X}  {2}", machine.CPU.CS, machine.CPU.EIP, machine.CPU.DecodeOpString(machine.OPCode, machine.Operands));
            SetCPULabel(opStr);
            PrintRegisters();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            if (!machine.Running)
                machine.Start();

            machine.Stepping = false;
        }

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            machineThread.Abort();
        }

        private void MemoryButtonClick(object sender, EventArgs e)
        {
            ushort seg = 0;
            ushort off = 0;

            try
            {
                seg = ushort.Parse(memSegment.Text, NumberStyles.HexNumber);
                off = ushort.Parse(memOffset.Text, NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show(Resources.Invalid_address, Resources.ErrorTitle);
            }
                
            var addr = (uint)((seg << 4) + off);

            memByte.Text = Memory.ReadByte(addr).ToString("X2");
            memWord.Text = Memory.ReadWord(addr).ToString("X4");
        }

        private void BreakpointsToolStripMenuItemClick(object sender, EventArgs e)
        {
            breakpoints.ShowDialog();
        }

        private void RestartToolStripMenuItemClick(object sender, EventArgs e)
        {
            machine.Restart();

            machine.Stepping = false;
        }
    }
}
