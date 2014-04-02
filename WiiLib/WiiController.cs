using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using WII.HID.Lib;

namespace WiiLib
{
    public class WiiController:IDisposable
    {
        #region Fields

        private HIDDevice Device { get; set; }
        private byte _byte11;
        private Dictionary<Button, bool> _buttonsState=new Dictionary<Button,bool>();
        private Dictionary<Button, byte> _buttonsMasksFirstByte=new Dictionary<Button,byte>();
        private Dictionary<Button, byte> _buttonsMasksSecondByte= new Dictionary<Button,byte>();
        #endregion

        #region Props
        public bool[] Leds { get; private set; }


        #endregion

        #region Ctors
        public WiiController(): this(0x57E, 0x306){ }

        public WiiController(int vendorId,int productId)
        {
            Dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
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

        #region Events

        public event EventHandler<HIDReport> Report;
        public event EventHandler<HIDReport> StatusReport;
        public event EventHandler<HIDReport> MemoryReport;
        public event EventHandler<HIDReport> AckReport;
        public event EventHandler<HIDReport> CoreButtonsReport;
        public event EventHandler<HIDReport> CoreButtonsAccelReport;
        public event EventHandler<Button> ButtonDown;
        public event EventHandler<Button> ButtonUp;
        public event EventHandler<Button> ButtonPressed;
        

        public virtual void OnReportReceived( HIDReport report)
        {
           if ( report != null)
            {
                if(Report != null)
                    Report(this, report);

                Console.WriteLine("Report: " + report.Data + " @ID:" + report.ReportID);
                switch (report.ReportID)
                {
                    case 0x20:
                        // Status report
                        OnStatusReportReceived( report);
                        break;
                    case 0x21:
                        // Memory en register read report
                        OnMemoryReportReceived( report);
                        break;
                    case 0x22:
                        // Acknowledge report
                        OnAckReportReceived( report);
                        break;
                    case 0x30:
                        // Core buttons data report
                        OnCoreButtonsReportReceived(report);
                        break;
                    case 0x31:
                        // Core buttons en accelerometer data report
                        OnCoreButtonsAccelReportReceived(report);
                        break;
                    case 0x37:
                        break;
                } 
           }
        }

        public virtual void OnStatusReportReceived( HIDReport report)
        {
            if (StatusReport != null && report != null)
            {
                StatusReport(this, report);
                Console.WriteLine("StatusReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnMemoryReportReceived( HIDReport report)
        {

            if (MemoryReport != null && report != null)
            {
                MemoryReport(this, report);
                Console.WriteLine("MemoryReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnAckReportReceived( HIDReport report)
        {

            if (AckReport != null && report != null)
            {
                AckReport(this, report);
                Console.WriteLine("AckReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnCoreButtonsReportReceived( HIDReport report)
        {
            if ( report != null)
            {
                if(CoreButtonsReport != null)
                    CoreButtonsReport(this, report);
                Console.WriteLine("CoreButtonsReport @ID:" + report.ReportID);
                ProcessButtonEvent( report.Data[0],report.Data[1]);
            }
        }

        

        public virtual void OnCoreButtonsAccelReportReceived(HIDReport report)
        {
            if (CoreButtonsAccelReport != null && report != null)
            {
                CoreButtonsAccelReport(this, report);
                Console.WriteLine("CoreButtonsAccelReport @ID:" + report.ReportID);
            }
        }

        public virtual void OnButtonDown(Button button)
        {
            if (ButtonDown != null && button != null)
            {
                ButtonDown(this, button);
                Console.WriteLine("ButtonDown:" + button);
            }
        }

        public virtual void OnButtonUp( Button button)
        {
            if (ButtonUp != null && button != null)
            {
                ButtonUp(this, button);
                Console.WriteLine("ButtonUp:" + button);
            }
        }

        public virtual void OnButtonPressed( Button button)
        {
            if (ButtonPressed != null && button != null)
            {
                ButtonPressed(this, button);
                Console.WriteLine("ButtonPressed:" + button);
            }
        }

        private void ProcessButtonEvent(byte currentByte1, byte currentByte2)
        {
            foreach (var buttonMask in _buttonsMasksFirstByte)
            {
                Button button = buttonMask.Key;
                byte mask = buttonMask.Value;
                if (IsMaskOn(mask, currentByte1))
                {
                    if (!_buttonsState[button])//if the button was not down but now is
                    {
                        _buttonsState[button] = true;
                        OnButtonDown( button);
                    }
                }
                else
                {
                    if (_buttonsState[button])//if the button was down, but now is not
                    {
                        _buttonsState[button] = false;
                        OnButtonUp( button);
                        OnButtonPressed( button);
                    }
                }
            }

            foreach (var buttonMask in _buttonsMasksSecondByte)
            {
                Button button = buttonMask.Key;
                byte mask = buttonMask.Value;
                if (IsMaskOn(mask, currentByte2))
                {
                    if (!_buttonsState[button])//if the button was not down but now is
                    {
                        _buttonsState[button] = true;
                        OnButtonDown( button);
                    }
                }
                else
                {
                    if (_buttonsState[button])//if the button was down, but now is not
                    {
                        _buttonsState[button] = false;
                        OnButtonUp( button);
                        OnButtonPressed( button);
                    }
                }
            }
        }

        #endregion

        #region WiiFunctions

        #region Rumble

        public void StartRumbling()
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;
            _byte11 = (byte)(_byte11 | 0x1);
            report.Data[0] = _byte11;
            WriteReport(report);
        }

        public void StopRumbling()
        {
            HIDReport report = Device.CreateReport();
            report.ReportID = 0x11;
            _byte11=(byte)(_byte11 & 0xF0);
            report.Data[0] =_byte11 ;
            WriteReport(report);
        }

        public void Rumble(double milliseconds)
        {
            StartRumbling();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = milliseconds;
            timer.Start();
            timer.Elapsed += rumble_Elapsed;
        }

        public void ToggleRumble()
        {
            if ((_byte11 & 0x1) != 1)
            {
                StartRumbling();
            }
            else
                StopRumbling();
        }

        void rumble_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((System.Timers.Timer)sender).Dispose();
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
            _byte11 =(byte)( _byte11 | byt);
            report.Data[0] = _byte11;
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
            _byte11 = (byte)(_byte11 &(255- byt));
            report.Data[0] = _byte11;
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

        Dispatcher Dispatcher { get; set; }

        private void OnReadReport(HIDReport report)
        {
            if (Thread.CurrentThread!=Dispatcher.Thread)
            {
                Dispatcher.Invoke(new ReadReportCallback(OnReadReport), report);
            }
            else
            {
                OnReportReceived(report);//good reports, but not on the right thread
                Device.ReadReport(OnReadReport);
            }
        }

        private void Invoke(ReadReportCallback readReportCallback, HIDReport report)
        {
            readReportCallback.Invoke(report);
        }

        private void StartReadingReports()
        {
            InitializeButtonsState();
            Device.ReadReport(OnReadReport);
            InitializeIR();
        }

        private void InitializeButtonsState()
        {
            foreach(Button button in Enum.GetValues(typeof(Button)))
            {
                _buttonsState.Add(button, false);
            }
            _buttonsMasksFirstByte.Add(Button.LEFT, 0x01);
            _buttonsMasksFirstByte.Add(Button.RIGHT, 0x02);
            _buttonsMasksFirstByte.Add(Button.DOWN, 0x04);
            _buttonsMasksFirstByte.Add(Button.UP, 0x08);
            _buttonsMasksFirstByte.Add(Button.PLUS, 0x10);

            _buttonsMasksSecondByte.Add(Button.TWO, 0x01);
            _buttonsMasksSecondByte.Add(Button.ONE, 0x02);
            _buttonsMasksSecondByte.Add(Button.B, 0x04);
            _buttonsMasksSecondByte.Add(Button.A, 0x08);
            _buttonsMasksSecondByte.Add(Button.MINUS, 0x10);
            _buttonsMasksSecondByte.Add(Button.HOME, 0x80);

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

        private bool IsMaskOn(byte mask,byte status)
        {
            return mask == (status & mask);
        }

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
