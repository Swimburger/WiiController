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
        private byte[] bytes48;

        #endregion

        #region Props
        public bool[] Leds { get; private set; }


        #endregion

        #region Ctors
        public Wii(): this(0x57E, 0x306){ }

        public Wii(int vendorId,int productId)
        {
            InitializeWii(vendorId, productId);
            StartReadingReports();
        }

        private void InitializeWii(int vendorId, int productId)
        {
            Device = HIDDevice.GetHIDDevice(vendorId, productId);
            Leds = new bool[4];
        }

        #endregion

        #region Enum

        public enum Button
        {
            A,B,POWER,UP,RIGHT,DOWN,LEFT,MINUS,PLUS,HOME,ONE,TWO
        }

        #endregion

        #region events

        public event EventHandler<HIDReport> Report;
        public event EventHandler<HIDReport> StatusReport;
        public event EventHandler<HIDReport> MemoryReport;
        public event EventHandler<HIDReport> AckReport;
        public event EventHandler<HIDReport> CoreButtonsReport;
        public event EventHandler<HIDReport> CoreButtonsAccelReport;
        public event EventHandler<Button> ButtonDown;
        public event EventHandler<Button> ButtonUp;
        public event EventHandler<Button> ButtonPressed;
        

        public virtual void OnReportReceived(Wii wii, HIDReport report)
        {
           if (Report != null && report != null)
            {
                Report(this, report);
                Console.WriteLine("Report: " + report.Data + " @ID:" + report.ReportID);
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
        }

        public virtual void OnStatusReportReceived(Wii wii, HIDReport report)
        {
            if (StatusReport != null && report != null)
            {
                StatusReport(this, report);
                Console.WriteLine("StatusReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnMemoryReportReceived(Wii wii, HIDReport report)
        {

            if (MemoryReport != null && report != null)
            {
                MemoryReport(this, report);
                Console.WriteLine("MemoryReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnAckReportReceived(Wii wii, HIDReport report)
        {

            if (AckReport != null && report != null)
            {
                AckReport(this, report);
                Console.WriteLine("AckReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnCoreButtonsReportReceived(Wii wii, HIDReport report)
        {
            if (CoreButtonsReport != null && report != null)
            {
                CoreButtonsReport(this, report);
                Console.WriteLine("CoreButtonsReport @ID:" + report.ReportID);
                ProcessButtonEvent(bytes48, report.Data);
            }
        }

        private void ProcessButtonEvent(byte[] prevBytes, byte[] currentBytes)
        {
            //first get the indexes of the values that are different
            //foreach difference check if its an up or down
            //foreach difference check what button it is
            //fire the right events
        }

        public virtual void OnCoreButtonsAccelReportReceived(Wii wii, HIDReport report)
        {
            if (CoreButtonsAccelReport != null && report != null)
            {
                CoreButtonsAccelReport(this, report);
                Console.WriteLine("CoreButtonsAccelReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnButtonDown(Wii wii,Button button)
        {
            if (ButtonDown != null && button != null)
            {
                ButtonDown(this, button);
                Console.WriteLine("ButtonDown:" + button);
            }
        }

        public virtual void OnButtonUp(Wii wii, Button button)
        {
            if (ButtonUp != null && button != null)
            {
                ButtonUp(this, button);
                Console.WriteLine("ButtonUp:" + button);
            }
        }

        public virtual void OnButtonPressed(Wii wii, Button button)
        {
            if (ButtonPressed != null && button != null)
            {
                ButtonPressed(this, button);
                Console.WriteLine("ButtonPressed:" + button);
            }
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

        private void WriteData(int address,byte[] data)
        {
            if(Device!=null&&data!=null)
            {
                int index = 0;
                while(index<data.Length)
                {
                    int leftOver = data.Length - index;

                    int count = (leftOver > 16 ? 16 : leftOver);

                    int tempAddress = address + index;

                    HIDReport report = CreateReport();
                    report.ReportID = 0x16;
                    report.Data[0] = (byte)((tempAddress & 0x4000000) >> 0x18);
                    report.Data[1] = (byte)((tempAddress & 0xff0000) >> 0x10);
                    report.Data[2] = (byte)((tempAddress & 0xff00) >> 0x8);
                    report.Data[3] = (byte)((tempAddress & 0xff));
                    report.Data[4] = (byte)count;

                    Buffer.BlockCopy(data, index, report.Data, 5, count);
                    WriteReport(report);
                    index += 16;
                }
            }else
                throw new ArgumentNullException("The device or data is null");
        }

        

        #endregion

        #region ReadReports

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

        private void StartReadingReports()
        {
            Device.ReadReport(OnReadReport);
            InitializeIR();
        }

        private void InitializeIR()
        {
            HIDReport report = CreateReport();
            report.ReportID = 0x13;
            report.Data[0] = 0x2;
            WriteReport(report);

            report = CreateReport();
            report.ReportID = 0x1A;
            report.Data[0] = 0x2;
            WriteReport(report);

            WriteData(0xB00030, new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x90, 0x00, 0x41 });

            WriteData(0xB0001A, new byte[] { 0x40, 0x00 });

            WriteData(0xB00033, new byte[] { 0x22, 0x03 });//onzeker

            WriteData(0xB00030, new byte[] { 0x08 });
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
