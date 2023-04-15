using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Yandex.Cloud.Logging.V1;
using static Yandex.Cloud.Logging.V1.LogIngestionService;

namespace Serilog.Sinks.YandexCloud
{
    internal class YandexCloudSink : IBatchedLogEventSink
    {
        private readonly LogIngestionServiceClient _logIngestionService;
        private readonly YandexCloudSinkSettings _settings;

        public YandexCloudSink(LogIngestionServiceClient logIngestionService, YandexCloudSinkSettings settings)
        {
            _logIngestionService = logIngestionService ?? throw new ArgumentNullException(nameof(logIngestionService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            try
            {
                var request = CreateWriteRequest();

                foreach (var entry in batch)
                {
                    request.Entries.Add(entry.ToIncomingLogEntry());
                }

                await _logIngestionService.WriteAsync(request);

            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"[{nameof(YandexCloudSink)}] error while sending log events\n{ex}");
            }
        }

        private WriteRequest CreateWriteRequest()
        {
            var request = new WriteRequest
            {
                Destination = new Destination(),
                Resource = new LogEntryResource
                {
                    Id = _settings.ResourceId,
                    Type = _settings.ResourceType
                },
            };

            if (!string.IsNullOrEmpty(_settings.FolderId))
            {
                request.Destination.FolderId = _settings.FolderId;
            }

            if (!string.IsNullOrEmpty(_settings.LogGroupId))
            {
                request.Destination.LogGroupId = _settings.LogGroupId;
            }

            if (!string.IsNullOrEmpty(_settings.ResourceId) || !string.IsNullOrEmpty(_settings.ResourceType))
            {
                request.Resource = new LogEntryResource();

                if (!string.IsNullOrEmpty(_settings.ResourceId))
                {
                    request.Resource.Id = _settings.ResourceId;
                }

                if (!string.IsNullOrEmpty(_settings.ResourceType))
                {
                    request.Resource.Type = _settings.ResourceType;
                }
            }

            return request;
        }

        public Task OnEmptyBatchAsync() => Task.CompletedTask;
    }
}
