using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiLib.ViewModel
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

        public WiiMoteViewModel(): base(0x57E, 0x306)
        {

        }
        public WiiMoteViewModel(int vendorId, int productId): base(vendorId,productId)
        {
            InitializeLeds();
        }

        private void InitializeLeds()
        {
            Leds = new ObservableCollection<bool>();
            for (int i = 0; i < 4; i++)
            {
                Leds.Add(false);
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

        public ObservableCollection<bool> Leds { get; set; }

        protected override void OnLedChanged(bool[] leds)
        {
            for (int i = 0; i < leds.Length; i++)
            {
                Leds[i] = leds[i];
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


        #endregion

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
}
