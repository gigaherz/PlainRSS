using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlainRSS
{
    public class FeedItemCollection : ICollection<FeedItem>
    {
        List<FeedItem> internalList;

        public FeedItem this[int index]
        {
            get { return internalList[index]; }
        }

        int ICollection<FeedItem>.Count
        {
            get { return internalList.Count; }
        }

        bool ICollection<FeedItem>.IsReadOnly
        {
            get { return true; }
        }

        internal FeedItemCollection(List<FeedItem> list)
        {
            internalList = list;
        }

        void ICollection<FeedItem>.Add(FeedItem item)
        {
            throw new NotSupportedException("This collection is read only.");
        }

        bool ICollection<FeedItem>.Remove(FeedItem item)
        {
            throw new NotSupportedException("This collection is read only.");
        }

        void ICollection<FeedItem>.Clear()
        {
            throw new NotSupportedException("This collection is read only.");
        }

        bool ICollection<FeedItem>.Contains(FeedItem item)
        {
            foreach(FeedItem _item in internalList)
            {
                if (item.Equals(_item))
                    return true;
            }
            return false;
        }

        IEnumerator<FeedItem> IEnumerable<FeedItem>.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        public void CopyTo(FeedItem[] array)
        {
            internalList.CopyTo(array);
        }

        public void CopyTo(FeedItem[] array, int arrayIndex)
        {
            internalList.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, FeedItem[] array, int arrayIndex, int count)
        {
            internalList.CopyTo(index, array, arrayIndex, count);
        }
    }
}
