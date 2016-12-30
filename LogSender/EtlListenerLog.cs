using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Serilog;

namespace LogSender
{
    public class EtlListenerLog
    {
        /// <summary>
        /// Helper class to register each ETL Event method that should sand info to Seq
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        private class CallbackRegistrator<TService> where TService : EventSource
        {
            private readonly TraceEventSession _activeSession;

            /// <summary>
            /// Gets the name of the provider.
            /// </summary>
            /// <value>
            /// The name of the provider.
            /// </value>
            public string ProviderName { get; }
            
            #region constructor            

            /// <summary>
            /// Initializes a new instance of the <see cref="CallbackRegistrator{TService}"/> class.
            /// </summary>
            /// <param name="activeSession">The active session.</param>
            public CallbackRegistrator(TraceEventSession activeSession)
            {
                _activeSession = activeSession;
                ProviderName = typeof(TService)
                    .GetCustomAttributes<EventSourceAttribute>()
                    .FirstOrDefault()
                    .Name;
            }

            #endregion            

            /// <summary>
            /// Registers callback to send ETW event to Seq service 
            /// As a parametr should be methodInfo from EventSource with [Event] attribute on that method
            /// </summary>
            /// <param name="methodInfo">The method information.</param>
            public void Register(MethodInfo methodInfo)
            {
                _activeSession.Source.Dynamic.AddCallbackForProviderEvent(
                    ProviderName,
                    methodInfo.Name,
                    LogLevel);
            }
            /// <summary>
            /// Gets the default parameter for Enricher logger.
            /// </summary>
            /// <param name="trace">The trace.</param>
            /// <returns></returns>
            private static Dictionary<string, object> GetDefaultParam(TraceEvent trace)
            {
                var values = new Dictionary<string, object>();
                var delay = (DateTime.Now - trace.TimeStamp).TotalSeconds;
                values.Add("PID", trace.ProcessID);
                values.Add("TaskName", trace.TaskName);
                values.Add("ProviderName", trace.ProviderName);
                values.Add("Delay", delay);
                values.Add("TimeStamp", trace.TimeStamp);
                trace.PayloadNames.ToList().ForEach(name =>
                {
                    //stringBuilder.Append("{" + a.Name + "};");
                    values.Add(name, trace.PayloadByName(name));
                });
                return values;
            }

            /// <summary>
            /// Translate each method in EvenSource (as a event type) into SeriLog coresponding level
            /// TraceEvent PayloadByName give a value from match method name
            /// </summary>
            /// <param name="trace">The trace event log from ETL.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">level - null</exception>
            private static void LogLevel(TraceEvent trace)
            {
                var values = GetDefaultParam(trace);
                var message = trace.FormattedMessage;
                switch (trace.Level)
                {
                    case TraceEventLevel.Always:
                        EnrichLog(values).Verbose(message);
                        break;
                    case TraceEventLevel.Critical:
                        EnrichLog(values).Fatal(message);
                        break;
                    case TraceEventLevel.Error:
                        EnrichLog(values).Error(message);
                        break;
                    case TraceEventLevel.Warning:
                        EnrichLog(values).Warning(message);
                        break;
                    case TraceEventLevel.Informational:
                        EnrichLog(values).Information(message);
                        break;
                    case TraceEventLevel.Verbose:
                       EnrichLog(values).Verbose(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static ILogger EnrichLog(Dictionary<string, object> dict) => Log.ForContext(new EtlEnricher(dict));            

        /// <summary>
        /// Translate each EventSource Event definition into corresponding callback into serilog logger
        /// </summary>
        /// <typeparam name="TService">The type of the service that want to listent to.</typeparam>
        /// <param name="activeSession">The active session during to observe Events.</param>
        public static void Register<TService>(TraceEventSession activeSession) where TService : EventSource
        {
            var registrator = new CallbackRegistrator<TService>(activeSession);
            typeof(TService).GetMethods()
                .Where(prop => Attribute.IsDefined(prop, typeof(EventAttribute)))
                //.OrderBy(p => GetAttributeVaue<EventAttribute>(p).EventId);
                .ToList()
                .ForEach(@event => registrator.Register(@event));

            activeSession.EnableProvider(registrator.ProviderName);
        }

        /// <summary>
        /// Gets the attribute vaue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInfo">The method information.</param>
        /// <returns></returns>
        private static T GetAttributeVaue<T>(MethodInfo methodInfo)
            where T:Attribute=>
            methodInfo.GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .FirstOrDefault();
    }
}