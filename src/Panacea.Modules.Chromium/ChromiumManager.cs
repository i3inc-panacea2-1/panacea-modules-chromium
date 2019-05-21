using CefSharp;
using CefSharp.WinForms;
using Panacea.Modularity.WebBrowsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Chromium
{
    public class ChromiumManager : IWebViewManager
    {
        public Task ClearCookies()
        {
            Cef.GetGlobalCookieManager().VisitAllCookies(new CookieVisitor());
            return Task.CompletedTask;
        }

        public IWebView CreateTab(string url = null)
        {
            Initialize();
            if (url == null) url = "about:blank";
            return new ChromiumWebView("https://google.com");
        }

        bool _initialized;
        void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            Cef.EnableHighDPISupport();
            var pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                Directory.Delete(Path.Combine(pluginPath, "cache"), true);
                Directory.CreateDirectory(Path.Combine(pluginPath, "cache"));
            }
            catch { }
            var settings = new CefSettings
            {
                CachePath = Path.Combine(pluginPath, "cache"),
                ResourcesDirPath = pluginPath,
                LocalesDirPath = Path.Combine(pluginPath, "locales"),
                UserDataPath = Path.Combine(pluginPath, "User Data"),
                BrowserSubprocessPath = Path.Combine(pluginPath, "CefSharp.BrowserSubprocess.exe"),
                MultiThreadedMessageLoop = true,
                WindowlessRenderingEnabled = false,
                PersistSessionCookies = true,
                LogSeverity = LogSeverity.Disable,

                //FocusedNodeChangedEnabled = true

            };

            CefSharpSettings.FocusedNodeChangedEnabled = true;
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            //CefSharpSettings.WcfTimeout = new TimeSpan(0,0,0,0,50);
            //settings.CefCommandLineArgs.Add("touch-events", "enabled");
            //settings.CefCommandLineArgs.Add("enable-pinch", "");

            //settings.CefCommandLineArgs.Add("--enable-npapi", "1");
            settings.CefCommandLineArgs.Add("--ppapi-flash-path", Path.Combine(pluginPath, "PepperFlash", "pepflashplayer.dll")); //Load a specific pepper flash version (Step 1 of 2)
            settings.CefCommandLineArgs.Add("--ppapi-flash-version", "32.0.0.142");
            settings.CefCommandLineArgs.Add("--enable-ephemeral-flash-permission", "0");
            settings.CefCommandLineArgs.Add("--plugin-policy", "allow");
            settings.CefCommandLineArgs.Add("--disable-smart-virtual-keyboard", "1");
            settings.CefCommandLineArgs.Add("--disable-virtual-keyboard", "1");
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            settings.CefCommandLineArgs.Add("--ignore-certificate-errors", "1");
            //settings.CefCommandLineArgs.Add("--kiosk", "1");
            //settings.CefCommandLineArgs.Add("--kiosk-noprint", "1");
            //settings.FocusedNodeChangedEnabled = true;

            //settings.WindowlessFrameRate = 60;
            // Disable Surfaces so internal PDF viewer works for OSR
            // https://bitbucket.org/chromiumembedded/cef/issues/1689
            //settings.CefCommandLineArgs.Add("disable-surfaces", "1");

            //settings.SetOffScreenRenderingBestPerformanceArgs();

            Cef.RegisterWidevineCdm(pluginPath + @"Widevine", new Callback());
            if (!Cef.Initialize(settings))
            {
                throw new Exception("Unable to Initialize Cef");
            }

        }
    }

    public class CookieVisitor : ICookieVisitor
    {
        public void Dispose()
        {

        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = true;
            return true;
        }
    }

    class Callback : IRegisterCdmCallback
    {
        bool _disposed;
        public bool IsDisposed => _disposed;

        public void Dispose()
        {
            _disposed = true;
        }

        public void OnRegistrationComplete(CdmRegistration registration)
        {
            System.Diagnostics.Debug.WriteLine(registration.ErrorCode + " - " + registration.ErrorMessage);
        }
    }
}
