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
    public partial class AddFeed : Form
    {
        Feed feed = null;

        public Feed Feed
        {
            get { return feed; }
        }

        public AddFeed()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        public AddFeed(string feedSource)
            : this()
        {
            textBox1.Text = feedSource;
        }

        public AddFeed(Feed oldFeed)
            : this()
        {
            Text = "Edit Feed";
            feed = oldFeed;
            textBox1.Text = feed.FeedSource.ToString();
            comboBox1.SelectedIndex = (int)feed.FeedType;
            textBox3.Text = feed.FeedTitle;
            textBox2.Text = feed.LastModified.ToString();
            progressBar1.Value = 100;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            try
            {
                feed = new Feed(new Uri(textBox1.Text));
                textBox1.Text = feed.FeedSource.ToString();
                comboBox1.SelectedIndex = (int)feed.FeedType;
                textBox3.Text = feed.FeedTitle;
                textBox2.Text = feed.LastModified.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading feed.");
            }
            if(feed.Items.Count() == 0)
            {
                MessageBox.Show("Invalid feed.");
            }
            progressBar1.Value = 100;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (feed == null)
                {
                    feed = new Feed(new Uri(textBox1.Text), textBox3.Text, (FeedType)comboBox1.SelectedIndex);
                }
                else
                {
                    if ((textBox1.Text != feed.FeedSource.ToString()) ||
                        (comboBox1.SelectedIndex != (int)feed.FeedType) ||
                        (textBox3.Text != feed.FeedTitle))
                    {
                        feed = new Feed(new Uri(textBox1.Text), textBox3.Text, (FeedType)comboBox1.SelectedIndex);
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Error loading feed.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddFeed_Load(object sender, EventArgs e)
        {
        }

        private void AddFeed_Shown(object sender, EventArgs e)
        {
            if((feed == null) && !string.IsNullOrEmpty(textBox1.Text))
            {
                button1_Click(this, EventArgs.Empty);
            }
        }
    }
}
