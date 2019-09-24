﻿using System;
using System.Collections.Generic;

namespace SplunkTracing
{
    /// <summary>
    ///     Records spans generated by the tracer. Used to batch/send spans to a collector.
    /// </summary>
    public interface ISpanRecorder
    {
        /// <summary>
        /// The start (creation) time of the span buffer.
        /// </summary>
        DateTime ReportStartTime { get; }
        /// <summary>
        /// The finishing (flushing) time of the span buffer.
        /// </summary>
        DateTime ReportEndTime { get; set; }
        /// <summary>
        /// The count of dropped spans for this, or a prior, span buffer.
        /// </summary>
        int DroppedSpanCount { get; set; }
        
        /// <summary>
        /// Saves a span.
        /// </summary>
        /// <param name="span"></param>
        void RecordSpan(SpanData span);

        /// <summary>
        /// Returns this instance of the span buffer.
        /// </summary>
        /// <returns></returns>
        ISpanRecorder GetSpanBuffer();

        /// <summary>
        /// Clears the span record.
        /// </summary>
        void ClearSpanBuffer();

        /// <summary>
        /// Increments the dropped span count.
        /// </summary>
        /// <param name="count"></param>
        void RecordDroppedSpans(int count);

        /// <summary>
        /// Gets the spans stored in the buffer.
        /// </summary>
        /// <returns></returns>
        IEnumerable<SpanData> GetSpans();
    }
}