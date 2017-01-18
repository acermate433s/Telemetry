using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemetry.Proxy.Attributes
{
    /// <summary>
    /// Attribute to log user-defined message
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MessageAttribute : Attribute
    {
        public MessageAttribute(
            string message
        )
        {
            Message = message;
        }

        /// <summary>
        /// Gets or sets the user-defined message.
        /// </summary>
        /// <value>
        /// The user-defined message.
        /// </value>
        public string Message { get; set; }
    }
}
