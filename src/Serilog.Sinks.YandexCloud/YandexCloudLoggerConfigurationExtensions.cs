using Serilog.Configuration;
using Serilog.Debugging;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using Yandex.Cloud;
using YandexCloud.IamJwtCredentials;

namespace Serilog.Sinks.YandexCloud
{
    public static class YandexCloudLoggerConfigurationExtensions
    {
        public static LoggerConfiguration YandexCloud(this LoggerSinkConfiguration sinkConfiguration,
            string keyId,
            string serviceAccountId,
            string privateKey,
            string? folderId = null,
            string? logGroupId = null,
            string? resourceId = null,
            string? resourceType = null,
            int batchSizeLimit = 100,
            int period = 2,
            int queueLimit = 1000,
            bool eagerlyEmitFirstEvent = true)
        {
            var credentials = new IamJwtCredentialsConfiguration
            {
                Id = keyId,
                ServiceAccountId = serviceAccountId,
                PrivateKey = privateKey
            };

            var sdk = new Sdk(new IamJwtCredentialsProvider(credentials));

            var settings = new YandexCloudSinkSettings
            {
                FolderId = folderId,
                LogGroupId = logGroupId,
                ResourceId = resourceId,
                ResourceType = resourceType
            };

            settings.Validate();

            var sink = new YandexCloudSink(sdk.Services.Logging.LogIngestionService, settings);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit,
                Period = TimeSpan.FromSeconds(period),
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent,
                QueueLimit = queueLimit
            };

            var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);

            return sinkConfiguration.Sink(batchingSink);
        }
    }
}
