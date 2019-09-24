namespace SplunkTracing
{
    /// <summary>
    ///     Constants and other values used by SplunkTracing.
    /// </summary>
    public static class SplunkTracingConstants
    {
        public static readonly string ParentSpanGuidKey = "parent_span_guid";
        public static readonly string GuidKey = "guid";
        public static readonly string HostnameKey = "hostname";
        public static readonly string ComponentNameKey = "component_name";
        public static readonly string CommandLineKey = "command_line";

        public static readonly string TracerPlatformKey = "tracer_platform";
        public static readonly string TracerPlatformValue = "csharp";
        public static readonly string TracerPlatformVersionKey = "tracer_platform_version";
        public static readonly string TracerVersionKey = "tracer_version";

        public static readonly string CollectorReportPath = "services/collector";

        public static class MetaEvent {
            public static readonly string MetaEventKey = "meta_event";
            public static readonly string PropagationFormatKey = "propagation_format";
            public static readonly string TraceIdKey = "trace_id";
            public static readonly string SpanIdKey = "span_id";
            public static readonly string TracerGuidKey = "tracer_guid";
            public static readonly string ExtractOperation = "extract_span";
            public static readonly string InjectOperation = "inject_span";
            public static readonly string SpanStartOperation = "span_start";
            public static readonly string SpanFinishOperation = "span_finish";
            public static readonly string TracerCreateOperation = "tracer_create";
        }
        
    }
}