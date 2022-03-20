using System.Threading;
using System.Collections.Generic;
using System;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using System.Net.Http;

namespace TeamFactory.Util.Metrics
{
    public class InsecureHandler : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            return handler;
        }
    }

    public class Handler
    {
        const long intervalMs = 10000;

        private bool run = true;

        private static readonly Mutex mutex = new Mutex();

        private static readonly List<CountEntry> counts = new List<CountEntry>();

        private static string apiKey;

        public static void StartWorker(string key)
        {
            apiKey = key;
            Thread t = new Thread(new ThreadStart(Worker));
            t.IsBackground = true;
            t.Start();
        }

        public static void Inc(string metricName, Dictionary<string, string> attributes = null)
        {
            if (apiKey == "")
                return;
    
            mutex.WaitOne();
            CountEntry foundCe = null;
            foreach(CountEntry ce in counts)
            {
                if (ce.Name != metricName)
                    continue;

                if (!attributeEqual(attributes, ce.Attributes))
                    continue;

                foundCe = ce;
                break;
            }

            if (foundCe == null)
            {
                foundCe = new CountEntry();
                foundCe.Attributes = attributes;
                foundCe.Name = metricName;
                foundCe.Value = 0;

                counts.Add(foundCe);
            }

            foundCe.Value++;

            mutex.ReleaseMutex();
        }

        private static bool attributeEqual(Dictionary<string, string> a, Dictionary<string, string> b)
        {
                if (a == null && b == null)
                    return true;

                if (a == null || b == null)
                    return false;

                if (a.Count != b.Count)
                    return false;

                foreach(string key in a.Keys)
                {
                    if (!b.ContainsKey(key))
                        return false;

                    if (a[key] != b[key])
                        return false;
                }

                return true;
        }

        public static void Worker() {
            Handler h = new Handler();
            while (h.run)
            {
                h.flushMetrics();
                Thread.Sleep((int)intervalMs);
            }
        }

        private Handler()
        { }

        private async void flushMetrics()
        {
            mutex.WaitOne();

            if (Handler.counts.Count <= 0)
            {
                mutex.ReleaseMutex();
                return;
            }

            PayloadEntry pe = new PayloadEntry();
            pe.entries = new List<object>();

            foreach(CountEntry e in Handler.counts)
                pe.entries.Add(e);
            
            Handler.counts.Clear();
            
            mutex.ReleaseMutex();

            Common pc = new Common();
            pc.Interval = intervalMs;
            pc.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            pe.Common = pc;

            List<PayloadEntry> p = new List<PayloadEntry>();
            p.Add(pe);
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(p);
            Godot.GD.Print(jsonStr);

            FlurlHttp.Configure(settings => {
                settings.HttpClientFactory = new InsecureHandler();
            });

            try {
                Url url = new Url($"https://metric-api.eu.newrelic.com/metric/v1?Api-Key={apiKey}");
                IFlurlResponse response = await url
                    .SendStringAsync(System.Net.Http.HttpMethod.Post, jsonStr);
            } catch (FlurlHttpException e) {
                string respData = await e.GetResponseStringAsync();
                Godot.GD.Print(e.StatusCode, respData);
            }
        }
    }
}