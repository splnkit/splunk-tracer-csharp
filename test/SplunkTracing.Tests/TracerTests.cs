using System;
using System.Collections.Generic;
using System.Linq;
using SplunkTracing.Collector;
using Moq;
using OpenTracing.Propagation;
using Xunit;

namespace SplunkTracing.Tests
{
    public class TracerTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var collectorOptions = new CollectorOptions("localhost", 80, true);
            var tracerOptions = new Options("TEST").WithCollector(collectorOptions).WithAutomaticReporting(false);

            return new Tracer(tracerOptions, spanRecorder);
        }

        [Fact]
        public void TracerShouldBuildSpan()
        {
            var tracer = GetTracer();
            var span = tracer.BuildSpan("test").Start();
            Assert.NotNull(span);
        }

        [Fact]
        public void TracerShouldExtractFromValidSpan()
        {
            var tracer = GetTracer();
            // by convention, we expect the upstream RPC to send ids as hex strings but to become uint64 internally
            var traceId = new Random().NextUInt64();
            var spanId = new Random().NextUInt64();
            var data = new Dictionary<string, string>
            {
                {"ot-tracer-traceid", traceId.ToString("X")},
                {"ot-tracer-spanid", spanId.ToString("X")}
            };
            var spanContext = tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(data));
            Assert.NotNull(spanContext);
            Assert.Equal(traceId.ToString(), spanContext.TraceId);
            Assert.Equal(spanId.ToString(), spanContext.SpanId);
        }

        [Fact]
        public void TracerShouldFailToExtractFromInvalidSpan()
        {
            var tracer = GetTracer();
            var data = new Dictionary<string, string>();
            var spanContext = tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(data));
            Assert.Null(spanContext);
        }

        [Fact]
        public void TracerShouldInjectTextMap()
        {
            var tracer = GetTracer();
            var span = tracer.BuildSpan("test").Start();
            var hexTraceId = Convert.ToUInt64(span.TypedContext().TraceId).ToString("X");
            var hexSpanId = Convert.ToUInt64(span.TypedContext().SpanId).ToString("X");
            var data = new Dictionary<string, string>();
            tracer.Inject(span.Context, BuiltinFormats.TextMap, new TextMapInjectAdapter(data));
            Assert.Equal(hexTraceId, data["ot-tracer-traceid"]);
            Assert.Equal(hexSpanId, data["ot-tracer-spanid"]);
        }

        [Fact]
        public void TracerShouldRecordAllSpanData()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);

            var startTimeStamp = new DateTimeOffset(2018, 2, 19, 12, 0, 0, TimeSpan.Zero);
            var endTimeStamp = new DateTimeOffset(2018, 2, 19, 12, 0, 5, TimeSpan.Zero);

            var span = tracer.BuildSpan("test")
                .WithStartTimestamp(startTimeStamp)
                .Start()
                .SetTag("key", "testValue")
                .SetBaggageItem("baggageKey", "bagTestValue");
            span.Finish(endTimeStamp);

            var recordedSpans = recorder.GetSpans().First();

            Assert.Equal("test", recordedSpans.OperationName);
            Assert.Equal(startTimeStamp, recordedSpans.StartTimestamp);
            Assert.Equal(TimeSpan.FromSeconds(5), recordedSpans.Duration);

            Assert.Equal("bagTestValue", recordedSpans.Context.GetBaggageItem("baggageKey"));
            Assert.Equal("testValue", recordedSpans.Tags["key"]);

            Assert.False(string.IsNullOrWhiteSpace(recordedSpans.Context.TraceId));
            Assert.False(string.IsNullOrWhiteSpace(recordedSpans.Context.SpanId));
        }

        [Fact]
        public void TracerOptionsShouldLetYouOverrideTags()
        {
            var collectorOptions = new CollectorOptions("localhost", 80, true);
            var overrideTags = new Dictionary<string, object> {{SplunkTracingConstants.ComponentNameKey, "test_component"}};
            var tracerOptions = new Options("TEST").WithCollector(collectorOptions).WithTags(overrideTags).WithAutomaticReporting(false);
            
            Assert.Equal("test_component", tracerOptions.Tags[SplunkTracingConstants.ComponentNameKey]);
            Assert.Equal(SplunkTracingConstants.TracerPlatformValue,
                tracerOptions.Tags[SplunkTracingConstants.TracerPlatformKey]);
        }

        [Fact]
        public void TracerShouldTrapExceptions()
        {
            var x = false;
            Action<Exception> eh = delegate { x = true; };
            var collectorOptions = new CollectorOptions("localhost", 80, true);
            var tracerOptions = new Options().WithCollector(collectorOptions).WithExceptionHandler(eh.Invoke);
            var recorder = new SimpleMockRecorder();
            var mockClient = new Mock<ISplunkTracingHttpClient>();
            mockClient.Setup(client => client.Translate(recorder)).Throws<OverflowException>();
            
            var tracer = new Tracer(tracerOptions, recorder, mockClient.Object);

            var span = tracer.BuildSpan("test").Start();
            span.Finish();
            
            Assert.False(x);
            tracer.Flush();
            Assert.True(x);
        }
    }
}