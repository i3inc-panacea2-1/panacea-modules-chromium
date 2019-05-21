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
        public Task<IWebViewManager> GetWebViewManagerAsync()
        {
            if(_manager == null)
            {
                _manager = new ChromiumManager();
            }
            return Task.FromResult(_manager);
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
