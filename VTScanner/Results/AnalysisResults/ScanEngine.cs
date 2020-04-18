using Newtonsoft.Json;
using System;
using VTScanner.Internals.DateTimeParsers;

namespace VTScanner.Results.AnalysisResults
{
    public class ScanEngine
    {

        [JsonProperty("category")]
        public string Category { get; set; }


        [JsonProperty("engine_name")]
        public string EngineName { get; set; }


        [JsonProperty("engine_update")]
        [JsonConverter(typeof(YearMonthDayConverter))]
        public DateTime EngineUpdate { get; set; }


        [JsonProperty("engine_version")]
        public string EngineVersion { get; set; }


        [JsonProperty("method")]
        public string Method { get; set; }


        [JsonProperty("result")]
        public string Result { get; set; }
    }
}