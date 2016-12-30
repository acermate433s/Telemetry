using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace Telemetry.Proxy
{
    public class MessageEventArgs : EventArgs, IMessage
    {
        IMessage _Message;

        public MessageEventArgs(IMessage message)
        {
            _Message = message;
        }

        public IDictionary Properties
        {
            get
            {
                return _Message.Properties;
            }
        }

        public Exception Exception { get; set; }
    }
}
