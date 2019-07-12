using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Panacea.Modules.Chromium
{
    public class CefWindowsFormsHost : WindowsFormsHost
    {
        static CefWindowsFormsHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CefWindowsFormsHost), new FrameworkPropertyMetadata(typeof(CefWindowsFormsHost)));
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            WmTouchDevice.MessageTouchDevice.WndProc(System.Windows.Window.GetWindow(this), msg, wParam, lParam, ref handled);
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
