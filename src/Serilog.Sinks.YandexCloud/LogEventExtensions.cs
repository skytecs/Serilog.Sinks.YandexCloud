using Google.Protobuf.WellKnownTypes;
using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

namespace Serilog.Sinks.YandexCloud
{
    public static class LogEventExtensions
    {
        public static Yandex.Cloud.Logging.V1.IncomingLogEntry ToIncomingLogEntry(this LogEvent entry, ITextFormatter formatter)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return new Yandex.Cloud.Logging.V1.IncomingLogEntry
            {
                Level = entry.ToLevel(),
                Timestamp = entry.Timestamp.ToTimestamp(),
                Message = entry.FormatWith(formatter),
                JsonPayload = entry.ToStructProperty(),
            };
        }

        public static Yandex.Cloud.Logging.V1.LogLevel.Types.Level ToLevel(this LogEvent entry)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return entry.Level switch
            {
                LogEventLevel.Fatal => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Fatal,
                LogEventLevel.Error => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Error,
                LogEventLevel.Warning => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Warn,
                LogEventLevel.Information => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Info,
                LogEventLevel.Debug => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Debug,
                LogEventLevel.Verbose => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Trace,

                _ => Yandex.Cloud.Logging.V1.LogLevel.Types.Level.Debug
            };

        }

        public static string FormatWith(this LogEvent entry, ITextFormatter formatter)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (formatter is null)
            {
                return entry.RenderMessage();
            }

            var sw = new StringWriter();
            formatter.Format(entry, sw);
            return sw.ToString();
        }

        public static dynamic ToStructProperty(this LogEvent entry)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var properties = new Dictionary<string, dynamic>();

            foreach (var key in entry.Properties.Keys)
            {
                properties.Add(key, ToStructure(entry.Properties[key]));
            }

            return Struct.Parser.ParseJson(JsonSerializer.Serialize(properties));
        }

        private static dynamic ToStructure(LogEventPropertyValue property)
        {
            if (property is null)
            {
                return null;
            }

            switch (property)
            {
                case ScalarValue scalar:
                    {
                        return scalar.Value?.ToString();
                    }

                case StructureValue structure:
                    {
                        var dictionary = new Dictionary<string, dynamic>();

                        foreach (var item in structure.Properties)
                        {
                            dictionary.Add(item.Name, ToStructure(item.Value));
                        }

                        return dictionary;
                    }
                case SequenceValue list:
                    {
                        return list.Elements.Select(x => ToStructure(x)).ToList();
                    }
                default:
                    {
                        return "";
                    }
            }
        }
    }

}
