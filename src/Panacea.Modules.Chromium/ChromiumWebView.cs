using CefSharp;
using CefSharp.WinForms;
using Panacea.Core;
using Panacea.Modularity.WebBrowsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Panacea.Modules.Chromium
{

    public class ChromiumWebView : System.Windows.Controls.ContentControl,
        IWebView,
        ILifeSpanHandler,
        IRequestHandler,
        IRenderProcessMessageHandler,
        IJsDialogHandler,
        IDownloadHandler,
        IDisplayHandler,
        IContextMenuHandler,
        IDialogHandler
    {
        public event EventHandler<bool> CanGoBackChanged;
        public event EventHandler<bool> CanGoForwardChanged;
        public event EventHandler<string> TitleChanged;
        public event EventHandler Close;
        public event EventHandler<bool> IsBusyChanged;
        private WindowsFormsHost wfh;
        private readonly ILogger _logger;
        public Window LastWindow { get; private set; }
        public ChromiumWebBrowser Browser { get; set; }
        Form _form;
        public ChromiumWebView(string url, ILogger logger)
        {
            _logger = logger;
            Browser = new ChromiumWebBrowser(url)
            {
                Dock = DockStyle.Fill
            };
            wfh = new WindowsFormsHost();
            _form = new Form() { TopLevel = false, FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false, Dock = DockStyle.Fill };
            _form.Controls.Add(Browser);
            wfh.Child = _form;
            Content = wfh;
            Browser.LifeSpanHandler = this;
            Browser.RequestHandler = this;
            Browser.JsDialogHandler = this;
            Browser.RenderProcessMessageHandler = this;
            Browser.DownloadHandler = this;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Browser.DisplayHandler = this;
            Browser.DialogHandler = this;
            Browser.MenuHandler = this;
            Browser.LoadingStateChanged += ChromiumBrowserHost_LoadingStateChanged;
            Browser.LoadError += Browser_LoadError;
            Browser.FrameLoadStart += Browser_FrameLoadStart;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd1;
            Browser.ConsoleMessage += Browser_ConsoleMessage;
            this.IsVisibleChanged += ChromiumWebView_IsVisibleChanged;

            Unloaded += ChromiumWebView_Unloaded;
            Loaded += ChromiumWebView_Loaded;
            //Browser.Load(url);
            _initialUrl = url;
            if (url == "about:blank") Url = url;
        }

        private void ChromiumWebView_Unloaded(object sender, RoutedEventArgs e)
        {
            LastWindow.PreviewMouseDown -= LastWindow_PreviewMouseDown;
        }

        private void ChromiumWebView_Loaded(object sender, RoutedEventArgs e)
        {
            LastWindow = Window.GetWindow(this);
            LastWindow.PreviewMouseDown -= LastWindow_PreviewMouseDown;
            LastWindow.PreviewMouseDown += LastWindow_PreviewMouseDown;
        }

        private void LastWindow_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Browser.Focus();
        }

        private void ChromiumWebView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Hax.FixTouch(this);
            }
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            switch (e.Level)
            {
                case LogSeverity.Error:
                    _logger.Error(this, $"{e.Message}");
                    break;
                case LogSeverity.Info:
                    _logger.Info(this, $"{e.Message}");
                    break;
                case LogSeverity.Warning:
                    _logger.Warn(this, $"{e.Message}");
                    break;
                default:
                    _logger.Debug(this, $"{e.Message}");
                    break;
            }
        }

        private void Browser_FrameLoadEnd1(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {

                //e.Frame.ExecuteJavaScriptAsync("window.print=function(){}");
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    if (_disposed) return;
                    DocumentLoaded?.Invoke(this, EventArgs.Empty);
                    await Task.Delay(30);
                    Browser.Focus();
                }));

            }
            //Browser.ShowDevTools();
        }

        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            //_logger.Error(this, e.ErrorText);
            if (e.ErrorCode == CefErrorCode.SslProtocolError)
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(1000);
                    this.Navigate(e.FailedUrl);
                }));
                return;
            }
            if (e.ErrorCode == CefErrorCode.Aborted) return;
            if (e.Frame != Browser.GetMainFrame()) return;
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(500);

                CanGoBack = false;
            }));

            Browser.LoadHtml(@"
<!DOCTYPE html>
<head>
	<link href='https://fonts.googleapis.com/css?family=Roboto' rel='stylesheet'>
   <style>
      body {
		  margin: 0px;
		  background-color: white;
		  text-align: center;
		  font-family: 'Roboto';font-size: 22px;
      }
		.top{
			height:100vh;
			background: #f44242;
			color:white;
			font-size: 400%;
			
		}
		.bottom {
			font-size: 200%;
			
			background: #f44242;
		}
		.top span {
			width:100%;
			position:absolute;
			top: calc(50% - 100px);
			left:0;
		}
		.bottom span {
			width:100%;
			position:absolute;
			top: calc(50% + 10px);
			left:0;
			color:white;
		}
      }
   </style>
