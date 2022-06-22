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
    public class SkywalkingBatchedSink : SkywalkingSink, IBatchedLogEventSink
    {
        public SkywalkingBatchedSink(IServiceProvider serviceCollection, ITextFormatter formatter)
           : base(serviceCollection, formatter)
        {
        }

        public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            foreach (var item in batch)
            {
                base.Emit(item);
            }

            return Task.CompletedTask;
        }

        public Task OnEmptyBatchAsync()
        {
            return Task.CompletedTask;
        }
    }
}
