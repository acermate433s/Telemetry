using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Telemetry.Proxy.Attributes;

namespace Telemetry.Proxy
{
    /// <summary>
    /// Raised before the method is invoked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MessageEventArgs"/> instance containing the event data.</param>
    public delegate void BeforeExecuteEventHandler(object sender, MessageEventArgs e);

    /// <summary>
    /// Raised after the method is invoked successfully
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MessageEventArgs"/> instance containing the event data.</param>
    public delegate void AfterExecuteEventHandler(object sender, MessageEventArgs e);

    /// <summary>
    /// Raised when an exception happens
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MessageEventArgs"/> instance containing the event data.</param>
    public delegate void ErrorExecuteEventHandler(object sender, MessageEventArgs e);

    /// <summary>
    /// A generic logging decorator based on https://msdn.microsoft.com/en-ca/magazine/dn574804.aspx
    /// </summary>
    /// <typeparam name="TLogger">An instance of ILogger.</typeparam>
    /// <typeparam name="TSource">The object type to be decorated.</typeparam>
    /// <seealso cref="System.Runtime.Remoting.Proxies.RealProxy" />
    public class LoggingProxy<TLogger, TSource> : RealProxy where TLogger : ILogger where TSource : class
    {
        private readonly TLogger _Logger;
        private readonly TSource _Decorated;
        
        public event BeforeExecuteEventHandler BeforeExecute;
        public event AfterExecuteEventHandler AfterExecution;
        public event ErrorExecuteEventHandler ErrorExecute;

        public LoggingProxy(
            TLogger logger,
            TSource decorated
        ) : base(typeof(TSource))
        {
            if (!typeof(TSource).IsInterface)
                throw new ArgumentException("TSource must be an interface");

            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

            if (decorated == null)
                throw new ArgumentNullException(nameof(decorated), "Type to log cannot be null");

            _Logger = logger;
            _Decorated = decorated;
        }

        public override IMessage Invoke(IMessage msg)
        {
            BeforeExecute?.Invoke(this, new MessageEventArgs(msg));

            var methodCall = msg as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;

            ILogger logger = _Logger;
            
            // check if we need to create a new activity for the method call
            var activityAttribute = methodInfo.GetCustomAttributes(true).OfType<ActivityAttribute>();
            if (activityAttribute.Count() > 0)
            {
                var activity = activityAttribute.First();
                logger = _Logger.CreateScope(activity.Name, activity.Id);
            }

            // log the method name and the parameter name and values
            logger
                .Verbose(
                    String.Format(
                        "Invoking {0}.{1}({2})",
                        _Decorated.GetType().Name,
                        methodCall.MethodName,
                        methodCall.ArgCount > 0
                            ? String.Join(
                                ", ",
                                methodCall
                                    .Args
                                    .Select((item, index) => $"({item.GetType().Name}) {methodCall.GetArgName(index)} = {item.ToString()}")
                            )
                            : ""
                    )
                );

            // if the method has a custom message, add the message to the log
            var message =
                methodInfo
                    .GetCustomAttributes(typeof(MessageAttribute), true)
                    .Cast<MessageAttribute>()
                    .DefaultIfEmpty(null)   
                    .First()?
                    .Message ?? "";

            if (!String.IsNullOrEmpty(message))
                logger.Information(message);

            try
            {
                var result = methodInfo.Invoke(_Decorated, methodCall.InArgs);

                AfterExecution?.Invoke(this, new MessageEventArgs(msg));

                return
                    new ReturnMessage(
                        result,
                        null,
                        0,
                        methodCall.LogicalCallContext,
                        methodCall
                    );
            }
            catch (Exception ex)
            {
                _Logger
                    .Error(
                        ex,
                        $"Exception in {_Decorated.GetType().Name}.{methodCall.MethodName}"
                    );

                ErrorExecute?
                    .Invoke(
                        this, 
                        new MessageEventArgs(msg)
                        {
                            Exception = ex
                        }
                    );

                return new ReturnMessage(ex, methodCall);
            }
            finally
            {
                // make sure to end the activity of the current logger if any
                logger.Dispose();
            }
        }
    }
}
