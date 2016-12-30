using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemetry.Proxy.Attributes
{
    /// <summary>
    /// Attribute to start activity
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public class ActivityAttribute : Attribute
    {
        public ActivityAttribute()
        {
            Name = "Default";
            Id = 0;
        }

        public ActivityAttribute(
            string name,
            int id
        )
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        /// <value>
        /// The name of the activity.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the activity.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public int Id { get; set; }
    }
}
