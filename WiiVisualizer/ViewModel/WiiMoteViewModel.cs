using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WiiLib;

namespace WiiVisualizer.ViewModel
{
    public class WiiMoteViewModel:WiiController,INotifyPropertyChanged
    {

        #region ctors
        public static WiiMoteViewModel GetOldWiiMoteViewModel()
        {
            return new WiiMoteViewModel(0x57E, 0x306);
        }

        public static WiiMoteViewModel GetNewWiiMoteViewModel()
        {
            return new WiiMoteViewModel(0x57E, 0x330);
        }

        public WiiMoteViewModel(): this(0x57E, 0x306)
        {
        }
        public WiiMoteViewModel(int vendorId, int productId): base(vendorId,productId)
        {
            InitializeLeds();
        }

        

        private void InitializeLeds()
        {
            Leds = new List<Led>();
            for (int i = 0; i < 4; i++)
            {
                
                Leds.Add(new Led());
            }
            for (int i = 0; i < 4; i++)
            {

                TurnOffLed(i);
            }
            
            
        }

        #endregion

        #region props


        public override float BatteryLevel
        {
            protected set
            {
                base.BatteryLevel = value;
                OnPropertyChanged("BatteryLevel");
            }
        }

        public override bool IsBatteryNearlyEmpty
        {
            set
            {
                base.IsBatteryNearlyEmpty = value;
                OnPropertyChanged("IsBatteryNearlyEmpty");
            }
        }

        public new List<Led> Leds { get; set; }

        protected override void OnLedChanged(bool[] leds)
        {
            for (int i = 0; i < leds.Length; i++)
            {
                Leds[i].IsOn = leds[i];
            }
            base.OnLedChanged(leds);
        }

        protected override void OnButtonDown(Button button)
        {
            base.OnButtonDown(button);
            OnButtonChanged(button, true);
            
        }

        protected override void OnButtonUp(Button button)
        {
            base.OnButtonUp(button);
            OnButtonChanged(button, false);
        }

        private void OnButtonChanged(Button button, bool p)
        {
            switch (button)
            {
                case Button.A:
                    IsADown = p;
                    OnPropertyChanged("IsADown");
                    break;
                case Button.B:
                    IsBDown = p;
                    OnPropertyChanged("IsBDown");
                    break;
                case Button.UP:
                    IsUpDown = p;
                    OnPropertyChanged("IsUpDown");
                    break;
                case Button.LEFT:
                    IsLeftDown = p;
                    OnPropertyChanged("IsLeftDown");
                    break;
                case Button.DOWN:
                    IsDownDown = p;
                    OnPropertyChanged("IsDownDown");
                    break;
                case Button.RIGHT:
                    IsRightDown = p;
                    OnPropertyChanged("IsRightDown");
                    break;
                case Button.MINUS:
                    IsMinusDown = p;
                    OnPropertyChanged("IsMinusDown");
                    break;
                case Button.PLUS:
                    IsPlusDown = p;
                    OnPropertyChanged("IsPlusDown");
                    break;
                case Button.HOME:
                    IsHomeDown = p;
                    OnPropertyChanged("IsHomeDown");
                    break;
                case Button.ONE:
                    IsOneDown = p;
                    OnPropertyChanged("IsOneDown");
                    break;
                case Button.TWO:
                    IsTwoDown = p;
                    OnPropertyChanged("IsTwoDown");
                    break;


            }
        }

        public bool IsADown { get; set; }
        public bool IsBDown { get; set; }
        public bool IsUpDown { get; set; }
        public bool IsLeftDown { get; set; }
        public bool IsRightDown { get; set; }
        public bool IsDownDown { get; set; }
        public bool IsMinusDown { get; set; }
        public bool IsPlusDown { get; set; }
        public bool IsHomeDown { get; set; }
        public bool IsOneDown { get; set; }
        public bool IsTwoDown { get; set; }

        private ObservableCollection<DataPoint> _points= new ObservableCollection<DataPoint>();

        public ObservableCollection<DataPoint> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                OnPropertyChanged("Points");
            }
        }
        

        #endregion

        public void ToggleLed(Led led)
        {
            int index = Leds.IndexOf(led);
            if (led.IsOn)
                TurnOnLed(index);
            else
                TurnOffLed(index);
        }



        #region OnPropChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if(PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion



        
    }

    public class DataPoint:INotifyPropertyChanged
    {
        public DataPoint(DateTime time, Acceleration accel)
        {
            Time = time;
            Acceleration = accel;
        }
        private DateTime _time;

        public DateTime Time
        {
            get { return _time; }
            set { _time = value;
                OnPropertyChanged("Time");
            }
        }

        private Acceleration _accel;

        public Acceleration Acceleration
        {
            get { return _accel; }
            set { _accel = value;
            OnPropertyChanged("Acceleration");
            }
        }

        public float X
        {
            get { return Acceleration.X; }
            set {Acceleration.X = value;
            OnPropertyChanged("X");
            }
        }

        public float Y
        {
            get { return Acceleration.Y; }
            set
            {
                Acceleration.Y= value;
                OnPropertyChanged("Y");
            }
        }

        public float Z
        {
            get { return Acceleration.Z; }
            set
            {
                Acceleration.Z = value;
                OnPropertyChanged("Z");
            }
        }
        



        #region OnPropChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public class Led:INotifyPropertyChanged
    {
        private bool _isOn;

        public bool IsOn
        {
            get { return _isOn; }
            set { _isOn = value;
            OnPropertyChanged("IsOn");
            }
        }
        #region OnPropChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    
}
