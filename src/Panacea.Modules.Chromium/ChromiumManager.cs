﻿using CefSharp;
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
        public ChromiumManager(Dictionary<string, string> args, ILogger logger)
        {
            _logger = logger;
            _args = args;
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
        private readonly Dictionary<string, string> _args;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            Cef.EnableHighDPISupport();
            var pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var chromiumPath = Path.Combine(pluginPath, "Chromium", Environment.Is64BitProcess ? "x64" : "x86");
            var settings = new CefSettings
            {
                CachePath = Path.Combine(pluginPath, "cache"),
                ResourcesDirPath = chromiumPath,

                LocalesDirPath = Path.Combine(chromiumPath, "locales"),
                UserDataPath = Path.Combine(pluginPath, "User Data"),
                BrowserSubprocessPath = Path.Combine(chromiumPath, "CefSharp.BrowserSubprocess.exe"),
                MultiThreadedMessageLoop = true,
                WindowlessRenderingEnabled = false,
                PersistSessionCookies = true,
                LogSeverity = LogSeverity.Disable,
                PersistUserPreferences = true
                //FocusedNodeChangedEnabled = true

            };

            CefSharpSettings.FocusedNodeChangedEnabled = true;
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            CefSharpSettings.WcfEnabled = false;
            CefSharpSettings.WcfTimeout = new TimeSpan(0);
            settings.CefCommandLineArgs.Add("--touch-events", "enabled");
            settings.CefCommandLineArgs.Add("--enable-pinch", "");
            settings.CefCommandLineArgs.Add("--ppapi-flash-path", Path.Combine(pluginPath, "PepperFlash", Environment.Is64BitProcess ? "x64" : "x86", "pepflashplayer.dll")); //Load a specific pepper flash version (Step 1 of 2)
            settings.CefCommandLineArgs.Add("--ppapi-flash-version", "32.0.0.238");
            settings.CefCommandLineArgs.Add("--enable-ephemeral-flash-permission", "0");
            settings.CefCommandLineArgs.Add("--plugin-policy", "allow");
            //settings.CefCommandLineArgs.Add("--disable-smart-virtual-keyboard", "1");
            //settings.CefCommandLineArgs.Add("--disable-virtual-keyboard", "1");
            settings.CefCommandLineArgs.Add("--enable-media-stream", "1");
            //settings.CefCommandLineArgs.Add("--ignore-certificate-errors", "1");
            settings.CefCommandLineArgs.Add("enable-experimental-web-platform-features", "1");
            settings.CefCommandLineArgs.Add("disable-web-security", "disable-web-security");

            foreach (var kp in _args)
            {

                if (kp.Value == null)
                {
                    settings.CefCommandLineArgs.Add(kp.Key, "");
                }
                else
                {
                    settings.CefCommandLineArgs.Add(kp.Key, kp.Value);
                }
            }


            Cef.RegisterWidevineCdm(Path.Combine(pluginPath, @"Widevine", Environment.Is64BitProcess ? "x64" : "x86"), new Callback());

            if (!Cef.Initialize(settings))
            {
                throw new Exception("Unable to Initialize Cef");
            }
            //Cef.GetGlobalRequestContext(). set SetStoragePath(Path.Combine(pluginPath, "cookies"), true);

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
