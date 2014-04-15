using Afkvalley;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using WiiLib;

namespace WiiMouseKeyboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        WiiController controller;
        double width, height;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var screens =WpfScreen.AllScreens();
            foreach(WpfScreen screen in screens)
            {
                width += screen.DeviceBounds.Width;
                height += screen.DeviceBounds.Height;
            }
            controller = WiiController.GetOldWiiRemote();
            controller.InfraredChanged += controller_InfraredChanged;
            controller.ButtonDown+= controller_ButtonDown;
            controller.ButtonUp += controller_ButtonUp;
            controller.ButtonPressed += controller_ButtonPressed;
        }

        void controller_ButtonPressed(object sender, Button e)
        {
            SendWiiKeyboardKey(e);
        }

        private void SendWiiKeyboardKey(Button e)
        {
            switch (e)
            {
                case Button.DOWN:
                    WinAPI.ManagedSendKeys("{DOWN}");
                    break;
                case Button.UP:
                    WinAPI.ManagedSendKeys("{UP}");
                    break;
                case Button.LEFT:
                    WinAPI.ManagedSendKeys("{LEFT}");
                    break;
                case Button.RIGHT:
                    WinAPI.ManagedSendKeys("{RIGHT}");
                    break;
                case Button.MINUS:
                    WinAPI.ManagedSendKeys("^-");
                    break;
                case Button.PLUS:
                    WinAPI.ManagedSendKeys("^{+}");
                    break;
                case Button.ONE:
                    WinAPI.ManagedSendKeys("1");
                    break;
                case Button.TWO:
                    WinAPI.ManagedSendKeys("2");
                    break;
            }
        }

        void controller_ButtonUp(object sender, Button e)
        {
            SendWiiKey(e, 1);
        }

        void controller_ButtonDown(object sender, Button e)
        {
            SendWiiKey(e, 0);
        }

        void SendWiiKey(Button e,int state)
        {
            switch (e)
            {
                case Button.A:
                    WinAPI.MouseClick("left",state);
                    break;
                case Button.B:
                    WinAPI.MouseClick("right", state);
                    break;
                case Button.HOME:
                    WinAPI.MouseClick("middle", state);
                    break;
            }
        }

        void controller_InfraredChanged(object sender, List<Point> e)
        {
            Point point = controller.GetAveragePoint();
            SetCursorPos((int)(width - (width * point.X)), (int)(height * point.Y));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            controller.Dispose();
            base.OnExit(e);
        }

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);
    }
}
