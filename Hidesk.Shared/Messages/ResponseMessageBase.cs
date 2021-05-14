using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hidesk.Shared.Messages
{
    [Serializable]
    public class ResponseMessageBase : MessageBase
    {
        public bool DeleteCallbackAfterInvoke { get; set; }

        public ResponseMessageBase(RequestMessageBase request)
        {
            DeleteCallbackAfterInvoke = true;
            CallbackID = request.CallbackID;
        }
    }
}
