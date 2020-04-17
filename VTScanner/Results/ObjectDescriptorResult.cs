using Newtonsoft.Json;
namespace VTScanner.Results
{
    public class ObjectDescriptorResult
    {
        [JsonProperty("data")]
        public ObjectDescriptor FileResult { set; get; }

        [JsonProperty("error")]
        public Error Error { set; get; }
    }
}
