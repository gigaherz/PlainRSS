using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PlainRSS
{
    public class DwmStuff
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;

            public MARGINS(int l, int t, int r, int b)
            {
                Left = l;
                Right = r;
                Top = t;
                Bottom = b;
            }

            public static MARGINS Zero
            {
                get {
                    return new MARGINS(0, 0, 0, 0);
                }
            }

            public override bool Equals(object obj)
            {
                MARGINS m = (MARGINS)obj;
                return this == m;
            }

            public override int GetHashCode()
            {
                return Left.GetHashCode() + Right.GetHashCode() + Top+GetHashCode() + Bottom.GetHashCode();
            }

            public static bool operator ==(MARGINS a, MARGINS b)
            {
                return ((a.Left == b.Left) && (a.Top == b.Top) && (a.Right == b.Right) && (a.Bottom == b.Bottom));
            }

            public static bool operator !=(MARGINS a, MARGINS b)
            {
                return !(a==b);
            }
        }

        struct DWM_BLURBEHIND {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea
                        (IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmEnableBlurBehindWindow
            (IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        public static bool IsCompositionEnabled
        {
            get { return DwmIsCompositionEnabled();  }
        }

        public static void ExtendFormMargins(Form form, MARGINS margins)
        {
            // defines how far we are extending the Glass margins

            if (margins == MARGINS.Zero)
                return;

            if (DwmIsCompositionEnabled())
            {
                // Paint the glass effect.
                DwmExtendFrameIntoClientArea(form.Handle, ref margins);
            }
        }

        public static void EnableBlurBehind(Form form)
        {
            if (DwmIsCompositionEnabled())
            {

                // Create and populate the blur-behind structure.
                DWM_BLURBEHIND bb = new DWM_BLURBEHIND();

                // Specify blur-behind and blur region.
                bb.dwFlags = 1;
                bb.fEnable = true;
                bb.hRgnBlur = IntPtr.Zero;
                bb.fTransitionOnMaximized = false;

                // Enable blur-behind.
                DwmEnableBlurBehindWindow(form.Handle, ref bb);
            }
        }
    }
}
