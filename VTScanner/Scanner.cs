using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VTScanner.Exceptions;
using VTScanner.Helpers;
using VTScanner.ResponseCodes;
using VTScanner.Results;
using VTScanner.Results.AnalysisResults;

namespace VTScanner
{
    public class Scanner
    {
        #region Private fields

        private readonly string _apiKey = string.Empty;
        private const string _apiKeyHeaderName = "x-apikey";
        private readonly string _apiUrl = "www.virustotal.com/api/v3/";

        private readonly HttpClient _client;
        private readonly HttpClientHandler _httpClientHandler;

        private readonly JsonSerializer _serializer;

        #endregion

        #region Class Constructor

        /// <param name="fileName">File name containing the Virus Total Key inside same working directory.</param>
        public Scanner(string fileName)
        {
            this._apiKey = FileHelper.ReadApiKeyFromCurrentDirectory(fileName);

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new ArgumentNullException(nameof(_apiKey), "Could not find the Virus Total API key. Key cannot be null.");
            }

            if (_apiKey.Length < 64)
            {
                throw new ArgumentException("API key is too short.", nameof(_apiKey));
            }


            _client = new HttpClient(handler:
                _httpClientHandler = new HttpClientHandler
                {
                    AllowAutoRedirect = true
                }
            );


            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            _serializer = JsonSerializer.Create(jsonSettings);

        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Occurs when the raw JSON response is received from VirusTotal.
        /// </summary>
        public event Action<byte[]> OnRawResponseReceived;

        /// <summary>
        /// Occurs just before we send a request to VirusTotal.
        /// </summary>
        public event Action<HttpRequestMessage> OnHTTPRequestSending;

        /// <summary>
        /// Occurs right after a response has been received from VirusTotal.
        /// </summary>
        public event Action<HttpResponseMessage> OnHTTPResponseReceived;

        /// <summary>
        /// When true, we check the file size before uploading it to Virus Total. The file size restrictions are based on the Virus Total public API 3.0 documentation.
        /// </summary>
        public bool RestrictSizeLimits { get; set; }

        /// <summary>
        /// When true, we check the number of resources that are submitted to Virus Total. The limits are according to Virus Total public API 3.0 documentation.
        /// </summary>
        public bool RestrictNumberOfResources { get; set; }

        /// <summary>
        /// The maximum size (in bytes) that the Virus Total public API 3.0 supports for file uploads.
        /// </summary>
        public int FileSizeLimit { get; set; } = 33553369; //32 MB - 1063 = 33553369 it is the effective limit by virus total as it measures file size limit on the TOTAL request size, and not just the file content.

        /// <summary>
        /// The maximum size when using the large file API functionality (part of private API)
        /// </summary>
        public long LargeFileSizeLimit { get; set; } = 1024 * 1024 * 200; //200 MB


        /// <summary>
        /// The maximum number of resources you can rescan in one request.
        /// </summary>
        public int RescanBatchSizeLimit { get; set; } = 25;

        /// <summary>
        /// The maximum number of resources you can get file reports for in one request.
        /// </summary>
        public int FileReportBatchSizeLimit { get; set; } = 4;


        /// <summary>
        /// Set to false to use HTTP instead of HTTPS. HTTPS is used by default.
        /// </summary>
        public bool UseTLS { get; set; } = true;

        /// <summary>
        /// The user-agent to use when doing queries
        /// </summary>
        public string UserAgent
        {
            get => _client.DefaultRequestHeaders.UserAgent.ToString();
            set => _client.DefaultRequestHeaders.Add("User-Agent", value);
        }

        /// <summary>
        /// Get or set the proxy.
        /// </summary>
        public IWebProxy Proxy
        {
            get => _httpClientHandler.Proxy;
            set
            {
                _httpClientHandler.UseProxy = value != null;
                _httpClientHandler.Proxy = value;
            }
        }

        /// <summary>
        /// Get or set the timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get => _client.Timeout;
            set => _client.Timeout = value;
        }


        #endregion


        #region Methods

