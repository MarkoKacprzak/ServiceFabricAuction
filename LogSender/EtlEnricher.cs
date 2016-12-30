using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace LogSender
{
    class EtlEnricher : ILogEventEnricher
    {
        private readonly Dictionary<string, object> _dict;
        public EtlEnricher(Dictionary<string, object> dict)
        {
            _dict = dict;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var key in _dict.Keys)
            {
                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty(key, _dict[key], true));
            }
        }
    }
}