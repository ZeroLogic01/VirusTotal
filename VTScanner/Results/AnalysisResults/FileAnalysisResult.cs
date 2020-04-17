using Newtonsoft.Json;
namespace VTScanner.Results.AnalysisResults
{
    public class FileAnalysisResult 
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
