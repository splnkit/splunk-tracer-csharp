using SplunkTracing.CSharpAspectTestApp.Aspects;
using SplunkTracing;
using OpenTracing.Util;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplunkTracing.CSharpAspectTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // create your tracer options, initialize it, assign it to the GlobalTracer
            var splKey = Environment.GetEnvironmentVariable("SPL_KEY");
            var splSettings = new CollectorOptions("localhost");
            var splOptions = new Options(splKey).WithCollector(lsSettings);
            var tracer = new Tracer(splOptions);
            
            GlobalTracer.Register(tracer);

            // do some work in parallel, this work also includes awaited calls
            Parallel.For(1, 100, i => DoThing(i));

            // block until you enter a key
            Console.ReadKey();
        }

        [Traceable]
        static void DoThing(int idx)
        {
            GlobalTracer.Instance.ActiveSpan.SetTag("args", idx);
            var client = new HttpWorker();
            client.Get($"https://jsonplaceholder.typicode.com/todos/{idx}");
        }

    }
}
