using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Yandex.Cloud.Billing.V1;
using Yandex.Cloud.Iam.V1;
using Yandex.Cloud.Logging.V1;
using static Yandex.Cloud.Logging.V1.LogIngestionService;

namespace Serilog.Sinks.YandexCloud.Tests.Integration;
public class YandexCloudSinkTests
{
    [Test]
    public void ShouldWriteToLog()
    {
        var ingestionServiceMock = CreateIngestionServiceMock();

        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(), 
            ResourceType = Guid.NewGuid().ToString(),
        };

        var sink = new YandexCloudSink(ingestionServiceMock.Object, settings);

        var logger = new LoggerConfiguration().WriteTo.Sink(new WrapperSink(sink))
            .CreateLogger();

        logger.Write(LogEventLevel.Information, "test message {subst}","subst value");

        Assert.That(ingestionServiceMock.Invocations.Count, Is.EqualTo(1));

        var writeRequest = ingestionServiceMock.Invocations.First().Arguments[0] as WriteRequest;
        
        Assert.That(writeRequest.Resource.Id, Is.EqualTo(settings.ResourceId));
        Assert.That(writeRequest.Resource.Type, Is.EqualTo(settings.ResourceType));
        Assert.That(writeRequest.Destination.LogGroupId, Is.EqualTo(settings.LogGroupId));
        Assert.That(writeRequest.Entries.Count, Is.EqualTo(1));
    }

    private static Mock<LogIngestionServiceClient> CreateIngestionServiceMock()
    {
        var ingestionServiceMock = new Mock<LogIngestionServiceClient>();

        ingestionServiceMock
            .Setup(x => x.WriteAsync(
                It.IsAny<WriteRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns((WriteRequest r, Metadata m, DateTime? d, CancellationToken ct) =>
            {
                return TestCalls.AsyncUnaryCall(
                    Task.FromResult(new WriteResponse()),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });
            });

        return ingestionServiceMock;
    }

    class WrapperSink : ILogEventSink
    {
        private readonly YandexCloudSink _sink;

        public WrapperSink(YandexCloudSink sink)
        {
            _sink = sink;
        }

        public void Emit(LogEvent logEvent) =>
            _sink.EmitBatchAsync(new[] { logEvent }).Wait();

    }
}
