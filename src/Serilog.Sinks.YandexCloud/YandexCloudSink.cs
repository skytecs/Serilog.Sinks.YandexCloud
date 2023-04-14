using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using Yandex.Cloud;

namespace Serilog.Sinks.YandexCloud
{
    internal class YandexCloudSink : IBatchedLogEventSink
    {
        private readonly Sdk _sdk;
        private readonly ITextFormatter _formatter;
        private readonly YandexCloudSinkSettings _settings;

        public YandexCloudSink(Sdk sdk, ITextFormatter formatter, YandexCloudSinkSettings settings)
        {
            _sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
            _formatter = formatter;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            try
            {
                var request = new Yandex.Cloud.Logging.V1.WriteRequest
                {
                    Destination = new Yandex.Cloud.Logging.V1.Destination
                    {
                        FolderId = _settings.FolderId,
                        LogGroupId = _settings.LogGroupId,
                    },
                    Resource = new Yandex.Cloud.Logging.V1.LogEntryResource
                    {
                        Id = _settings.ResourceId,
                        Type = _settings.ResourceType
                    },
                };

                foreach (var entry in batch)
                {
                    request.Entries.Add(entry.ToIncomingLogEntry(_formatter));
                }

                await _sdk.Services.Logging.LogIngestionService.WriteAsync(request);

            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"[{nameof(YandexCloudSink)}] error while sending log events - {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Task OnEmptyBatchAsync() { }
    }
}
