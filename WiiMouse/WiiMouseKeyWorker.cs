using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WiiLib;

namespace WiiMouseKeyboard
{
    public class WiiMouseKeyWorker:BackgroundWorker
    {
        private WiiController controller;
        public WiiMouseKeyWorker()
        {
            this.WorkerSupportsCancellation = true;
            this.WorkerReportsProgress = true;
            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            //base.OnDoWork(e);
            this.controller = WiiController.GetOldWiiRemote();
            controller.InfraredChanged += controller_InfraredChanged;
            while(this.CancellationPending==false)
            {

            }
        }

        void controller_InfraredChanged(object sender, List<System.Windows.Point> e)
        {
            ReportProgress(0, controller.GetAveragePoint());
        }

        protected override void OnProgressChanged(ProgressChangedEventArgs e)
        {
            //base.OnProgressChanged(e);
            Point point = (Point)e.UserState;
            SetCursorPos((int)point.X * 1280, (int)point.Y * 1920);
        }

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);
    }
}
