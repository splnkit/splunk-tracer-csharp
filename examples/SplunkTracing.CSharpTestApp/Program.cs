using System;
using System.IO;
using System.Threading;
using OpenTracing.Util;
using global::Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole;

namespace SplunkTracing.CSharpTestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            // var collectorOptions = new CollectorOptions("input-prd-p-5s7q2gt8x3qv.cloud.splunk.com"); "0b47304e-408c-47a5-8daa-3b5e424ef244"
            var collectorOptions = new CollectorOptions(host:"localhost", usePlaintext:true);    
            var tracer = new Tracer(new Options("08243c00-a31b-499d-9fae-776b41990997").WithCollector(collectorOptions));
            GlobalTracer.Register(tracer);
            
            for (var i = 0; i < 500; i++)
                using (var scope = tracer.BuildSpan("testParent").WithTag("testSpan", "true").StartActive(true))
                {
                    scope.Span.Log("test");
                    tracer.ActiveSpan.Log($"iteration {i}");
                    
                    Thread.Sleep(new Random().Next(5, 10));
                    var innerSpan = tracer.BuildSpan("childSpan").Start();
                    innerSpan.SetTag("innerTestTag", "true");
                    
                    Thread.Sleep(new Random().Next(10, 20));
                    innerSpan.Finish();
                }
            tracer.Flush();
            Console.ReadKey();
        }
        
    }
}