using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using SkyApm.Tracing;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using SkyApm.Tracing.Segments;
using LogEvent = Serilog.Events.LogEvent;

namespace Serilog.Sinks.Skywalking
{
    public class SkywalkingSink : ILogEventSink
    {
        ITextFormatter _formatter;

        public SkywalkingSink(IServiceProvider serviceCollection, ITextFormatter formatter)
        {
            _formatter = formatter;
            _skyApmLogDispatcher = serviceCollection.GetRequiredService<ISkyApmLogDispatcher>();
            _entrySegmentContextAccessor = serviceCollection.GetRequiredService<IEntrySegmentContextAccessor>();

        }

        ISkyApmLogDispatcher _skyApmLogDispatcher;
        IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        public void Emit(LogEvent logEvent)
        {
            string renderMessage;
            if (_formatter != null)
            {
                using var render = new StringWriter(CultureInfo.InvariantCulture);
                _formatter.Format(logEvent, render);

                renderMessage = render.ToString();
            }
            else
            {
                renderMessage = logEvent.RenderMessage();

                if (logEvent.Exception != null)
                    renderMessage += Environment.NewLine + logEvent.Exception.ToString();
            }

            var logs = new Dictionary<string, object>
            {
                { "Level", logEvent.Level.ToString() },
                { "logMessage", renderMessage }
            };
            SegmentContext segmentContext = _entrySegmentContextAccessor.Context;
            var logContext = new LoggerRequest
            {
                Logs = logs,
                SegmentReference = segmentContext == null
                    ? null
                    : new LoggerSegmentReference
                    {
                        TraceId = segmentContext.TraceId,
                        SegmentId = segmentContext.SegmentId
                    }
            };

            _skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
