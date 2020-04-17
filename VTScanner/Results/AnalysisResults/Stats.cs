using Newtonsoft.Json;

namespace VTScanner.Results.AnalysisResults
{
    public class Stats
    {
        [JsonProperty("confirmed-timeout")]
        public int ConfirmedTimeout { get; set; }

        [JsonProperty("failure")]
        public int Failure { get; set; }

        [JsonProperty("harmless")]
        public int Harmless { get; set; }


        [JsonProperty("malicious")]
        public int Malicious { get; set; }


        [JsonProperty("suspicious")]
        public int Suspicious { get; set; }

        [JsonProperty("timeout")]
        public int Timeout { get; set; }

        [JsonProperty("type-unsupported")]
        public int TypeUnsupported { get; set; }

        [JsonProperty("undetected")]
        public int Undetected { get; set; }

    }
}