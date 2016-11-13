using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Based on http://stackoverflow.com/questions/5646820/logger-wrapper-best-practice
/// </summary>
namespace Telemetry
{
    internal static class Constants
    {
        internal const string DEFAULT_MESSAGE = "";
        internal const object[] DEFAULT_DATUM = null;
        internal const Exception DEFAULT_EXCEPTION = null;
    }

    public enum SeverityType
    {
        None = 0,
        Critical = TraceEventType.Critical,
        Error = TraceEventType.Error,
        Information = TraceEventType.Information,
        Verbose = TraceEventType.Verbose,
        Warning = TraceEventType.Warning,
    }

    public interface ILogger : IDisposable
    {
        void Log(LogEntry entry);

        ILogger CreateScope();

        ILogger CreateScope(string activityName, int activityId);
    }

    public class LogEntry
    {
        internal readonly SeverityType Severity;
        internal readonly string Message = Constants.DEFAULT_MESSAGE;
        internal readonly object[] Datum = Constants.DEFAULT_DATUM;
        internal readonly Exception Exception = Constants.DEFAULT_EXCEPTION;

        public LogEntry(
            SeverityType severity,
            string message = Constants.DEFAULT_MESSAGE,
            object[] datum = Constants.DEFAULT_DATUM,
            Exception exception = Constants.DEFAULT_EXCEPTION
        )
        {
            this.Severity = severity;
            this.Message = message;
            this.Datum = datum;
            this.Exception = exception;
        }

        public object Arguments
        {
            get
            {
                if (IsData() && Datum.Length == 1)
                    return Datum[0];
                else
                    return null;
            }
        }

        public bool IsData()
        {
            return
                (String.IsNullOrEmpty(Message) || Exception == Constants.DEFAULT_EXCEPTION)
                && Datum != Constants.DEFAULT_DATUM
                && Datum.Length >= 1;
        }
    }

    public static class ILoggerExtensions
    {
        public static void Critical(
            this ILogger logger,
            Exception exception,
            string message = Constants.DEFAULT_MESSAGE,
            params object[] args
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Length > 0)
                throw new ArgumentException("Message cannot be empty", nameof(message));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            logger.Log(
                new LogEntry(
                    SeverityType.Critical,
                    message,
                    args,
                    exception
                )
            );
        }

        public static void Critical(
            this ILogger logger,
            Exception exception
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    SeverityType.Critical,
                    exception: exception
                )
            );
        }

        public static void Critical(
            this ILogger logger,
            Exception exception,
            object[] datum
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    SeverityType.Critical,
                    datum: datum
                )
            );
        }

        public static void Error(
            this ILogger logger,
            Exception exception,
            string message = "",
            params object[] args
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    SeverityType.Error,
                    message,
                    args,
                    exception
                )
            );
        }

        public static void Error(
            this ILogger logger,
            Exception exception
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    SeverityType.Error,
                    exception: exception
                )
            );
        }

        public static void Error(
            this ILogger logger,
            Exception exception,
            object[] datum
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    SeverityType.Error,
                    datum: datum,
                    exception: exception
                )
            );
        }

        public static void Information(
            this ILogger logger,
            string message = Constants.DEFAULT_MESSAGE,
            params object[] args
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            logger.Log(
                new LogEntry(
                    SeverityType.Information,
                    message,
                    args
                )
            );
        }

        public static void Information(
            this ILogger logger,
            object[] datum
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            logger.Log(
                new LogEntry(
                    SeverityType.Information,
                    datum: datum
                )
            );
        }

        public static void Verbose(
            this ILogger logger,
            string message = Constants.DEFAULT_MESSAGE,
            params object[] args
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message), "Message cannot be empty");

            logger.Log(
                new LogEntry(
                    SeverityType.Verbose,
                    message,
                    args
                )
            );
        }

        public static void Verbose(
            this ILogger logger,
            object[] datum
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            logger.Log(
                new LogEntry(
                    SeverityType.Verbose,
                    datum: datum
                )
            );
        }

        public static void Warning(
            this ILogger logger,
            string message = Constants.DEFAULT_MESSAGE,
            params object[] args
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message), "Message cannot be null");

            logger.Log(
                new LogEntry(
                    SeverityType.Warning,
                    message,
                    args
                )
            );
        }

        public static void Warning(
            this ILogger logger,
            object[] datum
        )
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            logger.Log(
                new LogEntry(
                    SeverityType.Warning,
                    datum: datum
                )
            );
        }
    }
}