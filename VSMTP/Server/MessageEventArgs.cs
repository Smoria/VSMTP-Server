using System;

namespace VSMTP
{
    public class MessageEventArgs : EventArgs
    {
        public SessionParameters Parameters { get; set; }

        public MessageEventArgs(SessionParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
