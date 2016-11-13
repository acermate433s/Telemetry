using System;
using System.ComponentModel;
using System.Diagnostics;

[assembly: CLSCompliant(false)]
namespace Telemetry.TraceSource
{
    /// <summary>
    /// Base on http://www.codeproject.com/Articles/185666/ActivityTracerScope-Easy-activity-tracing-with
    /// </summary>
    public class ActivityTracerScope<TActivityEnumType> : ActivityTracerScope where TActivityEnumType : struct, IConvertible, IComparable, IFormattable
    {
        private readonly TActivityEnumType _Activity;

        public TActivityEnumType Activity {
            get
            {
                return _Activity;
            }
        }

        /// <summary>
        /// Activity ID
        /// </summary>
        /// <remarks>
        /// Integer representation of the enumeration
        /// </remarks>
        public new int ActivityId
        {
            get
            {
                return Activity.ToInt32(null);
            }
        }

        /// <summary>
        /// Activity name
        /// </summary>
        /// <remarks>
        /// String represenation of the enumeration.  If there is a DescriptionAttribute it would be used, otherwise ToString() would be used.
        /// </remarks>
        public new string ActivityName
        {
            get
            {
                return GetDescription(Activity);
            }
        }

        /// <summary>
        /// Gets the string representation of the enumeration.  Uses the value of the DescriptionAttribute if there is one, otherwise ToString() would be used.
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        private static string GetDescription(
            TActivityEnumType activityType
        )
        {
            // default would be the ToString() method of the ActivityEnum
            // if we cannot find a DescriptionAttribute
            var result = activityType.ToString();

            // get the DescriptionAttribute of the ActivityEnum and
            // use the Description property as the return value
            var fieldInfo = activityType.GetType().GetField(result);
            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0 && attributes[0] is DescriptionAttribute)
            {
                var description = (DescriptionAttribute) attributes[0];
                result = description.Description;
            }

            return result;
        }

        /// <summary>
        /// Makes sure that ActivityEnumType is an enumeration type
        /// </summary>
        internal ActivityTracerScope() : base(String.Empty)
        {
            if (!typeof(TActivityEnumType).IsEnum)
                throw new ArgumentException("TActivityEnumType must be an enumerated type");
        }

        /// <summary>
        /// </summary>
        /// <param name="traceSource">TraceSource to use to log activity</param>
        /// <param name="activityType">Activity type of the current activity</param>
        public ActivityTracerScope(
            System.Diagnostics.TraceSource traceSource,
            TActivityEnumType activityType = default(TActivityEnumType)
        ) : base(
                traceSource,
                GetDescription(activityType),
                activityType.ToInt32(null)
        )
        {
            _Activity = activityType;
        }

        /// <summary>
        /// Creates a new TraceSource instead of using a user-defined one.
        /// </summary>
        /// <param name="traceName">Activity name of the current activity</param>
        /// <param name="activityType">Activity type of the current activity</param>
        public ActivityTracerScope(
            string traceName,
            TActivityEnumType activityType = default(TActivityEnumType)
        ) : this(new System.Diagnostics.TraceSource(traceName), activityType)
        {
        }
    }
}