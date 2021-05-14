using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hidesk.Shared.Messages
{
    [Serializable]
    public class MouseCursorResponse  : ResponseMessageBase
    {
        public MouseCursorResponse(RequestMessageBase request) : base(request)
        {
            DeleteCallbackAfterInvoke = false;
        }

        public Point CursorPosition { get; set; }
    }
}
