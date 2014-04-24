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
using WiiLib.ViewModel;

namespace WiiVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.g = Graphics.FromImage(this.b);
            g.Clear(System.Drawing.Color.FromArgb(0, 0, 0));
            SetImage();
            (this.DataContext as WiiMoteViewModel).InfraredChanged += MainWindow_InfraredChanged;
        }

        void MainWindow_InfraredChanged(object sender, List<System.Windows.Point> e)
        {
            
            g.Clear(System.Drawing.Color.FromArgb(0,0,0));
            foreach (System.Windows.Point point in e.Where(p=>p.X!=-1&&p.Y!=-1))
                g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.White),(float) point.X * 250f,(float) point.Y * 250f, 10f, 10f);
            var pt = (this.DataContext as WiiMoteViewModel).GetAveragePoint();
            g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.Tomato), (float)pt.X * 250f, (float)pt.Y * 250f, 10f, 10f);
            SetImage();
            
        }

        private void SetImage()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                b.Save(memory, ImageFormat.Png);
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
    }
}
