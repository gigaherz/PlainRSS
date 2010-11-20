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
    public partial class PopupPreview : Form
    {
        PopupBrowser browser;

        public PopupPreview(PopupBrowser parent)
        {
            browser = parent;
            InitializeComponent();
        }

        private void PopupPreview_ResizeEnd(object sender, EventArgs e)
        {
            this.Location = browser.Location - this.Size;
        }

        private void PopupPreview_Shown(object sender, EventArgs e)
        {
            this.Location = browser.Location - this.Size;
        }

        internal void ShowPreview(FeedItem feedItem)
        {
            if(webBrowser1.Url != feedItem.Link)
                webBrowser1.Navigate(feedItem.Link);
            Show();
        }
    }
}
