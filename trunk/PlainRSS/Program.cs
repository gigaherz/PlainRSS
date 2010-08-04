using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.IO.Pipes;
using System.Net;

namespace PlainRSS
{
    static class Program
    {
        public static string DataFolder;
        public static NamedPipeServerStream IPCServer;
        public static FeedManager MainWindow;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 1 && args[0].Substring(0,7) == "feed://") // special case for adding feeds from the browser
            {
                try
                {
                    Uri url = new Uri(args[0]);
                    args = new string[] { "-ShowBrowser", "-AddFeed", "http://" + args[0].Substring(7) };
                }
                catch (Exception)
                {
                }
            }

            try
            {
                DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlainRSS");
                if (!File.Exists(DataFolder))
                {
                    Directory.CreateDirectory(DataFolder);
                }
            }
            catch (IOException)
            {
                DataFolder = Application.StartupPath;
            }

            try
            {
                IPCServer = new NamedPipeServerStream("PlainRSS.IPC", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                MainWindow = new FeedManager(args);
                Application.Run(MainWindow);
            }
            catch (IOException)
            {
                if (args.Length == 0)
                    return;

                MemoryStream ms = new MemoryStream();
                BinaryWriter wr = new BinaryWriter(ms);
                wr.Write(args.Length);
                foreach (string arg in args)
                    wr.Write(arg);
                wr.Flush();
                byte[] contents = ms.ToArray();

                NamedPipeClientStream cli = new NamedPipeClientStream(".", "PlainRSS.IPC", PipeDirection.Out, PipeOptions.None);
                cli.Connect();
                cli.Write(BitConverter.GetBytes(contents.Length), 0, 4);
                cli.Write(contents, 0, contents.Length);
                cli.Flush();
                cli.WaitForPipeDrain();
                cli.Close();
            }
        }
    }
}