</head>
<body>
   <div class='top'><span>Aww...</span></div>
   <div class='bottom'><span>...the page failed to load</span></div>
</body>
</html>
");
        }

        bool _disposed;
        public void Dispose()
        {
            //Browser?.Stop();
            //Content = null;

            _disposed = true;

            wfh.Dispose();
            //Browser.Parent.Dispose();
            //Browser.Dispose();
            Console.WriteLine("disposed");

        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Navigated?.Invoke(this, e.Url);
                }));
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                //Browser.ExecuteScriptAsync("window.print = function(){}");
                UpdateBackForward();
            }));
        }

        private void UpdateBackForward()
        {
            CanGoBack = Browser.GetBrowser().CanGoBack;
            CanGoForward = Browser.GetBrowser().CanGoForward;
            CanGoBackChanged?.Invoke(this, CanGoBack);
            CanGoForwardChanged?.Invoke(this, CanGoForward);
        }

        bool _loaded;
        private bool _fullscreen;

        private void ChromiumBrowserHost_Initialized(object sender, EventArgs e)
        {
            if (_loaded) return;
            if (_disposed) return;
            _loaded = true;
            Url = "about:blank";
            UrlChanged?.Invoke(this, "about:blank");
        }

        private void ChromiumBrowserHost_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                UpdateBackForward();
                IsBusy = e.IsLoading;
            }));
        }

        void AttachHandlers(ChromiumWebBrowser c)
        {

        }

        public bool CanGoBack
        {
            get { return (bool)GetValue(CanGoBackProperty); }
            set { SetValue(CanGoBackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanGoBack.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanGoBackProperty =
            DependencyProperty.Register("CanGoBack", typeof(bool), typeof(ChromiumWebView), new PropertyMetadata(false));


        public bool CanGoForward
        {
            get { return (bool)GetValue(CanGoForwardProperty); }
            set { SetValue(CanGoForwardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CangoForward.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanGoForwardProperty =
            DependencyProperty.Register("CanGoForward", typeof(bool), typeof(ChromiumWebView), new PropertyMetadata(false));


        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(ChromiumWebView), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                IsBusyChanged?.Invoke(this, value);
            }
        }

        bool _hasInvalidCertificate;
        public bool HasInvalidCertificate
        {
            get => _hasInvalidCertificate;
            private set
            {
                _hasInvalidCertificate = value;
                HasInvalidCertificateChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<bool> HasInvalidCertificateChanged;

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public bool IsFullscreen
        {
            get
            {
                return _fullscreen;
            }

            set
            {
                _fullscreen = value;
            }
        }

        // Using a DependencyProperty as the backing store for Url.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(ChromiumWebView), new PropertyMetadata(""));
        private readonly string _initialUrl;

        public event EventHandler<string> ElementFocus;
        public event EventHandler ElementLostFocus;
        public event EventHandler<NavigatingEventArgs> Navigating;
        public event EventHandler<string> Navigated;
        public event EventHandler<string> NewWindow;
        public event EventHandler<string> UrlChanged;
        public event EventHandler<bool> FullscreenChanged;
        public event EventHandler DocumentLoaded;

        public BitmapImage CreateThumbnail()
        {
            try
            {

                using (var bmp = ControlSnapshot.Snapshot(_form))
                {
                    var ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Save(@"C:\Users\Giannis\Desktop\chromium.png", System.Drawing.Imaging.ImageFormat.Png);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();
                    return bi;
                }

            }
            catch
            {
                return null;
            }
        }

        public new bool Focus()
        {
            return true;
        }


        public void GoBack()
        {
            Browser.GetBrowser().GoBack();
        }

        public void GoForward()
        {
            Browser.GetBrowser().GoForward();
        }

        public void Navigate(string url)
        {
            Console.WriteLine("Navigating");
            Url = url;
            Browser.Load(url);
        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {

            newBrowser = null;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                NewWindow?.Invoke(this, targetUrl);
            }));
            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                if (Url != _initialUrl && _initialUrl != "about:blank")
                {
                    Navigate(Url);
                }
            }));

        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Close?.Invoke(this, null);
            }));

            return true;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            HasInvalidCertificate = true;
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    //To allow certificate
                    callback.Continue(true);
                    return true;
                }
            }

            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {

        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;


        }

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            System.Windows.MessageBox.Show(status.ToString());
        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }


        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {

            return false;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return null;
        }
        List<string> InputElements = new List<string>()
        {
            "input",
            "textarea"
        };
        List<string> InputTypesExcluded = new List<string>()
        {
            "checkbox",
            "radio",
            "button",
            "submit",
            "color",
            "date",
            "file",
            "range",
            "reset"
        };
        void IRenderProcessMessageHandler.OnFocusedNodeChanged(IWebBrowser browserControl,
            IBrowser browser, IFrame frame, IDomNode node)
        {
            if (_disposed) return;
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (_disposed) return;
                if (node == null) ElementLostFocus?.Invoke(this, null);
                else
                {
                    if (InputElements.Contains(node.TagName.ToLower()))
                    {
                        if (node.HasAttribute("type"))
                        {
                            if (InputTypesExcluded.Any(t => t == node["type"]))
                            {
                                ElementLostFocus?.Invoke(this, null);
                                return;
                            }
                        }
                        await Task.Delay(200);

                        ElementFocus?.Invoke(this, "text");
                    }
                }
            }));
        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (_disposed) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                UpdateBackForward();
            }));
        }

        public void OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public void OnFocusedNodeChanged(IWebBrowser browserControl, IBrowser browser, IFrame frame, IDomNode node)
        {

        }

        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            return false;
        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
        {
            return false;
        }

        public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {

        }

        public void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {

        }

        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }

        public bool OnFileDialog(IWebBrowser browserControl, IBrowser browser, CefFileDialogMode mode, CefFileDialogFlags flags, string title, string defaultFilePath, List<string> acceptFilters, int selectedAcceptFilter, IFileDialogCallback callback)
        {
            return true;
        }

        public void OnAddressChanged(IWebBrowser browserControl, AddressChangedEventArgs addressChangedArgs)
        {
            if (_disposed) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                UpdateBackForward();
                Url = addressChangedArgs.Address;
                UrlChanged?.Invoke(this, Url);
            }));

        }

        public void OnTitleChanged(IWebBrowser browserControl, TitleChangedEventArgs titleChangedArgs)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Title = titleChangedArgs.Title;
                TitleChanged?.Invoke(this, titleChangedArgs.Title);
            }));
        }

        public void OnFaviconUrlChange(IWebBrowser browserControl, IBrowser browser, IList<string> urls)
        {

        }

        public void OnFullscreenModeChange(IWebBrowser browserControl, IBrowser browser, bool fullscreen)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                _fullscreen = fullscreen;
                FullscreenChanged?.Invoke(this, fullscreen);
            }));
        }

        public bool OnTooltipChanged(IWebBrowser browserControl, string text)
        {
            return false;
        }

        public void OnStatusMessage(IWebBrowser browserControl, StatusMessageEventArgs statusMessageArgs)
        {
            if (_disposed) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                UpdateBackForward();
            }));
        }

        public bool OnConsoleMessage(IWebBrowser browserControl, ConsoleMessageEventArgs consoleMessageArgs)
        {
            return false;
        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        public Task ExecuteJavaScriptAsync(string script)
        {
            return Task.CompletedTask;
        }

        public bool OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }

        public void OnContextReleased(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public bool OnAutoResize(IWebBrowser browserControl, IBrowser browser, CefSharp.Structs.Size newSize)
        {
            return false;
        }

        public bool OnTooltipChanged(IWebBrowser browserControl, ref string text)
        {
            return false;
        }

        public bool CanGetCookies(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request)
        {
            return true;
        }

        public bool CanSetCookie(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, Cookie cookie)
        {
            return true;
        }

        public void OnUncaughtException(IWebBrowser browserControl, IBrowser browser, IFrame frame, JavascriptException exception)
        {

        }

        //public bool OnFileDialog(IWebBrowser browserControl, IBrowser browser, CefFileDialogMode mode, CefFileDialogFlags flags, string title, string defaultFilePath, List<string> acceptFilters, int selectedAcceptFilter, IFileDialogCallback callback)
        //{
        //    return false;
        //}

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (_disposed) return false;
            HasInvalidCertificate = false;
            Browser.ExecuteScriptAsync("window.print = function(){}");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Browser.Focus();
                if (_disposed) return;
                UpdateBackForward();
            }), DispatcherPriority.Background);
            if (frame.IsMain)
            {
                return Dispatcher.Invoke(() =>
                {
                    var e = new NavigatingEventArgs() { Url = request.Url };
                    Navigating?.Invoke(this, e);
                    if (e.Cancel)
                    {
                        return true;
                    }
                    return false;
                });
            }
            return false;
        }

        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            return false;
        }

        public void ScrollUp(int lines)
        {

        }
        public void ScrollDown(int lines)
        {
        }

        public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
        {
            
        }
    }
}
