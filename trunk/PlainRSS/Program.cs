using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.IO.Pipes;

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
