using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;

namespace PlainRSS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // get the name of our process
            Process proc = Process.GetCurrentProcess();
            // get the list of all processes by that name
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);
            // if there is more than one process...
            if (processes.Length > 1)
            {
                foreach(Process p in processes)
                {
                    if(p.Id != proc.Id)
                    {
                        MessageHelper hlp = new MessageHelper();
                        if (hlp.SendMessageToHandle(p.MainWindowHandle, MessageHelper.WM_USER + 1336, 0, args.Length) == 1337)
                        {

                            for (int i = 0; i < args.Length; i++)
                                hlp.SendStringMessageToHandle(p.MainWindowHandle, i, args[i]);
                            hlp.SendMessageToHandle(p.MainWindowHandle, MessageHelper.WM_USER + 1337, 0, args.Length);
                        }
                    }
                }
                MessageBox.Show("Another instance running. Exiting.");
                return;
            }

            Application.Run(new FeedManager(args));
        }
    }
}
