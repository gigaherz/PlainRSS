using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlainRSS
{

    public class RepositionEventArgs : EventArgs
    {
        bool chained;

        public bool IsChained
        {
            get { return chained; }
            set { chained = value; }
        }

        public RepositionEventArgs(bool chain)
        {
            chained = chain;
        }

        public static RepositionEventArgs Chained
        {
            get { return new RepositionEventArgs(true); }
        }

        public static RepositionEventArgs NonChained
        {
            get { return new RepositionEventArgs(false); }
        }

    }

    public interface IStackParent
    {

        event EventHandler<EventArgs> OnClose;
        event EventHandler<RepositionEventArgs> OnReposition;

    }
}
