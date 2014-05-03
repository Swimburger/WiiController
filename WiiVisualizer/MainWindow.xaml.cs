using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using WiiVisualizer.ViewModel;

namespace WiiVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        WiiMoteViewModel vm;
        DispatcherTimer tmr = new DispatcherTimer();
        DateTime initTime = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();
            vm=(this.DataContext as WiiMoteViewModel);
            this.g = Graphics.FromImage(this.b);
            g.Clear(System.Drawing.Color.FromArgb(0, 0, 0));
            SetImage();
            vm.InfraredChanged += MainWindow_InfraredChanged;
            tmr.Tick += tmr_Tick;
            tmr.Interval = TimeSpan.FromMilliseconds(200);
            tmr.Start();

            
        }

       

        void MainWindow_InfraredChanged(object sender, List<System.Windows.Point> e)
        {
            
            g.Clear(System.Drawing.Color.FromArgb(0,0,0));
            foreach (System.Windows.Point point in e.Where(p=>p.X!=-1&&p.Y!=-1))
                g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.White), (float)(point.X * irImg.ActualWidth), (float)(point.Y * irImg.ActualHeight), 5, 5);
            var pt = vm.GetAveragePoint();
            g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.Tomato), (float)(pt.X * irImg.ActualWidth), (float)(pt.Y * irImg.ActualHeight), 5, 5);
            SetImage();
            
        }

        private void SetImage()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                b.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                irImg.Source = bitmapImage;
            }
        }

        private Bitmap b = new Bitmap(256, 192);

        private Graphics g;


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var vm =(this.DataContext as WiiMoteViewModel);
            var led = (sender as CheckBox).DataContext as Led;
            vm.ToggleLed(led);
        }
        void tmr_Tick(object sender, EventArgs e)
        {
            vm.Points.Add(new DataPoint(DateTime.Now, vm.LastAcceleration));
            if (vm.Points.Count > 20) vm.Points.RemoveAt(0);
            //double span=DateTimeAxis.ToDouble(DateTime.Now);
            //vm.YPoints.Add(new DataPoint(span, vm.LastAcceleration.Y));
            //vm.XPoints.Add(new DataPoint(span, vm.LastAcceleration.X));
            //vm.ZPoints.Add(new DataPoint(span, vm.LastAcceleration.Z));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm.ToggleRumble();
        }
    }
}
