using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Sinks.Skywalking;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Serilog
{
    public static class LoggerConfigurationExtensions
    {
        static LoggerConfigurationExtensions() { }

        public static LoggerConfiguration Skywalking(
            this LoggerSinkConfiguration loggerConfiguration, IServiceProvider serviceCollection, ITextFormatter formatter = null)
        {
            return loggerConfiguration
                .Sink(new SkywalkingSink(serviceCollection, formatter));
        }

        public static LoggerConfiguration SkywalkingBatch(
            this LoggerSinkConfiguration loggerConfiguration, IServiceProvider serviceCollection, int batchSizeLimit, int period, ITextFormatter formatter = null)
        {
            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit,
                Period = TimeSpan.FromSeconds(period)
            };

            var batchingSink = new PeriodicBatchingSink(new SkywalkingBatchedSink(serviceCollection, formatter), batchingOptions);

            return loggerConfiguration
                .Sink(batchingSink);
        }
    }
}
