using System;
using System.Diagnostics;

/// <summary>
/// Based on http://stackoverflow.com/questions/5646820/logger-wrapper-best-practice
/// </summary>
namespace Telemetry
{
    internal class Constants
    {
        internal const string DEFAULT_MESSAGE = "";
        internal const object[] DEFAULT_DATUM = null;
        internal const Exception DEFAULT_EXCEPTION = null;
    }

    public enum Severity
    {
        Critical = TraceEventType.Critical,
        Error = TraceEventType.Error,
        Information = TraceEventType.Information,
        Verbose = TraceEventType.Verbose,
        Warning = TraceEventType.Warning,
    }

    public interface ILogger : IDisposable
    {
        void Log(LogEntry entry);

        ILogger CreateScope(string activityName = "", int activityID = 0);
    }

    public class LogEntry
    {
        public readonly Severity Severity;
        public readonly string Message = Constants.DEFAULT_MESSAGE;
        public readonly object[] Datum = Constants.DEFAULT_DATUM;
        public readonly Exception Exception = Constants.DEFAULT_EXCEPTION;

        public LogEntry(
            Severity severity,
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
                (Message == Constants.DEFAULT_MESSAGE || Exception == Constants.DEFAULT_EXCEPTION)
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
                    Severity.Critical,
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
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    Severity.Critical,
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
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    Severity.Critical,
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
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    Severity.Error,
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
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    Severity.Error,
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
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            logger.Log(
                new LogEntry(
                    Severity.Error,
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
            logger.Log(
                new LogEntry(
                    Severity.Information,
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
            logger.Log(
                new LogEntry(
                    Severity.Information,
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
            logger.Log(
                new LogEntry(
                    Severity.Verbose,
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
            logger.Log(
                new LogEntry(
                    Severity.Verbose,
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
            logger.Log(
                new LogEntry(
                    Severity.Warning,
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
            logger.Log(
                new LogEntry(
                    Severity.Warning,
                    datum: datum
                )
            );
        }
    }
}