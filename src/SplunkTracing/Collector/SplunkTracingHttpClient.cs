using System;
using System.Diagnostics; 
using System.IO;
using System.IO.Compression;
// using System.IO.Compression.GzipStream;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text; 
using System.Threading.Tasks;
using SplunkTracing.Logging;


namespace SplunkTracing.Collector
{
    /// <summary>
    ///     Contains methods to communicate to a Splunk HEC collector over HTTP.
    /// </summary>
    public class SplunkTracingHttpClient : ISplunkTracingHttpClient
    {
        private readonly Options _options;
        private HttpClient _client;
        private readonly string _url;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        ///     Create a new client.
        /// </summary>
        /// <param name="collectorUrl">URL to send results to.</param>
        /// <param name="options">An <see cref="Options" /> object.</param>
        public SplunkTracingHttpClient(string url, Options options)
        {
            _url = url;
            _options = options;
            _client = new HttpClient() {Timeout = _options.ReportTimeout};
        }

        internal HttpRequestMessage CreateCompressedRequest(ReportRequest report)
        {
            // var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
            // var jsonReport = jsonFormatter.Format(report);
            var jsonReport = CompressRequestContent(report);
            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = _options.UseHttp2 ? new Version(2, 0) : new Version(1, 1),
                Content = new ByteArrayContent(jsonReport)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentEncoding.Add("gzip");
            return request;
        }


        internal HttpRequestMessage BuildRequest(ReportRequest report)
        {
            HttpRequestMessage requestMessage = CreateCompressedRequest(report);

            // add HEC access token to request header
            requestMessage.Content.Headers.Add("Authorization", "Splunk " + report.Auth.AccessToken);

            return requestMessage;

        }

        internal byte[] CompressRequestContent(ReportRequest report)
        {
            string requestMessage = CreateStringRequest(report);
            byte[] requestBytes = Encoding.Unicode.GetBytes(requestMessage);

            using (MemoryStream memoryStream = new MemoryStream()) {
                using (System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(memoryStream, CompressionMode.Compress, true)) {
                    gZipStream.Write(requestBytes, 0, requestBytes.Length);
                }
            return memoryStream.ToArray();
            }

        }

        internal string CreateStringRequest(ReportRequest report)
        {
            
            return report.ToString();

        }



        /// <summary>
        ///     Send a report of spans to the Splunk Collector.
        /// </summary>
        /// <param name="report">An <see cref="ReportRequest" /></param>
        /// <returns>A <see cref="ReportResponse" />. This is usually not very interesting.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task<ReportResponse> SendReport(ReportRequest report)
        {
            // force net45 to attempt tls12 first and fallback appropriately
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            var requestMessage = BuildRequest(report);

            ReportResponse responseValue;

            try
            {
                var response = await _client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                responseValue = ReportResponse.Parse(responseData);
                _logger.Debug($"Report HTTP Response {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.WarnException("Exception caught while sending report, resetting HttpClient", ex);
                _client.Dispose();
                _client = new HttpClient {Timeout = _options.ReportTimeout};
                throw;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _logger.WarnException("Timed out sending report to collector", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unknown error sending report.", ex);
                throw;
            }
            
            return responseValue;
        }

        /// <summary>
        ///     Translate SpanData to a JSONHttp ReportRequest for sending to the Collector.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        public ReportRequest Translate(ISpanRecorder spanBuffer)
        {
            _logger.Debug($"Serializing {spanBuffer.GetSpans().Count()} spans.");
            var timer = new Stopwatch();
            timer.Start();

            var request = new ReportRequest
            {
                Reporter = new Reporter
                {
                    ReporterId = _options.TracerGuid
                },
                Auth = new Auth {AccessToken = _options.AccessToken}
            };
            _options.Tags.ToList().ForEach(t => request.Reporter.Tags.Add(t.Key,t.Value));
            spanBuffer.GetSpans().ToList().ForEach(span => {
                try 
                {
                    request.Spans.Add(span);
                    // var serializer = new JavaScriptSerializer();                   //////////////
                    // var serializedResult = serializer.Serialize(RegisteredUsers); //////////////
                }
                catch (Exception ex)
                {
                    _logger.WarnException("Caught exception converting spans.", ex);
                    spanBuffer.DroppedSpanCount++;
                }
            });

            timer.Stop();
            _logger.Debug($"Serialization complete in {timer.ElapsedMilliseconds}ms. Request size: {request.CalculateSize()}b.");
            
            return request;
        }
    }
}