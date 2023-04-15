using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.YandexCloud.Tests.Unit;
public class TemplateTokensRenderingTests
{
    [Test]
    public void SimpleEventsShouldBeRenderedAsTextMessages()
    {
        var messageTemplate = new MessageTemplate(new[] { new TextToken("message text") });

        var serilogEntry = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            messageTemplate, Array.Empty<LogEventProperty>());

        var yandexEntry = serilogEntry.ToIncomingLogEntry();

        Assert.That(yandexEntry.Message, Is.EqualTo("message text"));
    }

    [Test]
    public void PropertyTokensShouldBeSubsttutedInMessageTextAndAddedToThePayload()
    {
        var messageTemplate = new MessageTemplate("message text with {substitution}", new MessageTemplateToken[]
        {
            new TextToken("message text with "),
            new PropertyToken("substitution", "substitution value", startIndex: 18)
        });

        var serilogEntry = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            messageTemplate, Array.Empty<LogEventProperty>());

        var yandexEntry = serilogEntry.ToIncomingLogEntry();

        Assert.That(yandexEntry.Message, Is.EqualTo("message text with substitution value"));
    }
}
