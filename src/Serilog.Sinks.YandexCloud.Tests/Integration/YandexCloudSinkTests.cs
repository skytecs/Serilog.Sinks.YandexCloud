using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using Serilog.Context;
using Serilog.Core;
using Serilog.Debugging;
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

        logger.Write(LogEventLevel.Information, "test message");

        Assert.That(ingestionServiceMock.Invocations.Count, Is.EqualTo(1));

        var writeRequest = ingestionServiceMock.Invocations.First().Arguments[0] as WriteRequest;
        
        Assert.That(writeRequest.Resource.Id, Is.EqualTo(settings.ResourceId));
        Assert.That(writeRequest.Resource.Type, Is.EqualTo(settings.ResourceType));
        Assert.That(writeRequest.Destination.LogGroupId, Is.EqualTo(settings.LogGroupId));
        Assert.That(writeRequest.Entries.Count, Is.EqualTo(1));
    }

    [Test]
    public void ShouldWriteToSelfLogOnError()
    {
        var exceptionMessage = "Custom Exception with {FalseField}";
        var stringWriter = new StringWriter();
        SelfLog.Enable(stringWriter);

        var ingestionServiceMock = CreateIngestionServiceMockFromLambda(()=>throw new Exception(exceptionMessage));

        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
        };

        var sink = new YandexCloudSink(ingestionServiceMock.Object, settings);

        var logger = new LoggerConfiguration().WriteTo.Sink(new WrapperSink(sink))
            .CreateLogger();

        logger.Write(LogEventLevel.Information, "test message");

        Assert.That(stringWriter.ToString(), Does.Contain(exceptionMessage));

        SelfLog.Disable();
    }

    [Test]
    public void ScalarLogPropertiesShouldAppearInPayload()
    {
        var ingestionServiceMock = CreateIngestionServiceMock();

        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
        };

        var sink = new YandexCloudSink(ingestionServiceMock.Object, settings);

        var logger = new LoggerConfiguration()
            .Enrich.WithProperty("CustomScalarProperty","property value")
            .WriteTo.Sink(new WrapperSink(sink))
            .CreateLogger();

        logger.Write(LogEventLevel.Information, "test message");

        var writeRequest = ingestionServiceMock.Invocations.First().Arguments[0] as WriteRequest;

        var customPropertyField = writeRequest.Entries[0].JsonPayload.Fields["CustomScalarProperty"].StringValue;

        Assert.That(customPropertyField, Is.EqualTo("property value"));
    }

    [Test]
    public void ListLogPropertiesShouldAppearInPayload()
    {
        var ingestionServiceMock = CreateIngestionServiceMock();

        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
        };

        var sink = new YandexCloudSink(ingestionServiceMock.Object, settings);

        // TODO: Add support for scalar types other than strings
        var logger = new LoggerConfiguration()
            .Enrich.WithProperty("CustomListProperty", new object[] { "1", "string value", "true" })
            .WriteTo.Sink(new WrapperSink(sink))
            .CreateLogger();

        logger.Write(LogEventLevel.Information, "test message");

        var writeRequest = ingestionServiceMock.Invocations.First().Arguments[0] as WriteRequest;

        var customPropertyField = writeRequest.Entries[0].JsonPayload.Fields["CustomListProperty"].ListValue;

        Assert.That(customPropertyField.Values.Count, Is.EqualTo(3));
        Assert.That(customPropertyField.Values.Contains(Value.ForString("1")));
        Assert.That(customPropertyField.Values.Contains(Value.ForString("string value")));
        Assert.That(customPropertyField.Values.Contains(Value.ForString("true")));
    }

    [Test]
    public void MessageTemplateFieldsShouldAppearInPayload()
    {
        var ingestionServiceMock = CreateIngestionServiceMock();

        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
        };

        var sink = new YandexCloudSink(ingestionServiceMock.Object, settings);

        // TODO: Add support for scalar types other than strings
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(new WrapperSink(sink))
            .CreateLogger();

        logger.Write(LogEventLevel.Information, "Hello, {CustomText:l}!", "World");

        var writeRequest = ingestionServiceMock.Invocations.First().Arguments[0] as WriteRequest;
        Assert.That(writeRequest.Entries.First().Message, Is.EqualTo("Hello, World!"));

        var customPropertyField = writeRequest.Entries[0].JsonPayload.Fields["CustomText"].StringValue;

        Assert.That(customPropertyField, Is.EqualTo("World"));
    }

    private static Mock<LogIngestionServiceClient> CreateIngestionServiceMockFromLambda(Func<WriteResponse> valueFactory)
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
                    Task.FromResult(valueFactory()),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });
            });

        return ingestionServiceMock;
    }

    private static Mock<LogIngestionServiceClient> CreateIngestionServiceMock() => 
        CreateIngestionServiceMockFromLambda(() => new WriteResponse());

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
