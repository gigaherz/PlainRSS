using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace PlainRSS
{
    class FeedLoader
    {
        public static List<Feed> LoadFeeds()
        {
            List<Feed> feeds = new List<Feed>();

            try
            {
                XDocument doc = XDocument.Load("feeds.xml");
                var query = from c in doc.Elements("feedList").Elements("feed") select c;

                foreach (var item in query)
                {
                    Uri Source = new Uri(item.Element("source").Value);
                    string Title = item.Element("title").Value;
                    FeedType Type = (FeedType)int.Parse(item.Element("type").Value);
                    DateTime Modified = DateTime.Parse(item.Element("date").Value);
                    Feed feed = new Feed(Source, Title, Type, Modified, false);
                    feed.SetCachedItems(LoadFeedItems(feed));
                    feeds.Add(feed);

                }
            }
            catch (Exception)
            {

            }

            return feeds;
        }

        public static bool AddNewFeed(Feed feed)
        {
            try
            {
                Uri Source = feed.FeedSource;
                string Title = feed.FeedTitle;
                FeedType Type = feed.FeedType;
                DateTime Modified = feed.LastModified;

                XDocument doc;
                try
                {
                    doc = XDocument.Load("feeds.xml");
                }
                catch (Exception)
                {
                    doc = new XDocument(new XElement(XName.Get("feedList")));
                }

                var query = from c in doc.Elements("feedList").Elements("feed")
                            where c.Element("source").Value == Source.ToString()
                            select c;

                XElement element = new XElement(XName.Get("feed"),
                    new XElement(XName.Get("source"), Source.ToString()),
                    new XElement(XName.Get("title"), Title),
                    new XElement(XName.Get("type"), ((int)Type).ToString()),
                    new XElement(XName.Get("date"), Modified.ToString("yyyy-MM-dd HH:mm:ss")));

                if (query.Count() > 0)
                    query.ElementAt(0).ReplaceWith(element);
                else
                    doc.Element("feedList").Add(element);

                doc.Save("feeds.xml");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<FeedItem> LoadFeedItems(Feed feed)
        {
            List<FeedItem> feedItems = new List<FeedItem>();

            try
            {
                XDocument doc = XDocument.Load("itemCache.xml");

                var source = from c in doc.Elements("itemCache").Elements("feed")
                             where c.Attribute("source").Value == feed.FeedSource.ToString()
                             select c;

                // assume there's only one item
                if (source.Count() == 0)
                    return feedItems; // empty
                //Debug.Assert(source.Count() == 1);

                XElement element = source.First();

                var query = from c in element.Elements("item")
                            select c;

                foreach (var item in query)
                {
                    string Id = item.Element("id").Value;
                    string Title = item.Element("title").Value;
                    Uri Link = new Uri(item.Element("link").Value);
                    string Summary = item.Element("summary").Value;

                    bool IsVisited = false;
                    bool IsHidden = false;
                    bool IsDisplayed = false;

                    if (!bool.TryParse(item.Element("visited").Value, out IsVisited))
                        IsVisited=false;

                    if (!bool.TryParse(item.Element("hidden").Value, out IsHidden))
                        IsHidden = false;

                    if (!bool.TryParse(item.Element("displayed").Value, out IsDisplayed))
                        IsDisplayed = false;

                    string dateString = item.Element("date").Value;
                    DateTime Modified = DateTime.Parse(dateString);
                    FeedItem feedItem = new FeedItem(Id, Link, Title, Summary, Modified, feed);
                    feedItem.Visited = IsVisited;
                    feedItem.Hidden = IsHidden;
                    feedItem.Displayed = IsDisplayed;
                    feedItems.Add(feedItem);
                }
            }
            catch (Exception)
            {

            }

            return feedItems;
        }

        public static bool SaveFeedItems(Feed feed)
        {
            try
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load("itemCache.xml");
                }
                catch (Exception)
                {
                    doc = new XDocument(new XElement(XName.Get("itemCache")));
                }

                var source = from c in doc.Elements("itemCache").Elements("feed")
                             where c.Attribute("source").Value == feed.FeedSource.ToString()
                             select c;

                XElement element;
                if(source.Count()==1)
                {
                    element = source.First();
                    element.Elements().Remove(); // leave attributes
                }
                else
                {
                    element = new XElement(XName.Get("feed"), new XAttribute("source", feed.FeedSource.ToString()));
                    doc.Element("itemCache").Add(element);
                }

                foreach(FeedItem item in feed.Items)
                {
                    XElement xitem = new XElement(XName.Get("item"),
                        new XElement(XName.Get("id"), item.ItemId),
                        new XElement(XName.Get("title"), item.Title),
                        new XElement(XName.Get("link"), item.Link.ToString()),
                        new XElement(XName.Get("summary"), item.Summary),
                        new XElement(XName.Get("visited"), item.Visited),
                        new XElement(XName.Get("hidden"), item.Hidden),
                        new XElement(XName.Get("displayed"), item.Displayed),
                        new XElement(XName.Get("date"), item.Date.ToString("yyyy-MM-dd HH:mm:ss")));
                    element.Add(xitem);
                }

                doc.Save("itemCache.xml");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
