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

        public Task BeginInit()
        {
            _core.UserService.UserLoggedOut += UserService_UserLoggedOut;
            return Task.CompletedTask;
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
                    var manager =(ChromiumManager) await GetWebViewManagerAsync();
                    manager.Initialize();
                    await manager.ClearCookies();
                }
                catch { }
            }
        }
        IWebViewManager _manager;
        private readonly PanaceaServices _core;

        public Task<IWebViewManager> GetWebViewManagerAsync()
        {
            if(_manager == null)
            {
                _manager = new ChromiumManager(_core.Logger);
            }
            return Task.FromResult(_manager);
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
