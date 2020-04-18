using Newtonsoft.Json;

namespace VTScanner.Internals
{
    internal class LargeFileUpload
    {
        [JsonProperty("data")]
        public string UploadUrl { get; set; }
    }
}
