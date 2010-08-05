using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace PlainRSS
{
    public partial class FeedManager : Form
    {
        List<Feed> feeds;

        PopupBrowser feedBrowser = null;

        int lastKey = 1;

        string[] newArgs;
        bool needParseArgs = false;

        DateTime lastCheck;

        bool closing = false;

        bool isWaitingForConnection = false;
        bool isWaitingForRead = false;
        byte[] asyncReadData = new byte[1024];
        MemoryStream memStream = null;
        int nBytesRead = 0;
        int nBytesRemaining = 0;

        private delegate void ParseCommandLineDelegate(string[] args);


        public FeedManager(string[] args)
        {
            newArgs = args;
            if(args.Length>0)
                needParseArgs = true;
            InitializeComponent();
        }

        private void AddNewFeed(AddFeed af)
        {
            if (af.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Feed feed = af.Feed;
                    feeds.Add(feed);

                    FeedLoader.AddNewFeed(feed);

                    var node = treeView1.Nodes[0];
                    TreeNode newNode = node.Nodes.Add("#" + lastKey.ToString(), feed.FeedTitle, 1, 1);
                    lastKey++;
                    newNode.Tag = feed;
                    node.Expand();
                    RefreshFeeds();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error adding feed.");
                }
            }
        }

        private void ModifyFeed(Feed feed)
        {
            AddFeed af = new AddFeed(feed);
            if (af.ShowDialog() == DialogResult.OK)
            {
                FeedLoader.RemoveFeed(feed);
                feeds.Remove(feed);

                Feed f = af.Feed;
                feeds.Add(f);
                FeedLoader.AddFeed(feed);
                if ((feedBrowser != null) && (feedBrowser.Visible))
                    feedBrowser.AddFeed(f);
            }
        }

        private void RemoveFeed(Feed feed)
        {
            FeedLoader.RemoveFeed(feed);
            feeds.Remove(feed);
        }

        private void ParseCommandLine(string[] args)
        {
            // TODO
            //MessageBox.Show("New commandline received with " + args.Length.ToString() + " args:" + string.Join(", ", args));
            if (args.Length == 0)
                return;

            int first = 0;
            while (first < args.Length)
            {
                int remaining = args.Length - first;

                int nargs = 1;
                switch (args[first+0])
                {
                    case "-AddFeed":
                        if (remaining < 2)
                        {
                            MessageBox.Show("Invalid number of parameters in command.", "PlainRSS IPC");
                        }
                        else
                            AddNewFeed(new AddFeed(args[first+1]));
                        nargs = 2;
                        break;
                    case "-ShowBrowser":
                        Show();
                        break;
                    case "-HideBrowser":
                        Hide();
                        break;
                    case "-Popup":
                        TogglePopup();
                        break;
                    case "-Exit":
                        Close();
                        break;
                }
                first += nargs;
            }
        }

        private void ReadMore_Callback(IAsyncResult res)
        {
            try
            {
                Program.IPCServer.EndRead(res);
            }
            catch (Exception)
            {
                try
                {
                    isWaitingForConnection = true;
                    isWaitingForRead = false;

                    Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                }
                catch (Exception)
                {
                }
            }

            if (res.IsCompleted)
            {
                memStream.Write(asyncReadData, 0, nBytesRead);

                if (nBytesRemaining == 0)
                {
                    BinaryReader memStreamReader = new BinaryReader(memStream);
                    List<string> args = new List<string>();

                    memStream.Seek(0, SeekOrigin.Begin);

                    int nParams = memStreamReader.ReadInt32();
                    for (int i = 0; i < nParams; i++)
                        args.Add(memStreamReader.ReadString());

                    this.Invoke(new ParseCommandLineDelegate(ParseCommandLine), (object)(args.ToArray()));

                    Program.IPCServer.Disconnect();

                    isWaitingForConnection = true;
                    isWaitingForRead = false;

                    Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                }
                else
                {
                    nBytesRead = nBytesRemaining;
                    if (nBytesRead > 1024)
                    {
                        nBytesRemaining = nBytesRead - 1024;
                        nBytesRead = 1024;
                    }
                    else nBytesRemaining = 0;

                    Program.IPCServer.BeginRead(asyncReadData, 0, nBytesRead, new AsyncCallback(ReadMore_Callback), this);
                }
            }
            else
            {
                try
                {
                    Program.IPCServer.Disconnect();
                }
                catch (Exception)
                {
                }
                finally
                {
                    isWaitingForConnection = true;
                    isWaitingForRead = false;

                    Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                }
                // ELSE!!!!!!!!!!!!!
            }
        }

        private void ReadBegin_Callback(IAsyncResult res)
        {
            try
            {
                Program.IPCServer.EndRead(res);
            }
            catch (Exception)
            {
                try
                {
                    isWaitingForConnection = true;
                    isWaitingForRead = false;

                    Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                }
                catch (Exception)
                {
                }
            }

            if (res.IsCompleted)
            {
                nBytesRead = BitConverter.ToInt32(asyncReadData, 0);

                memStream = new MemoryStream(nBytesRead);

                if (nBytesRead > 1024)
                {
                    nBytesRemaining = nBytesRead - 1024;
                    nBytesRead = 1024;
                }
                else nBytesRemaining = 0;

                Program.IPCServer.BeginRead(asyncReadData, 0, nBytesRead, new AsyncCallback(ReadMore_Callback), this);
            }
            else
            {
                try
                {
                    Program.IPCServer.Disconnect();
                }
                catch (Exception)
                {
                }
                finally
                {
                    try
                    {
                        isWaitingForConnection = true;
                        isWaitingForRead = false;

                        Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                    }
                    catch (Exception)
                    {
                    }
                }
                // ELSE!!!!!!!!!!!!!
            }
        }

        private void WaitForConnection_Callback(IAsyncResult res)
        {
            try
            {
                Program.IPCServer.EndWaitForConnection(res);
            }
            catch (Exception)
            {
                return;
            }

            if (res.IsCompleted)
            {

                isWaitingForConnection = false;
                isWaitingForRead = true;

                Program.IPCServer.BeginRead(asyncReadData, 0, 4, new AsyncCallback(ReadBegin_Callback), this);
            }
            else
            {
                try
                {
                    Program.IPCServer.Disconnect();
                }
                catch (Exception)
                {
                }
                finally
                {
                    isWaitingForConnection = true;
                    isWaitingForRead = false;

                    Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
                }
                // ELSE!!!!!!!!!!!!!
            }
        }

        private void FeedManager_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.feed_magnify;
            notifyIcon1.Visible = true;

            isWaitingForConnection = true;
            Program.IPCServer.BeginWaitForConnection(new AsyncCallback(WaitForConnection_Callback), this);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TogglePopup();
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.Visible = !this.Visible;
            }
        }

        private void TogglePopup()
        {
            if ((feedBrowser != null) && (feedBrowser.Visible))
            {
                feedBrowser.Close();
                feedBrowser = null;
            }
            else
            {
                feedBrowser = new PopupBrowser(feeds);
                feedBrowser.Show(this);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closing=true;
            Close();
        }

        private void FeedManager_Shown(object sender, EventArgs e)
        {
            if (needParseArgs)
                ParseCommandLine(newArgs);
            else
                Hide();

            feeds = FeedLoader.LoadFeeds();
            var node = treeView1.Nodes[0];
            foreach (Feed feed in feeds)
            {
                TreeNode newNode = node.Nodes.Add("#" + lastKey.ToString(), feed.FeedTitle, 1, 1);
                lastKey++;
                newNode.Tag = feed;
            }
            node.Expand();

            RefreshFeeds();
        }

        private void showManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
        }

        private void showPopupStackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TogglePopup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddNewFeed(new AddFeed());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if((sel != null) && (sel.Tag != null) && (sel.Tag is Feed))
            {
                ModifyFeed(sel.Tag as Feed);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if ((sel != null) && (sel.Tag != null) && (sel.Tag is Feed))
            {
                RemoveFeed(sel.Tag as Feed);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RefreshFeeds();
        }

        private void RefreshFeeds()
        {
            notifyIcon1.Icon = Properties.Resources.feed_magnify;
            backgroundWorker1.RunWorkerAsync();
            lastCheck = DateTime.Now;
            timer1.Enabled = true;
        }

        private void FeedManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing && !closing)
            {
                e.Cancel = true;
                return;
            }

            closing = true;

            Program.IPCServer.Close();
            Program.IPCServer.Dispose();
            Program.IPCServer = null;

            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();

            foreach(Feed feed in feeds)
            {
                FeedLoader.SaveFeedItems(feed);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastCheck).TotalMinutes >= 30)
            {
                RefreshFeeds();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (Feed f in feeds)
            {
                f.Refresh();
                if (backgroundWorker1.CancellationPending)
                    return;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int newItems = 0;
            foreach (Feed f in feeds)
            {
                FeedLoader.SaveFeedItems(f);

                newItems += f.CountVisibleNonDisplayedItems();
            }
            if ((newItems > 0))
            {
                notifyIcon1.Icon = Properties.Resources.feed_go;
                if ((feedBrowser == null) || (feedBrowser.Visible == false))
                {
                    feedBrowser = new PopupBrowser(feeds);
                    feedBrowser.Show(this);
                }
                else feedBrowser.RefreshFeeds();
            }
            else notifyIcon1.Icon = Properties.Resources.feed;
        }
    }
}