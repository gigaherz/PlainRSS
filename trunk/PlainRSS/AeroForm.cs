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
    public partial class AeroForm : Form
    {
        DwmStuff.MARGINS aeroMargins = DwmStuff.MARGINS.Zero;

        public DwmStuff.MARGINS AeroMargins
        {
            get { return aeroMargins; }
            set { aeroMargins = value; }
        }

        public AeroForm()
        {
        }

        // uses PInvoke to setup the Glass area.

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            base.OnLoad(e);
            DwmStuff.ExtendFormMargins((Form)this, aeroMargins);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (DesignMode) return;
            base.OnPaint(e);
            if (DwmStuff.IsCompositionEnabled)
            {
                // paint background black to enable include glass regions

                e.Graphics.Clear(Color.Black);
                // revert the non-glass rectangle back to it's original colour

                Rectangle clientArea = new Rectangle(
                        aeroMargins.Left,
                        aeroMargins.Top,
                        this.ClientRectangle.Width - aeroMargins.Left - aeroMargins.Right,
                        this.ClientRectangle.Height - aeroMargins.Top - aeroMargins.Bottom
                    );
                Brush b = new SolidBrush(this.BackColor);
                e.Graphics.FillRectangle(b, clientArea);
            }
        }

    }
}
