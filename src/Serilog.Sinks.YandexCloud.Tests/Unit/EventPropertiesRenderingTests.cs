using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.YandexCloud.Tests.Unit;

public class EventPropertiesRenderingTests
{
    [Test]
    public void ScalarEventPropertiesShouldBeConvertedToProtobuf()
    {
        var messageTemplate = new MessageTemplate(Array.Empty<MessageTemplateToken>());

        var eventProperties = new[]
        {
            new LogEventProperty("textValue", new ScalarValue("a text"))
        };

        var serilogEntry = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            messageTemplate, eventProperties);

        var yandexEntry = serilogEntry.ToIncomingLogEntry();

        Assert.That(yandexEntry.JsonPayload.Fields.ContainsKey("textValue"));

        Assert.That(yandexEntry.JsonPayload.Fields["textValue"].StringValue, Is.EqualTo("a text"));
    }

    [Test]
    public void StructEventPropertiesShouldBeConvertedToProtobuf()
    {
        var messageTemplate = new MessageTemplate(Array.Empty<MessageTemplateToken>());

        var eventProperties = new[]
        {
            new LogEventProperty("structValue", new StructureValue(new[]
            {
                new LogEventProperty("field1", new ScalarValue("a text")),
                new LogEventProperty("field2", new StructureValue(new[]
                {
                    new LogEventProperty("field2_1", new ScalarValue("another text"))
                }))
            }))
        };

        var serilogEntry = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            messageTemplate, eventProperties);

        var yandexEntry = serilogEntry.ToIncomingLogEntry();

        var structValue = yandexEntry.JsonPayload.Fields["structValue"].StructValue;

        Assert.That(structValue.Fields.Count, Is.EqualTo(2));
        Assert.That(structValue.Fields["field1"].StringValue,
            Is.EqualTo("a text"));
        Assert.That(structValue.Fields["field2"].StructValue.Fields["field2_1"].StringValue,
            Is.EqualTo("another text"));
    }

    [Test]
    public void SequenceEventPropertiesShouldBeConvertedToProtobuf()
    {
        var messageTemplate = new MessageTemplate(Array.Empty<MessageTemplateToken>());

        var eventProperties = new[]
        {
            new LogEventProperty("listValue", new SequenceValue(new[]
            {
                new ScalarValue("a text"),
                new ScalarValue("another text")
            }))
        };

        var serilogEntry = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            messageTemplate, eventProperties);

        var yandexEntry = serilogEntry.ToIncomingLogEntry();

        var listValue = yandexEntry.JsonPayload.Fields["listValue"].ListValue;

        Assert.That(listValue.Values.Count, Is.EqualTo(2));
        Assert.That(listValue.Values[0].StringValue, Is.EqualTo("a text"));
        Assert.That(listValue.Values[1].StringValue, Is.EqualTo("another text"));
    }

}