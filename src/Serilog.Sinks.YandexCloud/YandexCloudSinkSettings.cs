using System.Text.RegularExpressions;
using Yandex.Cloud.Logging.V1;
using Yandex.Cloud.Resourcemanager.V1;

namespace Serilog.Sinks.YandexCloud
{
    internal class YandexCloudSinkSettings
    {
        private static readonly Regex _fieldRegex = new (@"^([a-zA-Z0-9][-a-zA-Z0-9_.]{0,63})?$", RegexOptions.Compiled);

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

            if (!string.IsNullOrEmpty(ResourceType) && !_fieldRegex.IsMatch(ResourceType))
            {
                throw new ArgumentException($"{nameof(ResourceType)} is in incorrect format.");
            }

            if (!string.IsNullOrEmpty(ResourceId) && !_fieldRegex.IsMatch(ResourceId))
            {
                throw new ArgumentException($"{nameof(ResourceId)} is in incorrect format.");
            }

            if (!string.IsNullOrEmpty(FolderId) && !_fieldRegex.IsMatch(FolderId))
            {
                throw new ArgumentException($"{nameof(FolderId)} is in incorrect format.");
            }

            if (!string.IsNullOrEmpty(LogGroupId) && !_fieldRegex.IsMatch(LogGroupId))
            {
                throw new ArgumentException($"{nameof(FolderId)} is in incorrect format.");
            }
        }
    }
}
