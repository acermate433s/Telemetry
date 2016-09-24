using System;
using System.Diagnostics;

namespace Telemetry
{
    /// <summary>
    /// Base on http://www.codeproject.com/Articles/185666/ActivityTracerScope-Easy-activity-tracing-with
    /// </summary>
    public class ActivityTracerScope : ILogger
    {
        /// <summary>
        /// Previous ID before TransferTrace
        /// </summary>
        protected Guid PreviousID { get; set; }

        /// <summary>
        /// ID for the current TransferTrace
        /// </summary>
        protected Guid CurrentID { get; set; }

        /// <summary>
        /// Current TraceSource
        /// </summary>
        public TraceSource TraceSource { get; protected set; }

        /// <summary>
        /// User-defined ID for the activity
        /// </summary>
        public int ActivityID { get; protected set; }

        /// <summary>
        /// User-defined name for the activity
        /// </summary>
        public string ActivityName { get; protected set; }

        /// <summary>
        /// </summary>
        /// <param name="traceSource">TraceSource to use to log activity</param>
        /// <param name="activityName">User-defined name for the current activity</param>
        /// <param name="activityID">User-defined id for the current activity</param>
        public ActivityTracerScope(
            TraceSource traceSource,
            string activityName = "",
            int activityID = 0
        )
        {
            TraceSource = traceSource;
            ActivityID = activityID;
            ActivityName = activityName;

            // remember the previous activity ID so we could come back to it
            // later when we switch back to the previous activity before this
            PreviousID = Trace.CorrelationManager.ActivityId;

            // create a new ID for the current activity; we would need this
            // when we when call TraceEvent with TraceEventType.Stop
            CurrentID = Guid.NewGuid();

            // transfer to a new activity and then start the trace event
            if (PreviousID != Guid.Empty)
                TraceSource.TraceTransfer(ActivityID, "Transferring to new activity", CurrentID);
            Trace.CorrelationManager.ActivityId = CurrentID;
            TraceSource.TraceEvent(TraceEventType.Start, ActivityID, ActivityName);
        }

        /// <summary>
        /// Creates a new TraceSource instead of using a user-defined one.
        /// </summary>
        /// <param name="traceName">Name for the soon-to-be created TraceSource to use to log activity</param>
        /// <param name="activityName">User-defined name for the current activity</param>
        /// <param name="activityID">User-defined id for the current activity</param>
        public ActivityTracerScope(
            string traceName,
            string activityName = "",
            int activityID = 0
        ) : this(new TraceSource(traceName), activityName, activityID)
        {
        }

        /// <summary>
        /// Transfer to the previous activity and then stop the current trace event
        /// </summary>
        public void Dispose()
        {
            if (PreviousID != Guid.Empty)
                TraceSource.TraceTransfer(ActivityID, "Transferring back to previous activity", PreviousID);
            TraceSource.TraceEvent(TraceEventType.Stop, ActivityID, ActivityName);
            Trace.CorrelationManager.ActivityId = PreviousID;
        }

        /// <summary>
        /// Log activity
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry.Exception != null)
            {
                TraceSource
                    .TraceData(
                        (TraceEventType) entry.Severity,
                        ActivityID,
                        new[]
                        {
                            entry.Datum == null ? entry.Message : String.Format(entry.Message, entry.Datum),
                            entry.Exception.GetType().FullName,
                            entry.Exception.Message,
                            entry.Exception.Source,
                            entry.Exception.StackTrace,
                        }
                    );
            }
            else if (entry.IsData())
            {
                TraceSource
                    .TraceData(
                        (TraceEventType) entry.Severity, 
                        ActivityID, 
                        entry.Datum
                    );
            }
            else
            {
                TraceSource
                    .TraceEvent(
                        (TraceEventType) entry.Severity,
                        ActivityID,
                        entry.Message,
                        entry.Datum
                    );
            }
        }

        /// <summary>
        /// Create a new instance.  The new instance could be enclosed inside a using statement to start an an activity.
        /// </summary>
        /// <param name="activityName">Name of the activity</param>
        /// <param name="activityID">Optional activity ID</param>
        /// <returns></returns>
        public ILogger CreateScope(string activityName = "", int activityID = 0)
        {
            return new ActivityTracerScope(this.TraceSource, activityName, activityID);
        }
    }
}