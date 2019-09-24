using System;

namespace SplunkTracing
{
    /// <summary>
    /// Options for the transport used to send spans to Splunk.
    /// </summary>
    [Flags]
    public enum TransportOptions
    {
        /// <summary>
        /// JSON protobuf encoding over HTTP
        /// </summary>
        JsonProto
    }
}