﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using SplunkTracing.Logging;

namespace SplunkTracing
{
    /// <summary>
    ///     Options for configuring the SplunkTracing tracer.
    /// </summary>
    public class Options
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        ///     An identifier for the Tracer.
        /// </summary>
        public readonly ulong TracerGuid = new Random().NextUInt64();
        
        /// <summary>
        ///     API key for a SplunkTracing project.
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// If true, tracer will start reporting
        /// </summary>
        /// 
        public bool Run { get; private set; }
        /// <summary>
        ///     True if the collector connection should use HTTP/2, false otherwise.
        /// </summary>
        public bool UseHttp2 { get; private set; }

        /// <summary>
        ///     Splunk HEC endpoint endpoint configuration.
        /// </summary>
        public CollectorOptions Collector { get; private set; }

        /// <summary>
        ///     How often the reporter will send spans to a Splunk HEC endpoint.
        /// </summary>
        public TimeSpan ReportPeriod { get; private set; }

        /// <summary>
        ///     Timeout for sending spans to a Splunk HEC endpoint.
        /// </summary>
        public TimeSpan ReportTimeout { get; private set; }
        
        /// <summary>
        ///     Maximum amount of spans to buffer in a single report. 
        /// </summary>
        public int ReportMaxSpans { get; private set;}

        /// <summary>
        ///     Tags that should be applied to each span generated by this tracer.
        /// </summary>
        public IDictionary<string, object> Tags { get; private set; }

        /// <summary>
        ///     If the tracer should send JSON rather than binary protobufs to the collector.
        /// </summary>
        public TransportOptions Transport { get; private set; }

        /// <summary>
        ///     Determines if tracer should report meta events to SplunkTracing
        /// </summary>
        public Boolean EnableMetaEventLogging { get; internal set; }

        public Action<Exception> ExceptionHandler { get; internal set; }
        public Boolean ExceptionHandlerRegistered { get; internal set; }

        public Options WithMetaEventLogging()
        {
            _logger.Debug("Enabling Meta Events");
            EnableMetaEventLogging = true;
            return this;
        }

        public Options WithToken(string token)
        {
            _logger.Debug($"Setting access token to {token}");
            AccessToken = token;
            return this;
        }

        public Options WithHttp2()
        {
            _logger.Debug("Enabling HTTP/2 support.");
            UseHttp2 = true;
            return this;
        }

        public Options WithCollector(CollectorOptions options)
        {
            _logger.Debug($"Setting collector to {options}");
            Collector = options;
            return this;
        }

        public Options WithReportPeriod(TimeSpan period)
        {
            _logger.Debug($"Setting reporting period to {period}");
            ReportPeriod = period;
            return this;
        }

        public Options WithReportTimeout(TimeSpan timeout)
        {
            _logger.Debug($"Setting report timeout to {timeout}");
            ReportTimeout = timeout;
            return this;
        }

        public Options WithTags(IDictionary<string, object> tags)
        {
            _logger.Debug($"Setting default tags to: {tags.Select(kvp => $"{kvp.Key}:{kvp.Value},")}");
            Tags = MergeTags(tags);
            return this;
        }

        public Options WithAutomaticReporting(bool shouldRun)
        {
            _logger.Debug($"Setting automatic reporting to {shouldRun}");
            Run = shouldRun;
            return this;
        }

        public Options WithMaxBufferedSpans(int count)
        {
            _logger.Debug($"Setting max spans per buffer to {count}");
            ReportMaxSpans = count;
            return this;
        }

        public Options WithTransport(TransportOptions transport)
        {
            _logger.Debug($"Setting JSON reports to {transport}");
            Transport = transport;
            return this;
        }

        public Options WithExceptionHandler(Action<Exception> handler)
        {
            _logger.Debug($"Registering exception handler {handler}");
            ExceptionHandler = handler;
            ExceptionHandlerRegistered = true;
            return this;
        }
        
        /// <summary>
        ///     Creates a new set of options for the SplunkTracing tracer.
        /// </summary>
        /// <param name="token">Project access token, if required.</param>
        public Options(string token = "")
        {
            Tags = InitializeDefaultTags();
            ReportPeriod = TimeSpan.FromMilliseconds(5000);
            ReportTimeout = TimeSpan.FromSeconds(30);
            AccessToken = token;
            Collector = new CollectorOptions("localhost", 8088, false);
            UseHttp2 = false;
            Run = true;
            ReportMaxSpans = int.MaxValue;
            Transport = TransportOptions.BinaryProto;
            EnableMetaEventLogging = false;
            ExceptionHandlerRegistered = false;
        }
        
        private IDictionary<string, object> MergeTags(IDictionary<string, object> input)
        {
            var attributes = InitializeDefaultTags();
            var mergedAttributes = new Dictionary<string, object>(input);
            foreach (var item in attributes)
            {
                if (!mergedAttributes.ContainsKey(item.Key))
                {                    
                    mergedAttributes.Add(item.Key, item.Value);
                }
            }

            return mergedAttributes;
        }
        
        private IDictionary<string, object> InitializeDefaultTags()
        {
            var attributes = new Dictionary<string, object>
            {
                [SplunkTracingConstants.TracerPlatformKey] = SplunkTracingConstants.TracerPlatformValue,
                [SplunkTracingConstants.TracerPlatformVersionKey] = GetPlatformVersion(),
                [SplunkTracingConstants.TracerVersionKey] = GetTracerVersion(),
                [SplunkTracingConstants.ComponentNameKey] = GetComponentName(),
                [SplunkTracingConstants.HostnameKey] = GetHostName(),
                [SplunkTracingConstants.CommandLineKey] = GetCommandLine()
            };
            return attributes;
        }

        private static string GetTracerVersion()
        {
            return typeof(SplunkTracing.Tracer).Assembly.GetName().Version.ToString();
        }

        private static string GetComponentName()
        {
            var entryAssembly = "";
            try
            {
                entryAssembly = Assembly.GetEntryAssembly().GetName().Name;
            }
            catch (NullReferenceException)
            {
                // could not get assembly name, possibly because we're running a test
                entryAssembly = "unknown";
            }  
            return entryAssembly;
        }

        private static string GetPlatformVersion()
        {
#if NET45
            var version = "";
            version = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
            // in unit testing scenarios, GetEntryAssembly returns null so make sure we aren't blowing up if this isn't available
            if (version == null && Assembly.GetEntryAssembly() != null)
            {
                TargetFrameworkAttribute tfa = (TargetFrameworkAttribute) Assembly.GetEntryAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute))
                    .SingleOrDefault();
                if (tfa != null)
                {
                    version = tfa.FrameworkName;    
                }
            }
            return version;
#elif NETSTANDARD2_0
            return Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
#else
            return "Unknown Framework Version";
#endif
        }


        private static string GetHostName()
        {
           return Environment.MachineName;
        }

        private static string GetCommandLine()
        {
            return Environment.CommandLine;
        }
    }
}