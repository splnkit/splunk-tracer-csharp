using System.Threading.Tasks;

namespace SplunkTracing.Collector
{
    public interface ISplunkTracingHttpClient
    {
        /// <summary>
        ///     Send a report of spans to the Splunk Collector.
        /// </summary>
        /// <param name="report">An <see cref="ReportRequest" /></param>
        /// <returns>A <see cref="ReportResponse" />. This is usually not very interesting.</returns>
        Task<ReportResponse> SendReport(ReportRequest report);

        /// <summary>
        ///     Translate SpanData to a protobuf ReportRequest for sending to the Collector.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        ReportRequest Translate(ISpanRecorder spanBuffer);
    }
}