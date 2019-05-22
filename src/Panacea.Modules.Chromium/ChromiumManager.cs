using CefSharp;
using CefSharp.WinForms;
using Panacea.Core;
using Panacea.Modularity.WebBrowsing;
using ServiceStack.Text;
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
        public ChromiumManager(ILogger logger)
        {
            _logger = logger;
        }

        public Task ClearCookies()
        {
            Cef.GetGlobalCookieManager().VisitAllCookies(new CookieVisitor());
            return Task.CompletedTask;
        }

        public IWebView CreateTab(string url = null)
        {
            Initialize();
            if (url == null) url = "about:blank";
            return new ChromiumWebView(url, _logger);
        }

        bool _initialized;
        private readonly ILogger _logger;

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
            CefSharpSettings.WcfEnabled = false;
            CefSharpSettings.WcfTimeout = new TimeSpan(0);
            settings.CefCommandLineArgs.Add("--touch-events", "enabled");
            settings.CefCommandLineArgs.Add("--enable-pinch", "");
            settings.CefCommandLineArgs.Add("--ppapi-flash-path", Path.Combine(pluginPath, "PepperFlash", "pepflashplayer.dll")); //Load a specific pepper flash version (Step 1 of 2)
            settings.CefCommandLineArgs.Add("--ppapi-flash-version", "32.0.0.142");
            settings.CefCommandLineArgs.Add("--enable-ephemeral-flash-permission", "0");
            settings.CefCommandLineArgs.Add("--plugin-policy", "allow");
            settings.CefCommandLineArgs.Add("--disable-smart-virtual-keyboard", "1");
            settings.CefCommandLineArgs.Add("--disable-virtual-keyboard", "1");
            settings.CefCommandLineArgs.Add("--enable-media-stream", "1");
            settings.CefCommandLineArgs.Add("disable-gpu", "disable-gpu");

            using (var reader = new StreamReader(Path.Combine(new DirectoryInfo(pluginPath).FullName, "settings.json")))
            {
                var lst = JsonSerializer.DeserializeFromString<List<string>>(reader.ReadToEnd());
                foreach (var line in lst)
                {
                    var parts = line.Split('@');
                    if (parts.Length == 1)
                    {
                        settings.CefCommandLineArgs.Add(line, "");
                    }
                    else if (parts.Length == 2)
                    {
                        settings.CefCommandLineArgs.Add(parts[0], parts[1]);
                    }
                }
            }

            Cef.RegisterWidevineCdm(Path.Combine(pluginPath, @"Widevine"), new Callback());
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
