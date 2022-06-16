using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using SkyApm.Tracing;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Serilog.Sinks.Skywalking
{
    public class SkywalkingSink : ILogEventSink
    {
        ITextFormatter _formatter;
        IServiceProvider _serviceCollection;

        public SkywalkingSink(IServiceProvider serviceCollection, ITextFormatter formatter)
        {
            this._serviceCollection = serviceCollection;
            this._formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            var skyApmLogDispatcher = _serviceCollection.GetService<ISkyApmLogDispatcher>();
            if (skyApmLogDispatcher == null)
                return;

            var _entrySegmentContextAccessor = _serviceCollection.GetService<IEntrySegmentContextAccessor>();
            if (_entrySegmentContextAccessor == null || _entrySegmentContextAccessor.Context == null)
                return;

            var logs = new Dictionary<string, object>();
            logs.Add("className", "className");
            logs.Add("Level", logEvent.Level.ToString());
            logs.Add("logMessage", logEvent.RenderMessage());

            var logContext = new SkyApm.Tracing.Segments.LoggerContext()
            {
                Logs = logs,
                SegmentContext = _entrySegmentContextAccessor.Context,
                Date = DateTimeOffset.UtcNow.Offset.Ticks
            };

            skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
