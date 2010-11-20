using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Atom;
using Atom.Core;
using Rss;

namespace PlainRSS
{
    public enum FeedType
    {
        Unknown,
        Rss,
        Atom
    }

    public class Feed
    {
        static uint sessionUid = 0;

        FeedType feedType = FeedType.Unknown;

        AtomFeed atomFeed = null;
        RssFeed rssFeed = null;

        DateTime lastModified = DateTime.MinValue;

        Uri feedSource = null;

        FeedItemCollection items = new FeedItemCollection(new List<FeedItem>());

        string feedTitle;
        bool customTitle = false;

        uint instanceUid = sessionUid++;

        TimeSpan updateInterval = new TimeSpan(0,30,0);
        DateTime lastUpdated;

        public DateTime LastUpdated
        {
            get { return lastUpdated; }
            set { lastUpdated = value; }
        }

        public TimeSpan UpdateInterval
        {
            get { return updateInterval; }
            set { updateInterval = value; }
        }

        public uint InstanceUid
        {
            get { return instanceUid; }
        }

        public string FeedTitle
        {
            get { return feedTitle; }
            internal set { feedTitle = value; customTitle = true; }
        }

        public FeedType FeedType
        {
            get { return feedType; }
        }

        public DateTime LastModified
        {
            get { return lastModified; }
        }

        public Uri FeedSource
        {
            get { return feedSource; }
        }

        internal FeedItemCollection Items
        {
            get { return items; }
        }

        internal Feed()
        {
        }

        public Feed(Uri source)
        {
            feedSource = source;
            Refresh();
        }

        public Feed(Uri source, string savedTitle, FeedType savedType, DateTime savedTime, bool refresh)
        {
            feedSource = source;
            feedTitle = savedTitle;
            feedType = savedType;
            lastModified = savedTime;
            if(refresh)
                Refresh();
        }

        public Feed(Uri source, string savedTitle, FeedType savedType)
        {
            feedSource = source;
            feedTitle = savedTitle;
            feedType = savedType;
            Refresh();
        }

        private bool UpdateRssFeed()
        {
            if (rssFeed == null) // query mode
                rssFeed = RssFeed.Read(feedSource.ToString());
            else // refresh mode
                rssFeed = RssFeed.Read(rssFeed);

            if (rssFeed.Channels.Count > 0)
            {
                if(feedTitle=="" || !customTitle)
                    feedTitle = rssFeed.Channels[0].Title;

                lastModified = rssFeed.LastModified;

                feedType = FeedType.Rss;

                return true;
            }

            return false;
        }

        private bool UpdateAtomFeed()
        {
            atomFeed = AtomFeed.Load(feedSource);

            if (feedTitle == "" || !customTitle)
                feedTitle = atomFeed.Title.Content;

            if (atomFeed.Modified != null)
                lastModified = atomFeed.Modified.DateTime;
            else if (atomFeed.Entries.Count > 0)
            {
                // get newest modified entry
                foreach (AtomEntry entry in atomFeed.Entries)
                {
                    DateTime entryDate = DateTime.MinValue;
                    if (entry.Modified != null)
                        entryDate = entry.Modified.DateTime;
                    else if (entry.Issued != null)
                        entryDate = entry.Issued.DateTime;
                    else if (entry.Created != null)
                        entryDate = entry.Created.DateTime;

                    if (entryDate > lastModified)
                        lastModified = entryDate;
                }
            }

            feedType = FeedType.Atom;
            return true;
        }

        public void Refresh()
        {
            DateTime prevModified = lastModified;

            switch(feedType)
            {
                case FeedType.Unknown:
                    try
                    {
                        if (UpdateRssFeed())
                            break;
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        UpdateAtomFeed();
                        break;
                    }
                    catch (Exception)
                    {

                    }
                    break;

                case FeedType.Rss:
                    try
                    {
                        if (UpdateRssFeed())
                            break;
                    }
                    catch (Exception)
                    {

                    }
                    break;

                case FeedType.Atom:
                    try
                    {
                        UpdateAtomFeed();
                        break;
                    }
                    catch (Exception)
                    {

                    }
                    break;

            }

            if (lastModified != prevModified)
            {
                List<FeedItem> newItems = new List<FeedItem>();

                foreach (FeedItem item in items)
                {
                    newItems.Add(item);
                }

                List<FeedItem> feedItems = GetFeedItems();
                foreach (FeedItem item in feedItems)
                {
                    bool found = false;
                    foreach(FeedItem prev in newItems)
                    {
                        if(prev.ItemId == item.ItemId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        newItems.Add(item);
                    }
                }

                items = new FeedItemCollection(newItems);
            }
        }

        private List<RssItem> GetRssItems()
        {
            List<RssItem> rssItems = new List<RssItem>();
            foreach(RssChannel channel in rssFeed.Channels)
            {
                foreach(RssItem item in channel.Items)
                {
                    rssItems.Add(item);
                }
            }
            return rssItems;
        }

        private List<AtomEntry> GetAtomEntries()
        {
            List<AtomEntry> entries = new List<AtomEntry>();
            foreach(AtomEntry entry in atomFeed.Entries)
            {
                entries.Add(entry);
            }
            return entries;
        }

        private List<FeedItem> GetFeedItems()
        {
            List<FeedItem> feedItems = new List<FeedItem>();
            if(feedType == FeedType.Rss)
            {
                var rssItems = GetRssItems();
                foreach(RssItem item in rssItems)
                {
                    feedItems.Add(FeedItem.FromRssItem(item,this));
                }
            }
            else if (feedType == FeedType.Atom)
            {
                var atomEntries = GetAtomEntries();
                foreach (AtomEntry entry in atomEntries)
                {
                    feedItems.Add(FeedItem.FromAtomEntry(entry,this));
                }
            }
            return feedItems;
        }

        internal int CountVisibleNonDisplayedItems()
        {
            int counted = 0;
            foreach (FeedItem item in items)
            {
                if(!item.Hidden && !item.Displayed)
                    counted++;
            }
            return counted;
        }

        internal void SetCachedItems(List<FeedItem> list)
        {
            items = new FeedItemCollection(list);
        }
    }
}
