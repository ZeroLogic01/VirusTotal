using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTScanner.Results.AnalysisResults
{
    public class Meta
    {
        [JsonProperty("file_info")]
        public FileInfo FileInfo { get; set; }
    }
}
