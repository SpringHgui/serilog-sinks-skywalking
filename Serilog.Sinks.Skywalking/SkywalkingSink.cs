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
    public class SkywalkingSink : IBatchedLogEventSink
    {
        ITextFormatter _formatter;
        IServiceProvider serviceCollection;

        public SkywalkingSink(IServiceProvider serviceCollection, ITextFormatter formatter)
        {
            this._formatter = formatter;
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            var skyApmLogDispatcher = serviceCollection.GetService<ISkyApmLogDispatcher>();
            if (skyApmLogDispatcher == null)
                return;

            var _entrySegmentContextAccessor = serviceCollection.GetService<IEntrySegmentContextAccessor>();
            if (_entrySegmentContextAccessor == null)
                return;

            foreach (var logEvent in batch)
            {
                using (var render = new StringWriter(CultureInfo.InvariantCulture))
                {
                    _formatter.Format(logEvent, render);

                    render.ToString();
                }

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

            await Task.CompletedTask;
        }

        public async Task OnEmptyBatchAsync()
        {
            await Task.CompletedTask;
        }
    }
}
