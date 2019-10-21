using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Propagation;
using Xunit;
using SplunkTracing.Collector;

namespace SplunkTracing.Tests
{
    public class SplunkTracingProtoTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var collectorOptions = new CollectorOptions("localhost", 8088, true);
            var tracerOptions = new Options("TEST").WithCollector(collectorOptions).WithAutomaticReporting(false);
            return new Tracer(tracerOptions, spanRecorder);
        }

        private SplunkTracingHttpClient GetClient(TransportOptions t = TransportOptions.JsonHttp)
        {
            var collectorOptions = new CollectorOptions("localhost", 8088, true);
            var tracerOptions = new Options("TEST").WithCollector(collectorOptions).WithAutomaticReporting(false).WithTransport(t);
            return new SplunkTracingHttpClient("http://localhost:80", tracerOptions);
        }

        [Fact]
        public void ReportShouldBeJsonWithJsonOption()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("test").Start();
            span.Finish();

            var client = GetClient(TransportOptions.JsonHttp);
            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            var report = client.BuildRequest(translatedSpans);
            Assert.Equal("application/json", report.Content.Headers.ContentType.MediaType);
            var contentString = report.Content.ReadAsStringAsync().Result;
            // Assert.Contains("test", contentString);
        }

        [Fact]
        public void ReportShouldBeBinaryWithoutJsonOption()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("test").Start();
            span.Finish();

            var client = GetClient();
            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            var report = client.BuildRequest(translatedSpans);
            // Assert.Equal("application/octet-stream", report.Content.Headers.ContentType.MediaType);
        }

        // [Fact]
        // public void ConverterShouldConvertValues()
        // {
        //     var recorder = new SimpleMockRecorder();
        //     var tracer = GetTracer(recorder);
        //     var span = tracer.BuildSpan("testOperation")
        //         .WithTag("boolTrueTag", true)
        //         .WithTag("boolFalseTag", false)
        //         .WithTag("intTag", 0)
        //         .WithTag("stringTag", "test")
        //         .WithTag("doubleTag", 0.1)
        //         .WithTag("nullTag", null)
        //         .WithTag("jsonTag", @"{""key"":""value""}")
        //         .Start();
        //     span.Finish();

        //     var client = GetClient();
            
        //     var translatedSpans = client.Translate(recorder.GetSpanBuffer());
        //     var translatedSpan = translatedSpans.Spans[0];

        //     foreach (var tag in translatedSpan.Tags)
        //     {
        //         switch (tag.Key)
        //         {
        //             case "boolTrueFlag":
        //                 Assert.True(tag.BoolValue);
        //                 break;
        //             case "boolFalseFlag":
        //                 Assert.False(tag.BoolValue);
        //                 break;
        //             case "intTag":
        //                 Assert.Equal(0, tag.IntValue);
        //                 break;
        //             case "stringTag":
        //                 Assert.Equal("test", tag.StringValue);
        //                 break;
        //             case "doubleTag":
        //                 Assert.Equal(0.1, tag.DoubleValue);
        //                 break;
        //             case "nullTag":
        //                 Assert.Equal("null", tag.StringValue);
        //                 break;
        //             case "jsonTag":
        //                 Assert.Equal(@"{""key"":""value""}", tag.JsonValue);
        //                 break;
        //             default:
        //                 continue;
        //         }
        //     }
        // }
    }
}