﻿using System;

namespace SplunkTracing
{
    /// <summary>
    ///     Utilities and other helpers.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Get a random uint64.
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static ulong NextUInt64(this Random rand)
        {
            var buffer = new byte[sizeof(ulong)];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static bool IsNotMetaSpan(Span span) 
        {
            return !span.Tags.ContainsKey(SplunkTracingConstants.MetaEvent.MetaEventKey);
        }

        public static string IdToHex(string uid) 
        {
            return string.Format("{0:x}", Convert.ToUInt64(uid));
        }
    }
}