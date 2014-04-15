using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
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
        Dispatcher _dispatcher=Dispatcher.CurrentDispatcher;

        private bool _isBatteryNearlyEmpty;
        #endregion

        #region Props
        public virtual bool[] Leds { get; protected set; }
        public virtual bool IsBatteryNearlyEmpty
        {
            get
            {
                return _isBatteryNearlyEmpty;
            }
            set
            {
                _isBatteryNearlyEmpty = value;
            }
        }

        public virtual bool IsExtensionControllerConnected { get; protected set; }

        public virtual bool IsSpeakerEnabled { get; protected set; }

        public virtual bool IsIRCamEnabled { get; protected set; }

        public virtual float BatteryLevel { get;protected set; }

        public virtual Acceleration LastAcceleration { get; set; }

        public virtual List<Point> InfraredPoints { get; set; }

        #endregion

        #region Ctors

        public WiiController(int vendorId,int productId)
        {
            LastAcceleration = new Acceleration(0, 0, 0);
            //_dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            InitializeWii(vendorId, productId);
            StartReadingReports();
        }

        public static WiiController GetOldWiiRemote()
        {
            return new WiiController(0x57E, 0x306);
        }

        public static WiiController GetNewWiiRemote()
        {
            return new WiiController(0x57E, 0x330);
        }

        private void InitializeWii(int vendorId, int productId)
        {
            Device = HIDDevice.GetHIDDevice(vendorId, productId);
            Leds = new bool[4];
        }

        #endregion

        #region Events

        public event EventHandler<HIDReport> Report;
        public event EventHandler<HIDReport> StatusReport;
        public event EventHandler<HIDReport> MemoryReport;
        public event EventHandler<HIDReport> AckReport;
        public event EventHandler<HIDReport> CoreButtonsReport;
        public event EventHandler<HIDReport> CoreButtonsAccelReport;
        public event EventHandler<HIDReport> CoreButtonsAccelIRExtensionReport;
        public event EventHandler<Button> ButtonDown;
        public event EventHandler<Button> ButtonUp;
        public event EventHandler<Button> ButtonPressed;
        public event EventHandler<Acceleration> Accelerated;
        public event EventHandler<List<Point>> InfraredChanged;
        public event EventHandler<bool[]> LedChanged;
        

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
                        OnCoreButtonsAccelIRExtensionReportReceived(report);
                        break;
                } 
           }
        }

        public virtual void OnStatusReportReceived( HIDReport report)
        {
            if (report != null)
            {
                if (StatusReport != null)
                {
                    StatusReport(this, report);
                    Console.WriteLine("StatusReport @ID:" + report.ReportID);
                }
                ProcessStatusReport(report.Data);
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
                ProcessButtonBytes( report.Data[0],report.Data[1]);
            }
        }

        public virtual void OnCoreButtonsAccelReportReceived(HIDReport report)
        {
            if (report != null)
            {
                if (CoreButtonsAccelReport != null)
                {
                    CoreButtonsAccelReport(this, report);
                    Console.WriteLine("CoreButtonsAccelReport @ID:" + report.ReportID);
                }
                ProcessButtonBytes(report.Data[0], report.Data[1]);
                ProcessAccelerometer(report.Data);
            }
        }

        

        public virtual void OnCoreButtonsAccelIRExtensionReportReceived(HIDReport report)
        {
            if (report != null)
            {
                if (CoreButtonsAccelIRExtensionReport != null)
                {
                    CoreButtonsAccelIRExtensionReport(this, report);
                    Console.WriteLine("CoreButtonsAccelIRReport @ID:" + report.ReportID);
                }
                ProcessButtonBytes(report.Data[0], report.Data[1]);
                ProcessAccelerometer(report.Data);
                ProcessIR(report.Data);
                //extensions need to be implemented by extending class
            }
        }

        private void ProcessIR(byte[] data)
        {
            ProcessBasicIR(data.Skip(5).Take(5).ToArray(),0);
            ProcessBasicIR(data.Skip(10).Take(5).ToArray(),1);
        }

        /// <summary>
        /// Method to process the infrared camera
        /// </summary>
        /// <param name="data">5 bytes from 2 IR points</param>
        /// <param name="part">if set 0, point 1 and 2. If set 1, point 3 and 4</param>
        private void ProcessBasicIR(byte[] data,int part)
        {
            if(data.Count()!=5)
                throw new ArgumentException("data length should be 5");
            double x1 = data[0];
            double y1 = data[1];
            double x2 = data[3];
            double y2 = data[4];

            byte b2 = data[2];
            y1 += (b2&0xc0)<<2;
            x1 += (b2 & 0x30)<<4;
            y2 += (b2 & 0x0c) << 6;
            x2 += (b2 & 0x03) << 8;

            bool isPoint1 = (x1 != 1023 || y1 != 1023);
            bool isPoint2 = (x2 != 1023 || y2 != 1023);
            
            //Console.WriteLine("x1: " + x1 + ", y1: " + y1 + ", x2: " + x2 + ", y2: " + y2);
            if(!isPoint1)
            {
                InfraredPoints[0 + (part * 2)] = new Point(-1, -1);
            }
            if (!isPoint2)
            {
                InfraredPoints[1 + (part * 2)] = new Point(-1, -1);
            }

            if (!isPoint1 && !isPoint2) return;
            

            x1 = x1 / 1023d;
            x2 = x2 / 1023d;

            y1 = y1 / 767d;
            y2 = y2 / 767d;
            //y1 = y1 / 1023d;
            //y2 = y2 / 1023d;

            
            
            if (isPoint1)
            {
                InfraredPoints[0 + (part * 2)] = new Point(x1,y1);
            }
            if (isPoint2)
            {
                InfraredPoints[1 + (part * 2)] = new Point(x2, y2);
            }
                OnInfraredChanged(InfraredPoints);
        }

        

        protected virtual void OnButtonDown(Button button)
        {
            if (ButtonDown != null && button != null)
            {
                ButtonDown(this, button);
                Console.WriteLine("ButtonDown:" + button);
            }
        }

        protected virtual void OnButtonUp( Button button)
        {
            if (ButtonUp != null && button != null)
            {
                ButtonUp(this, button);
                Console.WriteLine("ButtonUp:" + button);
            }
        }

        protected virtual void OnButtonPressed( Button button)
        {
            if (ButtonPressed != null && button != null)
            {
                ButtonPressed(this, button);
                Console.WriteLine("ButtonPressed:" + button);
            }
        }

        protected virtual void OnInfraredChanged(List<Point> list)
        {
            if (InfraredChanged != null)
            {
                InfraredChanged(this,list);
            }
        }

        protected virtual void OnLedChanged(bool[] leds)
        {
            if (LedChanged != null)
                LedChanged(this, leds);
        }

        private void ProcessButtonBytes(byte currentByte1, byte currentByte2)
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

        private void ProcessStatusReport(Byte[] data)
        {
            byte lf = data[2];
            IsBatteryNearlyEmpty = IsMaskOn(0x01, lf);
            IsExtensionControllerConnected = IsMaskOn(0x02, lf);
            IsSpeakerEnabled = IsMaskOn(0x04, lf);
            IsIRCamEnabled = IsMaskOn(0x08, lf);
            ChangeLed(0,IsMaskOn(0x10, lf));
            ChangeLed(1,IsMaskOn(0x20, lf));
            ChangeLed(2,IsMaskOn(0x40, lf));
            ChangeLed(3,IsMaskOn(0x80, lf));

            //according to test 192
            BatteryLevel = (float)(data[5] / 192f); ;
        }

        private void ProcessAccelerometer(byte[] data)
        {
            byte lsbx = (byte)(data[0] & 0x60);
            byte lsbz = (byte)(data[1] & 0x40);
            byte lsby = (byte)(data[1] & 0x20);
            LastAcceleration.X = (0x200 - ((data[2]) << 2) | (lsbx >> 5)) / 10.23f;
            LastAcceleration.Y = (0x200 - ((data[3]) << 2) | (lsby >> 4)) / 10.23f;
            LastAcceleration.Z = (0x200 - ((data[4]) << 2) | (lsbz >> 5)) / 10.23f;
            if (Accelerated != null)
                Accelerated(this, LastAcceleration);
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
            ChangeLed(ledPosition, true);
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
            ChangeLed(ledPosition, false);
        }

        private void ChangeLed(int position,Boolean isOn)
        {
            if (Leds[position] == isOn) return;
            Leds[position] = isOn;
            OnLedChanged(Leds);
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

        private void OnReadReport(HIDReport report)
        {
            if (Thread.CurrentThread!=_dispatcher.Thread)
            {
                _dispatcher.Invoke(new ReadReportCallback(OnReadReport), report);
            }
            else
            {
                OnReportReceived(report);//good reports, but not on the right thread
                Device.ReadReport(OnReadReport);
            }
        }

        private void StartReadingReports()
        {
            InitializeButtonsState();
            Device.ReadReport(OnReadReport);
            InitializeIR();
            RequestStatus();
            SetDataReportingMode(0x37);
            
        }

        private void RequestStatus()
        {
            var report = CreateReport();
            report.ReportID=0x15;
            WriteReport(report);
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
            InfraredPoints = new List<Point>();
            InfraredPoints.Add(new Point(-1,-1));
            InfraredPoints.Add(new Point(-1, -1));
            InfraredPoints.Add(new Point(-1, -1));
            InfraredPoints.Add(new Point(-1, -1));

            HIDReport report = CreateReport();
            report.ReportID = 0x13;
            report.Data[0] = 0x4;
            WriteReport(report);

            report = CreateReport();
            report.ReportID = 0x1A;
            report.Data[0] = 0x4;
            WriteReport(report);

            WriteData(0xB00030, new byte[] { 0x8 });

            WriteData(0xB00000, new byte[] { 0x02, 0, 0, 0x71, 0x01, 0, 0x90, 0, 0x41 });

            WriteData(0xB0001A, new byte[] { 0x40, 0 });

            WriteData(0xB00033, new byte[] { 0x01});

            WriteData(0xB00030, new byte[] { 0x08 });

            #region WiiIRInitalization

            /*HIDReport report = CreateReport();
            report.ReportID = 0x13;
            report.Data[0] = 0x06;
            WriteReport(report);

            report = CreateReport();
            report.ReportID = 0x1a;
            report.Data[0] = 0x06;
            WriteReport(report);

            WriteData(0xB00030, new byte[] { 0x01 });

            WriteData(0xB00000, new byte[] { 0x02, 0, 0, 0x71, 0x01, 0, 0x90, 0, 0x41 });

            WriteData(0xB0001A, new byte[] { 0x40, 0 });

            WriteData(0xB00033, new byte[] { 0x01 });

            WriteData(0xB00030, new byte[] { 0x08 });*/

            #endregion

        }

        private void SetDataReportingMode(byte mode)
        {
            var report = CreateReport();
            report.ReportID = 0x12;
            report.Data[1] = mode;
            WriteReport(report);
        }

        #endregion

        public Point GetAveragePoint()
        {
            return GetAveragePoint(InfraredPoints);
        }

        private static Point GetAveragePoint(List<Point> points)
        {
            points=points.Where(p => p.Y != -1 && p.X != -1).ToList();
            return new Point(points.Average(p => p.X), points.Average(p => p.Y));
        }

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

    #region Enum

    public enum Button
    {
        A, B, UP, RIGHT, DOWN, LEFT, MINUS, PLUS, HOME, ONE, TWO
    }

    #endregion

    public class ButtonState
    {
        public Button Button { get; set; }
        public bool IsDown { get; set; }
        public ButtonState(Button button)
        {

        }
    }
}
