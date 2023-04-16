using Yandex.Cloud.Logging.V1;
using Yandex.Cloud.Resourcemanager.V1;

namespace Serilog.Sinks.YandexCloud
{
    internal class YandexCloudSinkSettings
    {
        public string? FolderId { get; set; }
        public string? LogGroupId { get; set; }
        public string? ResourceId { get; set; }
        public string? ResourceType { get; set; }

        public void Validate()
        {
            if (!string.IsNullOrEmpty(FolderId) && !string.IsNullOrEmpty(LogGroupId))
            {
                throw new ArgumentException($"{nameof(FolderId)} and {nameof(LogGroupId)} parameters can't be specified together.");
            }

            if (string.IsNullOrEmpty(FolderId) && string.IsNullOrEmpty(LogGroupId))
            {
                throw new ArgumentException($"One of {nameof(FolderId)} or {nameof(LogGroupId)} parameters arguments is required.");
            }
        }
    }
}
