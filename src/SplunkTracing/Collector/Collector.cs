using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
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
      // return pb::JsonFormatter.ToDiagnosticString(this);
      return "string";
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

    // public ReportRequest(ReportRequest other) : this() {
    //   reporter_ = other.reporter_ != null ? other.reporter_.Clone() : null;
    //   auth_ = other.auth_ != null ? other.auth_.Clone() : null;
    //   spans_ = other.spans_.Clone();
    // }

    // public ReportRequest Clone() {
    //   return new ReportRequest(this);
    // }

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


    private readonly List<SpanData> spans_ = new List<SpanData>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public List<SpanData> Spans {
      get { return spans_; }
    }

    public override string ToString() {
      // return pb::JsonFormatter.ToDiagnosticString(this);
      List<string> report_obj_list = new List<string>();
      report_obj_list.Add("a");
      // string[] report_obj_array = report_obj_list.ToArray
      string reportString = string.Join("\n", report_obj_list);
      return "string";
    }


    public int CalculateSize() {
      int size = 0;
      // if (reporter_ != null) {
      //   size += 1 + pb::CodedOutputStream.ComputeMessageSize(Reporter);
      // }
      // if (auth_ != null) {
      //   size += 1 + pb::CodedOutputStream.ComputeMessageSize(Auth);
      // }
      // size += spans_.CalculateSize(_repeated_spans_codec);
      // if (TimestampOffsetMicros != 0L) {
      //   size += 1 + pb::CodedOutputStream.ComputeInt64Size(TimestampOffsetMicros);
      // }
      // if (internalMetrics_ != null) {
      //   size += 1 + pb::CodedOutputStream.ComputeMessageSize(InternalMetrics);
      // }
      // if (_unknownFields != null) {
      //   size += _unknownFields.CalculateSize();
      // }
      return size;
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
//   public sealed partial class ReportResponse {

//     public ReportResponse() {
//       OnConstruction();
//     }

//     partial void OnConstruction();

//     public ReportResponse(ReportResponse other) : this() {

//     }

//     private readonly int code_ = new int;

//     [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//     public int Code {
//       get { return code_; }
//     }

//     private readonly string text_ = new string;

//     [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//     public int Text {
//       get { return text_; }
//     }

//     public ReportResponse Clone() {
//       return new ReportResponse(this);
//     }


//     public override int GetHashCode() {
//       int hash = 1;
//       return hash;
//     }

//     public override string ToString() {

//       return text_;
//     }

//     public int CalculateSize() {
//       int size = 0;
//       return size;
//     }
//     public static ReportResponse Parse(string response) {

//       return ReportResponse(response);
//     }

//   }

// }
}