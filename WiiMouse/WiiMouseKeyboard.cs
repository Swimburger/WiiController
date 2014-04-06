using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WiiLib;

namespace WiiMouseKeyboard
{
    public class WiiMouseKeyboard
    {
       private static void Main(string[] args)
       {
         try
         {
             new WiiMouseKeyWorker().RunWorkerAsync();

            ManualResetEvent close = new ManualResetEvent(false);
            SystemEvents.SessionEnding += (object sender, SessionEndingEventArgs e) =>
            close.Set();

            
            close.WaitOne();
         }
         catch (Exception e)
         {
         }
       }
   }
}