        public async Task<FileAnalysisResult> ScanFileAsync(string filePath, int delayBeforeAnalysisRetry = 5)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Could not find the file '{filePath}'.");
            }

            // Check if the file already exists or not
            string fileHash = HashHelper.GetSha256(new System.IO.FileInfo(filePath));
            var fileDescriptorResult = await RetrieveFileDescriptorAsync(fileHash).ConfigureAwait(false);

            string fileDescriptorID;
            // if there is an error, that mean file doesn't already exist
            if (fileDescriptorResult.Error != null)
            {
                Console.WriteLine(fileDescriptorResult.Error.Code);
                Console.WriteLine(fileDescriptorResult.Error.Message);

                var multipartContent = new MultipartFormDataContent
                {
                    { new ByteArrayContent(File.ReadAllBytes(filePath)), "file", Path.GetFileName(filePath) }
                };

                var result = await GetResponse<ObjectDescriptorResult>("files", HttpMethod.Post, multipartContent).ConfigureAwait(false);
                fileDescriptorID = result.FileResult.ID;
            }
            else
            {
                fileDescriptorID = fileDescriptorResult.FileResult.ID;
            }

            var analysisResult = await GetFileAnalysisAsync(fileDescriptorID).ConfigureAwait(false);

            while (analysisResult.Data.Attributes.Status.Equals(FileAnalysisResponseStatus.Queued, StringComparison.InvariantCultureIgnoreCase))
            {
                await Task.Delay(TimeSpan.FromSeconds(delayBeforeAnalysisRetry)).ConfigureAwait(false);
                analysisResult = await GetFileAnalysisAsync(fileDescriptorID).ConfigureAwait(false);
            }
            return analysisResult;
        }

        public async Task<FileAnalysisResult> GetFileAnalysisAsync(string id)
        {
            return await GetResponse<FileAnalysisResult>($"analyses/{id}", HttpMethod.Get, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Files that have been already uploaded to VirusTotal can be re-analysed without uploading them again, 
        /// you can use this endpoint (https://www.virustotal.com/api/v3/files/{id}/analyse) for that purpose. 
        /// The response is an object descriptor for the new analysis as in the POST /files endpoint. 
        /// The ID contained in the descriptor can be used with the GET /analysis/{id} endpoint to get information about the analysis.
        /// See <see cref="https://developers.virustotal.com/v3.0/reference#files-analyse"/>.
        /// </summary>
        /// <param name="id">SHA-256, SHA-1 or MD5 identifying the file.</param>
        /// <returns></returns>
        public async Task<ObjectDescriptorResult> RetrieveFileDescriptorAsync(string id)
        {
            return await GetResponse<ObjectDescriptorResult>($"files/{id}/analyse", HttpMethod.Post, null).ConfigureAwait(false);
        }

        #region Private Methods
        private async Task<T> GetResponse<T>(string url, HttpMethod method, HttpContent content)
        {
            HttpResponseMessage response = await SendRequest(url, method, content).ConfigureAwait(false);

            using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
            using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
            {
                jsonTextReader.CloseInput = false;

                SaveResponse(responseStream);

                return _serializer.Deserialize<T>(jsonTextReader);
            }
        }

        private async Task<HttpResponseMessage> SendRequest(string url, HttpMethod method, HttpContent content)
        {
            //We need this check because sometimes url is a full url and sometimes it is just an url segment
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = (UseTLS ? "https://" : "http://") + _apiUrl + url;

            HttpRequestMessage request = new HttpRequestMessage(method, url)
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation(_apiKeyHeaderName, _apiKey);

            OnHTTPRequestSending?.Invoke(request);

            HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);

            OnHTTPResponseReceived?.Invoke(response);

            if (response.StatusCode == HttpStatusCode.NoContent)
                throw new RateLimitException("You have reached the 4 requests pr. min. limit of VirusTotal");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new AccessDeniedException("You don't have access to the service. Make sure your API key is working correctly.");

            if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                throw new SizeLimitException(FileSizeLimit);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                if (!ResourcesHelper.IsReanalyseEndpoint(request.RequestUri.AbsolutePath))
                {
                    throw new Exception("API gave error code " + response.StatusCode);
                }
                else if (string.IsNullOrWhiteSpace(response.Content.ToString()))
                    throw new Exception("There were no content in the response.");
                else
                {
                    return response;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("API gave error code " + response.StatusCode);

            if (string.IsNullOrWhiteSpace(response.Content.ToString()))
                throw new Exception("There were no content in the response.");

            return response;
        }

        private void SaveResponse(Stream stream)
        {
            if (OnRawResponseReceived == null)
                return;

            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                OnRawResponseReceived(ms.ToArray());
            }

            stream.Position = 0;
        }


        #endregion



        #endregion
    }
}
