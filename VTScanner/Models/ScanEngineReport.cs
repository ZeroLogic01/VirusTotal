using Newtonsoft.Json;

namespace VTScanner.Models
{
    public class ScanEngineReport
    {
        [JsonProperty("file-name")]
        public string FileName { get; set; }

        [JsonProperty("engine_name")]
        public string EngineName { get; set; }

        [JsonProperty("is-detected")]
        public int IsDetected { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
