using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace PlainRSS
{
    public partial class PopupItem : Form, IStackParent
    {
        PopupBrowser browser;
        IStackParent stackParent;
        FeedItem itemData;

        public FeedItem ItemData
        {
            get { return itemData; }
        }

        public event EventHandler<EventArgs> OnClose;
        public event EventHandler<RepositionEventArgs> OnReposition;

        double tempOpacity = 1;
        DateTime lastOp;

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public IStackParent StackParent
        {
            get { return stackParent; }
            set 
            {
                Reparent(value);
            }
        }

        private void Parent_OnReposition(object sender, RepositionEventArgs args)
        {
            Location = new Point((stackParent as Form).Location.X,
                                (stackParent as Form).Location.Y - Height - 8);

            if (args.IsChained && (OnReposition != null))
                OnReposition.Invoke(this, RepositionEventArgs.Chained);
        }

        public PopupItem(PopupBrowser brows, IStackParent parent, FeedItem item)
        {
            browser = brows;

            //DwmStuff.MARGINS margins = new DwmStuff.MARGINS();
            //margins.Left = 20;
            //margins.Top = 20;
            //margins.Right = 20;
            //margins.Bottom = 20;
            //AeroMargins = margins;

            stackParent = parent;
            stackParent.OnReposition += new EventHandler<RepositionEventArgs>(Parent_OnReposition);
            itemData = item;
            InitializeComponent();
            try {
                AllowTransparency = true;
            }
            catch(Exception)
            {

            }
        }

        public void Reparent(IStackParent newParent)
        {
            (stackParent).OnReposition -= new EventHandler<RepositionEventArgs>(Parent_OnReposition);
            stackParent = newParent;
            (stackParent).OnReposition += new EventHandler<RepositionEventArgs>(Parent_OnReposition);
            Parent_OnReposition(stackParent, RepositionEventArgs.NonChained);
        }

        private void PopupItem_Load(object sender, EventArgs e)
        {
            linkLabel1.Text = itemData.Title;
            linkLabel1.LinkVisited = itemData.Visited;
            label1.Text = itemData.Source.FeedTitle;
            if(!itemData.Displayed)
            {
                BackColor = Color.LemonChiffon;
                itemData.Displayed = true;
            }
            Parent_OnReposition(stackParent, RepositionEventArgs.NonChained);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo(itemData.Link.ToString());
            info.UseShellExecute = true;
            Process.Start(info);
            linkLabel1.LinkVisited = true;
            itemData.Visited = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            itemData.Hidden = true;
            //lastOp = DateTime.Now;
            //timer1.Enabled = true;
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(AllowTransparency)
            {
                DateTime curOp = DateTime.Now;
                TimeSpan diff = curOp - lastOp;
                lastOp = curOp;
                
                tempOpacity -= 4 * diff.TotalSeconds;
                if (tempOpacity < 0.01)
                {
                    timer1.Enabled = false;
                    Close();
                }
                else Opacity = tempOpacity;
            }
            else
            {
                timer1.Enabled = false;
                Close();
            }
        }

        private void PopupItem_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) && (OnClose != null))
                OnClose.Invoke(this, EventArgs.Empty);
        }

        public void Close(bool causesOnClose)
        {
            if(!causesOnClose)
                OnClose = null;
            Close();
        }


        #region AlphaLayering
        /// <para>Changes the current bitmap.</para>
        public void SetBitmap(Bitmap bitmap)
        {
            SetBitmap(bitmap, 255);
        }

        /// <para>Changes the current bitmap with a custom opacity level.  Here is where all happens!</para>
        public void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

            // The ideia of this is very simple,
            // 1. Create a compatible DC with screen;
            // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
            // 3. Call the UpdateLayeredWindow.

            IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
            IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                oldBitmap = Win32.SelectObject(memDc, hBitmap);

                Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                Win32.Point pointSource = new Win32.Point(0, 0);
                Win32.Point topPos = new Win32.Point(Left, Top);
                Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                blend.BlendOp = Win32.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = opacity;
                blend.AlphaFormat = Win32.AC_SRC_ALPHA;

                Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
            }
            finally
            {
                Win32.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBitmap);
                    //Windows.DeleteObject(hBitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                    Win32.DeleteObject(hBitmap);
                }
                Win32.DeleteDC(memDc);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                return cp;
            }
        }
        #endregion

        private void PopupItem_Paint(object sender, PaintEventArgs e)
        {
            //SetBitmap(Properties.Resources.skin1);
        }

        private void PopupItem_MouseHover(object sender, EventArgs e)
        {
            browser.ShowPreview(this);
        }

        private void PopupItem_MouseLeave(object sender, EventArgs e)
        {
            browser.HidePreview();
        }
    }
}
