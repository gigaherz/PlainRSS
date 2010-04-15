using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace PlainRSS
{
    public class FeedItem
    {
        string itemId;
        Uri link;
        string title;
        string summary;
        DateTime date;

        bool visited = false;

        bool hidden = false;

        bool displayed = false;

        Feed source;

        public string ItemId
        {
            get { return itemId; }
        }

        public Uri Link
        {
            get { return link; }
        }

        public string Title
        {
            get { return title; }
        }

        public string Summary
        {
            get { return summary; }
        }

        public DateTime Date
        {
            get { return date; }
        }

        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }

        public bool Hidden
        {
            get { return hidden; }
            set { hidden = value; }
        }

        public bool Displayed
        {
            get { return displayed; }
            set { displayed = value; }
        }

        public Feed Source
        {
            get { return source; }
            set { source = value; }
        }

        internal FeedItem(string _itemId, Uri _link, string _title, string _summary, DateTime _date, Feed _src)
        {
            itemId = _itemId;
            link = _link;
            title = _title;
            summary = _summary;
            date = _date;
            source = _src;
        }

        internal static FeedItem FromRssItem(Rss.RssItem item, Feed _src)
        {
            string itemId = item.Guid.Name;
            Uri link = item.Link;
            string title = item.Title;
            string summary = item.Description;
            DateTime date = item.PubDate;
            return new FeedItem(itemId, link, title, summary, date, _src);
        }

        internal static FeedItem FromAtomEntry(Atom.Core.AtomEntry entry, Feed _src)
        {
            string itemId = entry.Id.ToString();
            Uri link = entry.Links[0].HRef;
            string title = entry.Title.Content;
            string summary = entry.Summary.Content;
            DateTime date = entry.Modified.DateTime;
            return new FeedItem(itemId, link, title, summary, date, _src);
        }

        internal static FeedItem FromDataRow(DataRow row, Feed _src)
        {
            string itemId = row.Field<string>(0);
            Uri link = new Uri(row.Field<string>(2));
            string title = row.Field<string>(3);
            string summary = row.Field<string>(4);
            DateTime date = row.Field<DateTime>(5);
            return new FeedItem(itemId, link, title, summary, date, _src);
        }
    }
}
