using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Chromium
{
    class CefForm:Form
    {
        public CefForm(ChromiumWebBrowser br)
        {
            _br = br;
            _br.IsBrowserInitializedChanged += _br_IsBrowserInitializedChanged;
        }


        bool _initialized;
        private void _br_IsBrowserInitializedChanged(object sender, CefSharp.IsBrowserInitializedChangedEventArgs e)
        {
            _initialized = e.IsBrowserInitialized;
        }

        public static short HIWORD(int a)
        {
            return ((short)(a >> 16));
        }

        public static short LOWORD(int a)
        {
            return ((short)(a & 0xffff));
        }

        const int WM_PARENTNOTIFY = 0x210;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_XBUTTONDOWN = 0x020B;
        const int WM_POINTERDOWN = 0x0246;
        private ChromiumWebBrowser _br;

        //sends a WM_KILLFOCUS to the old control, activates the new window if necessary and sends a WM_SETFOCUS to the new control.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        //returns the handle of the currently focused control on the thread.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            bool handled = false;
            m.Result = IntPtr.Zero;
            if (_br != null && _initialized)
            {
                var _browserWindowHandle = _br.GetBrowserHost().GetWindowHandle();
                if (m.Msg == WM_PARENTNOTIFY)
                {
                    //https://docs.microsoft.com/en-us/previous-versions/windows/desktop/inputmsg/wm-parentnotify
                    //WM_PARENTNOTIFY
                    //Return value
                    //If the application processes this message, it returns zero.
                    //If the application does not process this message, it calls DefWindowProc.
                    if (_browserWindowHandle != IntPtr.Zero)
                    {
                        int loWord = LOWORD((int)m.WParam);
                        switch (loWord)
                        {
                            case WM_LBUTTONDOWN:
                            case WM_MBUTTONDOWN:
                            case WM_RBUTTONDOWN:
                            case WM_XBUTTONDOWN:
                            case WM_POINTERDOWN:
                                handled = true;
                                IntPtr focusedControl = GetFocus();
                                if (focusedControl != _browserWindowHandle)
                                {
                                    SetFocus(_browserWindowHandle);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (!handled)
            {
                base.WndProc(ref m);
            }

        }
    }
}
