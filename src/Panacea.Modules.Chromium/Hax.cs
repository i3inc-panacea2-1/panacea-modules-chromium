using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Panacea.Modules.Chromium
{
    static class Hax
    {
        public static void FixTouch(ChromiumWebView element)
        {
            if (element.LastWindow == null) return;
            Win32Point p = new Win32Point();
            GetCursorPos(ref p);
            SetCursorPos((int)element.LastWindow.Left + (int)element.LastWindow.ActualWidth / 2, (int)element.LastWindow.Top + (int)element.LastWindow.ActualHeight / 2);
            DoSomerandomMouseMovements(8);
            SetCursorPos(p.X, p.Y);
        }

        static void DoSomerandomMouseMovements(int times)
        {
            var rnd = new Random();
            int range = 5;
            for (var i = 0; i < times; i++)
            {
                
                mouse_event(MOUSEEVENTF_MOVE, rnd.Next(-1 * range, range), rnd.Next(-1 * range, range), 0, UIntPtr.Zero);
               
                Thread.Sleep(60);
                
                
            }
        }

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_MOVE = 0x0001;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);


        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };

        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

    }
}
