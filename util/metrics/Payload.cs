using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamFactory.Util.Metrics
{
    public class Common
    {
        [JsonProperty("interval.ms")]
        public long Interval { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Attributes { get; set; }
    }

    public class CountEntry
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("type")]
        public string Type { 
            get { return "count"; }
        }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Attributes { get; set; }
    }

    public class PayloadEntry
    {
        [JsonProperty("common")]
        public Common Common { get; set; }

        [JsonProperty("metrics")]
        public List<object> entries { get; set; }
    }
}