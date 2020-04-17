using Newtonsoft.Json;

namespace VTScanner.Results.AnalysisResults
{
    public class FileInfo
    {
        /// <summary>
        /// MD5 hash of the resource.
        /// </summary>
        [JsonProperty("md5")]
        public string MD5 { get; set; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }


        /// <summary>
        /// SHA1 hash of the resource.
        /// </summary>
        [JsonProperty("sha1")]
        public string SHA1 { get; set; }

        /// <summary>
        /// SHA256 hash of the resource.
        /// </summary>
        [JsonProperty("sha256")] 
        public string SHA256 { get; set; }


        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }


    }
}