﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Panacea.Modules.Chromium
{
    public class ControlSnapshot
    {
        public static Bitmap Snapshot(Control c, double width = 800, double height = 600)
        {

            IntPtr hwnd = IntPtr.Zero;
            IntPtr dc = IntPtr.Zero;
            c.Invoke(new MethodInvoker(() => {
                hwnd = c.Handle;
                dc = GetDC(hwnd);
            }));
            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppRgb);
            if (dc != IntPtr.Zero)
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr bdc = g.GetHdc();
                        try
                        {
                            BitBlt(bdc, 0, 0, (int)width, (int)height, dc, 0, 0, TernaryRasterOperations.SRCCOPY);
                        }
                        finally
                        {
                            g.ReleaseHdc(bdc);
                        }
                    }
                }
                finally
                {
                    ReleaseDC(hwnd, dc);
                }
            }
            return bmp;
        }

        public static Bitmap Snapshot(IntPtr handle, double width = 800, double height = 600)
        {

            IntPtr hwnd = handle;
            IntPtr dc = IntPtr.Zero;

            dc = GetDC(hwnd);

            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppRgb);
            if (dc != IntPtr.Zero)
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr bdc = g.GetHdc();
                        try
                        {
                            BitBlt(bdc, 0, 0, (int)width, (int)height, dc, 0, 0, TernaryRasterOperations.SRCCOPY);
                        }
                        finally
                        {
                            g.ReleaseHdc(bdc);
                        }
                    }
                }
                finally
                {
                    ReleaseDC(hwnd, dc);
                }
            }
            return bmp;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }
    }
}
