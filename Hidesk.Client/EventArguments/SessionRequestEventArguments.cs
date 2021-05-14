using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidesk.Shared.Messages;

namespace Hidesk.Client.EventArguments
{
    public class SessionRequestEventArguments
    {
        public SessionRequest Request { get; set; }
        private Action ConfirmAction;
        private Action RefuseAction;

        public SessionRequestEventArguments(Action confirmAction, Action refuseAction)
        {
            ConfirmAction = confirmAction;
            RefuseAction = refuseAction;
        }

        public void Confirm()
        {
            ConfirmAction();
        }

        public void Refuse()
        {
            RefuseAction();
        }
    }
}
