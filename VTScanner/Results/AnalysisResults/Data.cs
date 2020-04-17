using Newtonsoft.Json;

namespace VTScanner.Results.AnalysisResults
{
    public class Data
    {
        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}