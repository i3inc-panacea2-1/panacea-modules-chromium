using Panacea.Core;
using Panacea.Modularity.WebBrowsing;
using System;
using System.Collections.Generic;
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
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }

        public Task EndInit()
        {
            return Task.CompletedTask;
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
