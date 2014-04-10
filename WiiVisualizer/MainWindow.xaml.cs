using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
        private WiiController Device;
        private DispatcherTimer timer=new DispatcherTimer();
        private int ledPosition;
        private bool isRumbling;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Device = WiiController.GetOldWiiRemote();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
            timer.Start();
            Device.ButtonDown += Device_ButtonDown;
            Device.ButtonUp += Device_ButtonUp;
            Device.InfraredChanged += Device_InfraredChanged;

            cnvIR.Children.Add(ellipse);
            ellipse.Height = 50;
            ellipse.Width = 50;
            ellipse.Fill = new SolidColorBrush(Colors.Black);
            cnvIR.Children.Add(ellipse2);
            ellipse2.Height = 50;
            ellipse2.Width = 50;
            ellipse2.Fill = new SolidColorBrush(Colors.Black);
            cnvIR.Children.Add(ellipse3);
            ellipse3.Height = 50;
            ellipse3.Width = 50;
            ellipse3.Fill = new SolidColorBrush(Colors.Black);
            cnvIR.Children.Add(ellipse4);
            ellipse4.Height = 50;
            ellipse4.Width = 50;
            ellipse4.Fill = new SolidColorBrush(Colors.Black);
        }
        Ellipse ellipse = new Ellipse();
        Ellipse ellipse2 = new Ellipse();
        Ellipse ellipse3 = new Ellipse();
        Ellipse ellipse4 = new Ellipse();



        void Device_InfraredChanged(object sender, List<Point> e)
        {
            Point point = Device.GetAveragePoint();
            //SetCursorPos((int)(1920-(1920 * point.X)), (int)(1200 * point.Y));

            ellipse.SetValue(Canvas.LeftProperty, point.X * cnvIR.ActualWidth);
            ellipse.SetValue(Canvas.TopProperty, point.Y * cnvIR.ActualHeight);
            /*Point point = e[0];
            ellipse.SetValue(Canvas.LeftProperty, point.X*cnvIR.ActualWidth);
            ellipse.SetValue(Canvas.TopProperty, point.Y * cnvIR.ActualHeight);
            point = e[1];
            ellipse2.SetValue(Canvas.LeftProperty, point.X * cnvIR.ActualWidth);
            ellipse2.SetValue(Canvas.TopProperty, point.Y * cnvIR.ActualHeight);
            point = e[2];
            ellipse3.SetValue(Canvas.LeftProperty, point.X * cnvIR.ActualWidth);
            ellipse3.SetValue(Canvas.TopProperty, point.Y * cnvIR.ActualHeight);
            point = e[3];
            ellipse4.SetValue(Canvas.LeftProperty, point.X * cnvIR.ActualWidth);
            ellipse4.SetValue(Canvas.TopProperty, point.Y * cnvIR.ActualHeight);*/

        }

        void Device_ButtonUp(object sender, WiiController.Button e)
        {
            buttonListBox.Items.Remove(e);
        }

        void Device_ButtonDown(object sender, WiiController.Button e)
        {
            buttonListBox.Items.Add(e);
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

        //[DllImport("User32.Dll")]
        //public static extern long SetCursorPos(int x, int y);
    }
}
