using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WiiLib;

namespace WiiVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region fields
        private Wii Device;
        private DispatcherTimer timer=new DispatcherTimer();
        private int ledPosition;
        private bool isRumbling;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Device = new Wii();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        void timer_Tick(object sender, EventArgs e)
        {
            LedShift();
        }

        private void LedShift()
        {
            int prev = ledPosition;
            ledPosition++;
            ledPosition = ledPosition % 4;
            Device.TurnOnLed(ledPosition);
            Device.TurnOffLed(prev);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Device.Dispose();
        }

        private void Rumble_Click(object sender, RoutedEventArgs e)
        {
            /*if (isRumbling)
                Device.StopRumbling();
            else
                Device.StartRumbling();
            isRumbling = !isRumbling;*/
            Device.Rumble(800);
            //Device.ToggleRumble();
        }
    }
}
