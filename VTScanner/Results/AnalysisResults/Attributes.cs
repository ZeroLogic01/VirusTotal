using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VTScanner.Internals.DateTimeParsers;

namespace VTScanner.Results.AnalysisResults
{
    public class Attributes
    {
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTime Date { get; set; }

        /// <summary>
        /// The scan results from each engine.
        /// </summary>
        [JsonProperty("results")]
        public Dictionary<string, ScanEngine> Results { get; set; }


        [JsonProperty("stats")]
        public Stats Stats { get; set; }


        [JsonProperty("status")]
        public string Status { get; set; }
    }
}