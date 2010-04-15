using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        public FeedManager(string[] args)
        {
            newArgs = args;
            if(args.Length>0)
                needParseArgs = true;
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MessageHelper.WM_USER+1336:
                    newArgs = new string[(int)m.LParam];
                    m.Result = (IntPtr)1337;
                    break;
                case MessageHelper.WM_USER + 1337:
                    ParseCommandLine(newArgs);
                    break;
                case MessageHelper.WM_COPYDATA:
                    MessageHelper.COPYDATASTRUCT mystr = new MessageHelper.COPYDATASTRUCT();
                    Type mytype = mystr.GetType();
                    mystr = (MessageHelper.COPYDATASTRUCT)m.GetLParam(mytype);
                    newArgs[(int)mystr.dwData] = mystr.lpData;
                    break;
            }

            base.WndProc(ref m);
        }

        private void ParseCommandLine(string[] args)
        {
            // TODO
            //MessageBox.Show("New commandline received with " + args.Length.ToString() + " args:" + string.Join(", ", args));
            AddFeed af = new AddFeed(args[0]);
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
                }
                catch (Exception)
                {
                    MessageBox.Show("Error adding feed.");
                }
            }

        }

        private void FeedManager_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.feed_magnify;
            notifyIcon1.Visible = true;
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
            AddFeed af = new AddFeed();
            if(af.ShowDialog() == DialogResult.OK)
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
                }
                catch(Exception)
                {
                    MessageBox.Show("Error adding feed.");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if((sel != null) && (sel.Tag != null) && (sel.Tag is Feed))
            {
                AddFeed af = new AddFeed(sel.Tag as Feed);
                if (af.ShowDialog() == DialogResult.OK)
                {
                    Feed f = af.Feed;
                    feeds.Add(f);
                    if ((feedBrowser != null) && (feedBrowser.Visible))
                        feedBrowser.AddFeed(f);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

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