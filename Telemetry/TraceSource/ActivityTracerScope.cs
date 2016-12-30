using System;
using System.Diagnostics;

namespace Telemetry.TraceSource
{
    /// <summary>
    /// Base on http://www.codeproject.com/Articles/185666/ActivityTracerScope-Easy-activity-tracing-with
    /// </summary>
    public class ActivityTracerScope : ILogger
    {
        /// <summary>
        /// Flag to indicate if instance is disposed
        /// </summary>
        private bool _Disposed = false;

        /// <summary>
        /// Previous ID before TransferTrace
        /// </summary>
        protected Guid PreviousId { get; set; }

        protected string PreviousActivityName { get; set; }

        protected int PreviousActivityID { get; set; }

        /// <summary>
        /// ID for the current TransferTrace
        /// </summary>
        protected Guid CurrentId { get; set; }

        /// <summary>
        /// Current TraceSource
        /// </summary>
        public System.Diagnostics.TraceSource TraceSource { get; protected set; }

        /// <summary>
        /// User-defined ID for the activity
        /// </summary>
        public int ActivityId { get; protected set; }

        /// <summary>
        /// User-defined name for the activity
        /// </summary>
        public string ActivityName { get; protected set; }

        /// <summary>
        /// </summary>
        /// <param name="traceSource">TraceSource to use to log activity</param>
        /// <param name="activityName">User-defined name for the current activity</param>
        /// <param name="activityId">User-defined id for the current activity</param>
        public ActivityTracerScope(
            System.Diagnostics.TraceSource traceSource,
            string activityName = "",
            int activityId = 0
        )
        {
            TraceSource = traceSource;
            ActivityId = activityId;
            ActivityName = activityName;

            // create a new ID for the current activity; we would need this
            // when we when call TraceEvent with TraceEventType.Stop
            CurrentId = Guid.NewGuid();

            // remember the previous activity ID so we could come back to it
            // later when we switch back to the previous activity before this
            PreviousId = Trace.CorrelationManager.ActivityId;

            // transfer to a new activity and then start the trace event
            if (PreviousId != Guid.Empty)
                TraceSource.TraceTransfer(ActivityId, $"Transferring to new activity", CurrentId);

            TraceSource.TraceEvent(TraceEventType.Start, ActivityId, ActivityName);

            Trace.CorrelationManager.ActivityId = CurrentId;
        }

        /// <summary>
        /// Creates a new TraceSource instead of using a user-defined one.
        /// </summary>
        /// <param name="traceName">Name for the soon-to-be created TraceSource to use to log activity</param>
        /// <param name="activityName">User-defined name for the current activity</param>
        /// <param name="activityId">User-defined ID for the current activity</param>
        public ActivityTracerScope(
            string traceName,
            string activityName = "",
            int activityId = 0
        ) : this(new System.Diagnostics.TraceSource(traceName), activityName, activityId)
        {
        }

        ~ActivityTracerScope()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                TraceSource.TraceEvent(TraceEventType.Stop, ActivityId, ActivityName);

                // transfer back to the previous activity
                if (PreviousId != Guid.Empty)
                    TraceSource.TraceTransfer(ActivityId, $"Transferring back to previous activity", PreviousId);

                Trace.CorrelationManager.ActivityId = PreviousId;

                _Disposed = true;
            }
        }

        /// <summary>
        /// Transfer to the previous activity and then stop the current trace event
        /// </summary>
        public void Dispose()
        {   
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Log activity
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry), "Log entry cannot be null");

            if (entry.Exception != null)
            {
                TraceSource
                    .TraceData(
                        (TraceEventType) entry.Severity,
                        ActivityId,
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
                        ActivityId, 
                        entry.Datum
                    );
            }
            else
            {
                TraceSource
                    .TraceEvent(
                        (TraceEventType) entry.Severity,
                        ActivityId,
                        entry.Message,
                        entry.Datum
                    );
            }
        }

        /// <summary>
        /// Create a new instance with a blank activity name.  The new instance could be enclosed inside a using statement to start an an activity.
        /// </summary>
        public ILogger CreateScope()
        {
            return CreateScope("", 0);
        }

        /// <summary>
        /// Create a new instance.  The new instance could be enclosed inside a using statement to start an an activity.
        /// </summary>
        /// <param name="activityName">Name of the activity</param>
        /// <returns></returns>
        public ILogger CreateScope(string activityName)
        {
            return new ActivityTracerScope(this.TraceSource, activityName, 0);
        }

        /// <summary>
        /// Create a new instance.  The new instance could be enclosed inside a using statement to start an an activity.
        /// </summary>
        /// <param name="activityName">Name of the activity</param>
        /// <param name="activityId">Optional activity ID</param>
        /// <returns></returns>
        public ILogger CreateScope(string activityName, int activityId)
        {
            return new ActivityTracerScope(this.TraceSource, activityName, activityId);
        }
    }
}