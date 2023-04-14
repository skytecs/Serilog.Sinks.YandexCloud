namespace Serilog.Sinks.YandexCloud
{
    internal class YandexCloudSinkSettings
    {
        public string FolderId { get; set; }
        public string LogGroupId { get; set; }
        public string ResourceId { get; set; }
        public string ResourceType { get; set; }
    }
}
