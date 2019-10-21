using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Json;
using SplunkTracing.Logging;

namespace SplunkTracing.Collector {

  public sealed partial class Reporter {

    public Reporter() {
      OnConstruction();
    }

    partial void OnConstruction();

    public Reporter(Reporter other) : this() {
      reporterId_ = other.reporterId_;
    }

    public Reporter Clone() {
      return new Reporter(this);
    }

    /// <summary>Field number for the "reporter_id" field.</summary>
    public const int ReporterIdFieldNumber = 1;
    private ulong reporterId_;

    public ulong ReporterId {
      get { return reporterId_; }
      set {
        reporterId_ = value;
      }
    }

    private readonly Dictionary<string, object> tags_ = new Dictionary<string, object>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Dictionary<string, object> Tags {
      get { return tags_; }
    }

    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    public override string ToString() {
      return reporterId_.ToString();
    }


    public int CalculateSize() {
      int size = 0;
      return size;
    }

  }

  public sealed partial class Auth {

    public Auth() {
      OnConstruction();
    }

    partial void OnConstruction();

    public Auth(Auth other) : this() {
      accessToken_ = other.accessToken_;
    }

    public Auth Clone() {
      return new Auth(this);
    }

    /// <summary>Field number for the "access_token" field.</summary>
    public const int AccessTokenFieldNumber = 1;
    private string accessToken_ = "";

    public string AccessToken {
      get { return accessToken_; }
      set {
        accessToken_ = value;
      }
    }

    public bool Equals(Auth other) {
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    public override string ToString() {
      // return pb::JsonFormatter.ToDiagnosticString(this);
      return "string";
    }

    public int CalculateSize() {
      int size = 0;

      return size;
    }

  }

  public sealed partial class ReportRequest {

    public ReportRequest() {
      OnConstruction();
    }

    partial void OnConstruction();


    /// <summary>Field number for the "reporter" field.</summary>
    private global::SplunkTracing.Collector.Reporter reporter_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SplunkTracing.Collector.Reporter Reporter {
      get { return reporter_; }
      set {
        reporter_ = value;
      }
    }

    /// <summary>Field number for the "auth" field.</summary>

    private global::SplunkTracing.Collector.Auth auth_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SplunkTracing.Collector.Auth Auth {
      get { return auth_; }
      set {
        auth_ = value;
      }
    }

    private readonly List<string> spans_ = new List<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public List<string> Spans {
      get { return spans_; }
    }

    public override string ToString() {
      return string.Join("\n", spans_);
    }


    public int CalculateSize() {
      return spans_.Count;
    }

  }

  
  public class Span  
  { 
    private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();


    public static string Parse(SpanData span, Reporter reporter)  {
        var event_obj_list = new List<string>();
        var epochZero = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero); 
        var convertedSpan = new JsonObject();
        convertedSpan.Add("time", (span.StartTimestamp.UtcTicks - epochZero.UtcTicks) / 10000000.0);
        convertedSpan.Add("sourcetype", "splunktracing:span");
        convertedSpan.Add("event", new JsonObject() {
          ["component_name"] = reporter.Tags["component_name"].ToString(),
          ["operation_name"] = span.OperationName,
          ["tracer_platform_version"] = reporter.Tags["tracer_platform_version"].ToString(),
          ["tracer_platform"] = reporter.Tags["tracer_platform"].ToString(),
          ["tracer_version"] = reporter.Tags["tracer_version"].ToString(),
          ["trace_id"] = Utilities.IdToHex(span.Context.TraceId),
          ["span_id"] = Utilities.IdToHex(span.Context.SpanId),
          ["parent_span_id"] = Utilities.IdToHex(span.Context.ParentSpanId),
          ["device"] = reporter.Tags["device"].ToString(),
          ["guid"] = reporter.ReporterId,
          ["timestamp"] = (span.StartTimestamp.UtcTicks - epochZero.UtcTicks) / 10000000.0,  //UtcTicks
          ["duration"] = Convert.ToUInt64(Math.Abs(span.Duration.Ticks) / 10),
          ["tags"] = new JsonObject(DictToJson(span.Tags)), //span.Tags
          ["baggage"] = new JsonObject(StringDictToJson(span.Context.GetBaggage())), // span.Context.GetBaggage()
          }
        );
        event_obj_list.Add(convertedSpan.ToString());
        foreach (var log in span.LogData)
        {
          var log_obj = new JsonObject();
          log_obj.Add("time", (log.Timestamp.UtcTicks - epochZero.UtcTicks) / 10000000.0);
          log_obj.Add("sourcetype", "splunktracing:log");
          log_obj.Add("event", new JsonObject() {
            ["component_name"] = reporter.Tags["component_name"].ToString(),
            ["operation_name"] = span.OperationName,
            ["tracer_platform_version"] = reporter.Tags["tracer_platform_version"].ToString(),
            ["tracer_platform"] = reporter.Tags["tracer_platform"].ToString(),
            ["tracer_version"] = reporter.Tags["tracer_version"].ToString(),
            ["trace_id"] = Utilities.IdToHex(span.Context.TraceId),
            ["span_id"] = Utilities.IdToHex(span.Context.SpanId),
            ["parent_span_id"] = Utilities.IdToHex(span.Context.ParentSpanId),
            ["device"] = reporter.Tags["device"].ToString(),
            ["guid"] = reporter.ReporterId,
            ["timestamp"] = (log.Timestamp.UtcTicks - epochZero.UtcTicks) / 10000000.0,
            ["tags"] = new JsonObject(DictToJson(span.Tags)),
            ["baggage"] = new JsonObject(StringDictToJson(span.Context.GetBaggage())), 
            ["fields"] = new JsonObject(LogFieldsToJson(log.Fields)),
          });
          event_obj_list.Add(log_obj.ToString());
        }

        return string.Join("\n", event_obj_list);
    }
    public static IEnumerable<KeyValuePair<string, JsonValue>> DictToJson(IDictionary<string, object> thing)
    {
        foreach (var item in thing)
        {
          KeyValuePair<string,JsonValue> json_attr = new KeyValuePair<string,JsonValue>(item.Key, item.Value.ToString());
          yield return json_attr;
        }
    }
    public static IEnumerable<KeyValuePair<string, JsonValue>> StringDictToJson(IDictionary<string, string> thing)
    {
        foreach (var item in thing)
        {
          KeyValuePair<string,JsonValue> json_attr = new KeyValuePair<string,JsonValue>(item.Key, item.Value.ToString());
          yield return json_attr;
        }
    }
    public static IEnumerable<KeyValuePair<string, JsonValue>> LogFieldsToJson(IEnumerable<KeyValuePair<string, object>> frank)
    {
        foreach (var flink in frank)
        {
            KeyValuePair<string,JsonValue> json_attr = new KeyValuePair<string,JsonValue>(flink.Key.ToString(), flink.Value.ToString());
            yield return json_attr;
        }
    }
  }

  [DataContract]  
  public class ReportResponse  
  {  
    [DataMember]  
    public string text;  

    [DataMember]  
    public int code; 


    public static ReportResponse Parse(string json)  {  
        var deserializedResponse = new ReportResponse();  
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));  
        var ser = new DataContractJsonSerializer(deserializedResponse.GetType());  
        deserializedResponse = ser.ReadObject(ms) as ReportResponse;  
        ms.Close();  
        return deserializedResponse;  
    }
  }
}