using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Panacea.Modules.Chromium
{
    [DataContract]
    public class GetSettingsResponse
    {
        [DataMember(Name = "chromiumFlags")]
        public string ChromiumFlags { get; set; }
    }
}
