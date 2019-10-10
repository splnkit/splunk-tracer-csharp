# splunk-tracer-csharp
The Splunk distributed tracing library for C#

[![MIT license](http://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

# Installation
Install the package via NuGet into your solution, or use `Install-Package SplunkTracing` / `dotnet add package SplunkTracing`.

# Basic Usage
It's recommended to initialize the tracer once at the beginning of your application and assign it as the global tracer, as follows:
```c#
var tracerOptions = new Options();
var tracer = new Tracer(tracerOptions);
GlobalTracer.Register(tracer);
```

Once you've initialized a tracer, you can begin to create and emit spans.

Please refer to the [OpenTracing C# documentation](https://github.com/opentracing/opentracing-csharp) for information on how to create spans.

You can also refer to the `examples` folder for sample code and projects. 

# Advanced Usage

There's several options that can be adjusted when instantiating a `SplunkTracer`.

## `Options`
| Method | Description |
| ------ | ----------- |
| WithTags(IDictionary<string, object>) | Default tags to apply to all spans created by the tracer. |
| WithReportPeriod(TimeSpan) | How frequently the Tracer should batch and send Spans to Splunk (5s default) |
| WithReportTimeout(TimeSpan) | Timeout for sending spans to the Collector (30s default) |
| WithToken(string) | The Splunk Project Access Token |
| WithCollector(CollectorOptions) | A CollectorOptions object that specifies the host, port, and if we should use HTTPS |
| WithHttp2(bool) | If this is true, we use HTTP/2 to communicate with the Collector. We recommend you enable this option if you're on a modern version of .NET (4.6.1+ or .NET Core) |
| WithAutomaticReporting(bool) | If false, disables the automatic flushing of buffered spans. |
| WithMaxBufferedSpans(int) | The maximum amount of spans to record in a single buffer. |
| WithTransport(enum) | Which transport to use when sending spans to the Collector. |

## `CollectorOptions`
| Property | Description |
| -------- | ----------- |
| CollectorHost | The hostname of a HEC collector (e.g., `localhost`)
| CollectorPort | The port number where the HEC is listening for HTTP traffic (defaults to 8088)
| UsePlaintext | Should we use HTTP or HTTPS traffic? (Defaults to HTTPS)

The C# Tracer will prefer TLS 1.2 when available on all .NET Runtime versions, but should fall back to TLS 1.1 or 1.0 in that order.

The following is an example of overriding the Splunk Component Name and adding a new custom tag for all spans -

```csharp
var collectorOptions = new CollectorOptions("localhost");
var overrideTags = new Dictionary<string, object> 
{
    {SplunkConstants.ComponentNameKey, "test_component"},
    {"my_tag", "foobar"}
};
var tracerOptions = new Options("TEST_TOKEN").WithCollector(collectorOptions).WithTags(overrideTags);
var tracer = new Tracer(tracerOptions);
```

## Logging
This tracer uses [LibLog](https://github.com/damianh/LibLog), a transparent logging abstraction that provides built-in support for NLog, Log4Net, Serilog, and Loupe.
If you use a logging provider that isn't identified by LibLog, see [this gist](https://gist.github.com/damianh/fa529b8346a83f7f49a9) on how to implement a custom logging provider.



This library is the Splunk binding for [OpenTracing](http://opentracing.io/). See the [OpenTracing Python API](https://github.com/opentracing/opentracing-csharp) for additional detail.

## License

The Splunk Tracer for CSharp is licensed under the MIT License. Details can be found in the LICENSE file.

### Third-party libraries

This is a fork of the CSharp tracer from Lightstep, which is also licensed under the MIT License. Links to the original repository and license are below:

* [lightstep-tracer-csharp][lightstep]: [MIT][lightstep-license]

[lightstep]:                      https://github.com/lightstep/lightstep-tracer-csharp
[lightstep-license]:              https://github.com/lightstep/lightstep-tracer-csharp/blob/master/LICENSE
