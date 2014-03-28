using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using WII.HID.Lib;

namespace WiiLib
{
    public class Wii:IDisposable
    {
        #region Fields

        private HIDDevice Device { get; set; }
        private byte byte11;

        #endregion

        #region Props
        public bool[] Leds { get; private set; }


        #endregion

        #region Ctors
        public Wii(): this(0x57E, 0x306){ }

        public Wii(int vendorId,int productId)
        {
            Device = HIDDevice.GetHIDDevice(vendorId, productId);
            Report += Wii_Report;
            Leds = new bool[4];
            Device.ReadReport(OnReadReport);
        }

        #endregion

        #region events

        public event EventHandler<HIDReport> Report;
        public event EventHandler<HIDReport> StatusReport;
        public event EventHandler<HIDReport> MemoryReport;
        public event EventHandler<HIDReport> AckReport;
        public event EventHandler<HIDReport> CoreButtonsReport;
        public event EventHandler<HIDReport> CoreButtonsAccelReport;

        private void Wii_Report(object sender, HIDReport report)
        {
            switch (report.ReportID)
            {
                case 0x20:
                    // Status report
                    OnStatusReportReceived(this, report);
                    break;
                case 0x21:
                    // Memory en register read report
                    OnMemoryReportReceived(this, report);
                    break;
                case 0x22:
                    // Acknowledge report
                    OnAckReportReceived(this, report);
                    break;
                case 0x30:
                    // Core buttons data report
                    OnCoreButtonsReportReceived(this, report);
                    break;
                case 0x31:
                    // Core buttons en accelerometer data report
                    OnCoreButtonsAccelReportReceived(this, report);
                    break;
                case 0x37:
                    break;
            }
        }

        public virtual void OnReportReceived(Wii wii, HIDReport report)
        {

            if (Report != null && report != null)
                Report(this, report);
        }

        public virtual void OnStatusReportReceived(Wii wii, HIDReport report)
        {
            if (StatusReport != null && report != null)
                StatusReport(this, report);
        }

        public virtual void OnMemoryReportReceived(Wii wii, HIDReport report)
        {

            if (MemoryReport != null && report != null)
                MemoryReport(this, report);
        }

        public virtual void OnAckReportReceived(Wii wii, HIDReport report)
        {

            if (AckReport != null && report != null)
                AckReport(this, report);
        }

        public virtual void OnCoreButtonsReportReceived(Wii wii, HIDReport report)
        {
            if (CoreButtonsReport != null && report != null)
                CoreButtonsReport(this, report);
        }

        public virtual void OnCoreButtonsAccelReportReceived(Wii wii, HIDReport report)
        {
            if (CoreButtonsAccelReport != null && report != null)
                CoreButtonsAccelReport(this, report);
        }

        #endregion

        #region WiiFunctions

        #region Rumble

        public void StartRumbling()
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;
            byte11 = (byte)(byte11 | 0x1);
            report.Data[0] = byte11;
            WriteReport(report);
        }

        public void StopRumbling()
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;
            byte11=(byte)(byte11 & 0xF0);
            report.Data[0] =byte11 ;
            WriteReport(report);
        }

        public void Rumble(double milliseconds)
        {
            StartRumbling();
            Timer timer = new Timer();
            timer.Interval = milliseconds;
            timer.Start();
            timer.Elapsed += rumble_Elapsed;
        }

        public void ToggleRumble()
        {
            if ((byte11 & 0x1) != 1)
            {
                StartRumbling();
            }
            else
                StopRumbling();
        }

        void rumble_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Dispose();
            StopRumbling();
        }

        #endregion

        #region Leds

        public void TurnOnLed(int ledPosition)
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;

            byte byt=0;
           
            switch(ledPosition)
            {
                case 0:
                    byt = 0x10;
                    break;
                case 1:
                    byt = 0x20;
                    break;
                case 2:
                    byt = 0x40;
                    break;
                case 3:
                    byt = 0x80;
                    break;
            }
            byte11 =(byte)( byte11 | byt);
            report.Data[0] = byte11;
            WriteReport(report);
            Leds[ledPosition] = true;
        }
        public void TurnOffLed(int ledPosition)
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;

            byte byt = 0;

            switch (ledPosition)
            {
                case 0:
                    byt = 0x10;
                    break;
                case 1:
                    byt = 0x20;
                    break;
                case 2:
                    byt = 0x40;
                    break;
                case 3:
                    byt = 0x80;
                    break;
            }
            byte11 = (byte)(byte11 &(255- byt));
            report.Data[0] = byte11;
            WriteReport(report);
            Leds[ledPosition] = false;
        }

        #endregion

        #endregion

        #region WriteReport

        private HIDReport CreateReport()
        {
            return Device.CreateReport();
        }

        private void WriteReport(HIDReport report)
        {
            Device.WriteReport(report);
        }

        #endregion

        #region ReadReports

        public void StartReading()
        {
            InvokeRequired = true;
            OnReadReport(Device.CreateReport());
        }

        public bool InvokeRequired { get; set; }

        private void OnReadReport(HIDReport report)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ReadReportCallback(OnReadReport), report);
            }
            else
            {
                OnReportReceived(this, report);
                Device.ReadReport(OnReadReport);
            }
        }

        private void Invoke(ReadReportCallback readReportCallback, HIDReport report)
        {
            readReportCallback.Invoke(report);
        }

        #endregion

        private bool IsBitOn(byte status, int i)
        {
            int pow = (int)Math.Pow(2, i);
            return pow == (status & Convert.ToByte(pow));
        }

        public void Dispose()
        {
            Device.Dispose();
        }        
    }
}
