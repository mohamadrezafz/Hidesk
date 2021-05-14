using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hidesk.Shared.Messages
{
    [Serializable]
    public class RemoteDesktopRequest : RequestMessageBase
    {
        public int Quality { get; set; }

        public RemoteDesktopRequest()
        {
            Quality = 30;
        }
    }
}

