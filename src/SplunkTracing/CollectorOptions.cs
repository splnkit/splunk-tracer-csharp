namespace SplunkTracing
{
    /// <summary>
    /// Options to configure a SplunkTracing HEC endpoint.
    /// </summary>
    public class CollectorOptions
    {
        /// <summary>
        ///     Create a new Collector Endpoint.
        /// </summary>
        /// <param name="host">satellite hostname</param>
        /// <param name="port">satellite port</param>
        /// <param name="usePlaintext">unused</param>
        public CollectorOptions(string host, int port = 8088, bool usePlaintext = false)
        {
            CollectorHost = host;
            CollectorPort = port;
            UsePlaintext = usePlaintext;
        }

        /// <summary>
        ///     Hostname of a Collector
        /// </summary>
        public string CollectorHost { get; }

        /// <summary>
        ///     Port number of a Collector
        /// </summary>
        public int CollectorPort { get; }

        /// <summary>
        ///     Currently unused
        /// </summary>
        public bool UsePlaintext { get; }

        public override string ToString()
        {
            return $"Host: {CollectorHost}, Port: {CollectorPort}, Use Plaintext: {UsePlaintext}";
        }
    }
}