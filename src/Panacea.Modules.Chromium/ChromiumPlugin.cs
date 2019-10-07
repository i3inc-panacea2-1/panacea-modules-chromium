using Panacea.Core;
using Panacea.Modularity.WebBrowsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Chromium
{
    public class ChromiumPlugin : IWebViewPlugin
    {
        public ChromiumPlugin(PanaceaServices core)
        {
            _core = core;
        }

        public async Task BeginInit()
        {
            _core.UserService.UserLoggedOut += UserService_UserLoggedOut;
            var resp = await _core.HttpClient.GetObjectAsync<GetSettingsResponse>("web/get_browser_settings/");
            if (resp.Success)
            {
                _settings = resp.Result;
            }
        }

        private async Task UserService_UserLoggedOut(IUser user)
        {
            try
            {
                var manager = await GetWebViewManagerAsync();
                await manager.ClearCookies();
            }
            catch { }
        }

        public void Dispose()
        {

        }

        public async Task EndInit()
        {
            if (!Debugger.IsAttached)
            {
                try
                {
                    var manager = (ChromiumManager)await GetWebViewManagerAsync();
                    manager.Initialize();
                    await manager.ClearCookies();
                }
                catch { }
            }
        }
        IWebViewManager _manager;
        private GetSettingsResponse _settings;
        private readonly PanaceaServices _core;

        public Task<IWebViewManager> GetWebViewManagerAsync()
        {
            if (_manager == null)
            {
                var args = new Dictionary<string, string>();
                if (_settings.ChromiumFlags != null)
                {
                    foreach (var line in _settings.ChromiumFlags
                        .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = line.Split('@');
                        args[parts[0]] = parts.Length > 1 ? parts[1] : null;
                    }
                }
                _manager = new ChromiumManager(args, _core.Logger);
            }
            return Task.FromResult(_manager);
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
