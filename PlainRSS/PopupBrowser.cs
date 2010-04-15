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
    public partial class PopupBrowser : Form, IStackParent
    {
        const int MaxOnScreen = 10;

        List<PopupItem> popupList = new List<PopupItem>();

        Dictionary<FeedItem, PopupItem> popupItems = new Dictionary<FeedItem, PopupItem>();

        List<Feed> feeds;

        List<FeedItem> visibleItems;
        int firstShown = 0;

        bool refreshing = false;

        public event EventHandler<EventArgs> OnClose;
        public event EventHandler<RepositionEventArgs> OnReposition;

        public PopupBrowser(List<Feed> feedList)
        {
            feeds = feedList;
            InitializeComponent();
        }

        private void Stack_OnClose(object sender, EventArgs args)
        {
            PopupItem stackItem = (PopupItem)sender;
            IStackParent stackParent = stackItem.StackParent;
            foreach(PopupItem f in popupList)
            {
                if(f.StackParent == stackItem)
                {
                    f.Reparent(stackParent);
                }
            }
            popupList.Remove(stackItem);
            popupItems.Remove(stackItem.ItemData);
            visibleItems.Remove(stackItem.ItemData);

            RefreshVisibleItems();
        }

        private void RefreshVisibleItems()
        {
            if (refreshing)
                return;

            refreshing = true;

            visibleItems.Sort(new Comparison<FeedItem>(ItemCompareByDate));

            int min = Math.Max(0, Math.Min(firstShown, visibleItems.Count - MaxOnScreen));
            int max = Math.Min(firstShown + MaxOnScreen, visibleItems.Count);

            firstShown = min;

            HashSet<PopupItem> addedItems = new HashSet<PopupItem>();

            List<PopupItem> oldItems = new List<PopupItem>(popupList);

            popupList.Clear();

            Form stackParent = this;
            for (int i = min; i < max; i++)
            {
                PopupItem item = MakeVisible(visibleItems[i], stackParent);
                popupList.Add(item);
                addedItems.Add(item);
                stackParent = item;
            }

            foreach (PopupItem item in oldItems)
            {
                if (!addedItems.Contains(item))
                    item.Hide();
            }

            hScrollBar1.Minimum = 0;
            hScrollBar1.Maximum = visibleItems.Count;
            hScrollBar1.SmallChange = 1;
            hScrollBar1.LargeChange = MaxOnScreen;
            hScrollBar1.Value = firstShown;

            min = firstShown + 1;
            max = Math.Min(firstShown + MaxOnScreen, visibleItems.Count);

            if (visibleItems.Count == 0)
            {
                min = 0;
                max = 0;
            }

            label1.Text = min.ToString() + " - " + max.ToString() + " / " + visibleItems.Count.ToString();

            if (OnReposition != null)
                OnReposition.Invoke(this, RepositionEventArgs.Chained);

            refreshing = false;
        }

        private PopupItem MakeVisible(FeedItem feedItem, Form parent)
        {
            if(popupItems.ContainsKey(feedItem))
            {
                PopupItem item = popupItems[feedItem];

                item.Reparent(parent as IStackParent);
                if(!item.Visible)
                    item.Show(this);

                return item;
            }
            else
            {
                PopupItem newItem = new PopupItem(parent as IStackParent, feedItem);
                newItem.OnClose += new EventHandler<EventArgs>(Stack_OnClose);
                newItem.Show(this);
                popupItems[feedItem] = newItem;
                return newItem;
            }
        }

        private int ItemCompareByDate(FeedItem a, FeedItem b)
        {
            if (a.Date > b.Date)
                return 1;
            if (a.Date < b.Date)
                return -1;
            return 0;
        }

        public void RefreshFeeds()
        {
            visibleItems = new List<FeedItem>();

            foreach(PopupItem item in popupList)
            {
                item.Close(false);
            }
            popupList.Clear();
            popupItems.Clear();

            foreach(Feed feed in feeds)
            {
                foreach(FeedItem item in feed.Items)
                {
                    if(!item.Hidden)
                        visibleItems.Add(item);
                }
            }

            RefreshVisibleItems();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            popupList.Clear();
            Close();
        }

        private void PopupBrowser_Load(object sender, EventArgs e)
        {
            var bounds = Screen.GetWorkingArea(this);
            Location = new Point(bounds.Width - Width - 8,
                bounds.Height - Height - 8);
            RefreshFeeds();
        }

        internal void AddFeed(Feed feed)
        {
            feeds.Add(feed);
            RefreshFeeds();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            firstShown = Math.Max(0, visibleItems.Count - MaxOnScreen - 1);
            RefreshVisibleItems();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            firstShown = 0;
            RefreshVisibleItems();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            firstShown = Math.Max(0, firstShown - MaxOnScreen);
            RefreshVisibleItems();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            firstShown = Math.Min(visibleItems.Count - MaxOnScreen - 1, firstShown + MaxOnScreen);
            RefreshVisibleItems();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            firstShown = hScrollBar1.Value;
            RefreshVisibleItems();
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            firstShown = hScrollBar1.Value;
            RefreshVisibleItems();
        }

        private void PopupBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (OnClose != null)
                OnClose.Invoke(this, EventArgs.Empty);
        }
    }
}
