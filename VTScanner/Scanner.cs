using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VTScanner.Exceptions;
using VTScanner.Helpers;
using VTScanner.Internals;
using VTScanner.Models;
using VTScanner.ResponseCodes;
using VTScanner.Results;
using VTScanner.Results.AnalysisResults;

namespace VTScanner
{
    public sealed class Scanner : IDisposable
    {
        #region Private fields

        private const string _apiUrl = "www.virustotal.com/api/v3/";
        private const string _apiKeyHeaderName = "x-apikey";
        private readonly string _apiKey = string.Empty;

        private readonly HttpClient _client;
        private readonly HttpClientHandler _httpClientHandler;

        private JsonSerializer _serializer;
        private CancellationToken _cancellationToken;

        #endregion

        #region Class Constructor

        /// <param name="fileName">File name containing the Virus Total Key inside same working directory.</param>
        public Scanner(string fileName, CancellationToken cancellationToken)
        {
            _apiKey = FileHelper.ReadApiKeyFromCurrentDirectory(fileName);
            _cancellationToken = cancellationToken;

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
        /// Occurs when some progress made.
        /// </summary>
        public event Action<string> OnProgressChanged;

        /// <summary>
        /// The maximum size (in bytes) that the Virus Total public API 3.0 supports for NORMAL file uploads. For uploading larger files see the GET /files/upload_url endpoint.
        /// </summary>
        public int FileSizeLimit { get; set; } = 33553369; //32 MB - 1063 = 33553369 it is the effective limit by virus total as it measures file size limit on the TOTAL request size, and not just the file content.

        /// <summary>
        /// Number of seconds to wait before retry fetching file analysis again.
        /// </summary>
        public int DelayInSecondsBeforeAnalysisRetry { get; set; } = 5;

        /// <summary>
        /// Restrict maximum size (200MB) when uploading to the URL. 
        /// Notice that files larger than 200MBs tend to be bundles of some sort, (compressed files, ISO images, etc.) in these cases it makes sense to upload the inner individual files instead for several reasons, as an example:
        /// <para>1. Engines tend to have performance issues on big files(timeouts, some may not even scan them).</para> 
        /// <para>2. Some engines are not able to inspect certain file types whereas they will be able to inspect the inner files if submitted.</para> 
        /// <para>3. When scanning a big bundle you lose context on which specific inner file is causing the detection.</para> 
        /// </summary>
        public long LargeFileSizeLimit { get; set; } = 1024 * 1024 * 200 - 1063; //200 MB - 1063 = 209,714,137‬ it is the effective limit by virus total as it measures file size limit on the TOTAL request size, and not just the file content.


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
        /// Get or set the timeout. This is important in order to upload large files. 
        /// <para>Note: <see cref="_client"/> will timeout when uploading large files to the VT API.
        /// So need to set it to <see cref="System.Threading.Timeout.Infinite"/> in order to process large files.</para>
        /// </summary>
        public TimeSpan Timeout
        {
            get => _client.Timeout;
            set => _client.Timeout = value;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Scan the file &amp; generate output files containing list of <see cref="ScanEngineReport"/>.
        /// </summary>
        /// <param name="filePath">The file to be scanned.</param>
        /// <returns></returns>
        public async Task ScanFileAndGenrateOutputAsync(string filePath)
        {
            await ScanFileAndGenrateOutputAsync(new System.IO.FileInfo(filePath)).ConfigureAwait(false);
        }

        /// <summary>
        /// Scan the file &amp; generate output files containing list of <see cref="ScanEngineReport"/>.
        /// </summary>
        /// <param name="file">The file to be scanned.</param>
        public async Task ScanFileAndGenrateOutputAsync(System.IO.FileInfo file)
        {
            var fileScanSummary = await GetScanEnginesReportListAsync(file).ConfigureAwait(false);

            if (fileScanSummary.Count > 0)
            {
                FileHelper fileWriter = new FileHelper();
                Task<string> t1 = fileWriter.WriteFileScanResultToTextFileAsync(file.FullName, fileScanSummary, _cancellationToken);
                Task<string> t2 = fileWriter.WriteFileScanResultToJsonFileAsync(file.FullName, fileScanSummary, _cancellationToken);
                await Task.WhenAll(t1, t2).ConfigureAwait(false);

                string textFileCreated = await t1;
                string jsonFileCreated = await t2;

                ShowProgress($"{textFileCreated} is generated.");
                ShowProgress($"{jsonFileCreated} is generated.");
            }
        }

        /// <summary>
        /// Scans the file &amp; extracts the list of <see cref="ScanEngineReport"/> from, the data received from the Virus Total API.
        /// If you want to save the list of <see cref="ScanEngineReport"/> to the output files, 
        /// use <see cref="ScanFileAndGenrateOutputAsync(string)"/> instead.
        /// </summary>
        /// <param name="file">The file to be scanned.</param>
        /// <returns>List of <see cref="ScanEngineReport"/> objects.</returns>
        public async Task<List<ScanEngineReport>> GetScanEnginesReportListAsync(System.IO.FileInfo file)
        {
            var fileAnalysisResult = await GetFileAnalysisResultAsync(file);
            List<ScanEngineReport> reports = new List<ScanEngineReport>();
            if (fileAnalysisResult != null)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileAnalysisResult.Data.Attributes.Results)
                {
                    int isVirusDetected = scan.Value.Category.Equals(nameof(Stats.Suspicious), StringComparison.OrdinalIgnoreCase) ||
                                    scan.Value.Category.Equals(nameof(Stats.Malicious), StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                    // if result is null, choose custom text message or category as description
                    string description = scan.Value.Result ??
                        (scan.Value.Category.Equals(@"type-unsupported", StringComparison.OrdinalIgnoreCase)
                        ? "Unable to process file type" : scan.Value.Category);

                    reports.Add(
                        new ScanEngineReport
                        {
                            FileName = file.FullName,
                            EngineName = scan.Key,
                            IsDetected = isVirusDetected,
                            Description = description
                        });
                }
            }
            return reports;
        }

        /// <summary>
        /// This is the actual method scanning given file through the Virus Total v3 API. Call this method if you want
        /// to get a detailed analysis result <see cref="FileAnalysisResult"/> from the API.
        /// </summary>
        /// <param name="file">File to be scanned</param>
        /// <returns></returns>
        public async Task<FileAnalysisResult> GetFileAnalysisResultAsync(System.IO.FileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"Could not find the file '{file}'.");
            }

            ShowProgress($"Calculating SHA-256 Hash of {file}.");
            // Check if the file already exists or not
            string fileHash = HashHelper.GetSha256(file.FullName);
            return await GetFileAnalysisResultAsync(file, fileHash).ConfigureAwait(false);
        }

        /// <summary>
        /// This is the actual method scanning given file through the Virus Total v3 API. Call this method if you want
        /// to get a detailed analysis result <see cref="FileAnalysisResult"/> from the API.
        /// </summary>
        /// <param name="file">The file to be scanned.</param>
        /// <param name="fileHash">Hash of the file. Can be a MD5, SHA1 or SHA256 hash.</param>
        /// <returns></returns>
        public async Task<FileAnalysisResult> GetFileAnalysisResultAsync(System.IO.FileInfo file, string fileHash)
        {
            var fileDescriptorResult = await RetrieveFileDescriptorAsync(fileHash).ConfigureAwait(false);

            string fileDescriptorID;
            // if there is an error, that mean file doesn't already exist
            if (fileDescriptorResult.Error != null)
            {
                if (!file.Exists)
                {
                    throw new FileNotFoundException($"Could not find the file '{file}'.");
                }

                ShowProgress($"File hasn't been scanned before.");

                using (var ms = new MemoryStream(File.ReadAllBytes(file.FullName)))
                {
                    var result = await UploadFileAsync(ms, file.Name).ConfigureAwait(false);
                    fileDescriptorID = result.FileResult.ID;
                }
                ShowProgress($"Analyzing it...");
            }
            else
            {
                ShowProgress($"File has been scanned before.");
                fileDescriptorID = fileDescriptorResult.FileResult.ID;
                ShowProgress($"Reanalyzing it...");
            }

            int delay = DelayInSecondsBeforeAnalysisRetry > 0 ? DelayInSecondsBeforeAnalysisRetry : 1;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            FileAnalysisResult analysisResult = await GetFileAnalysisAsync(fileDescriptorID).ConfigureAwait(false);

            while (analysisResult.Data.Attributes.Status.Equals(FileAnalysisResponseStatus.Queued, StringComparison.InvariantCultureIgnoreCase))
            {
                await Task.Delay(TimeSpan.FromSeconds(delay)).ConfigureAwait(false);
                ShowProgress($"The File is still being analyzed... | {stopwatch.Elapsed:hh\\:mm\\:ss} | {analysisResult.Data.Attributes.Status}");
                analysisResult = await GetFileAnalysisAsync(fileDescriptorID).ConfigureAwait(false);
            }
            stopwatch.Stop();
            ShowProgress($"File analyses {analysisResult.Data.Attributes.Status}.");

            return analysisResult;
        }

        /// <summary>
        /// Call this method when you're done scanning.
        /// </summary>
        public void Dispose()
        {
            _serializer = null;
            _httpClientHandler?.Dispose();
            _client?.Dispose();

        }

        #region Private Methods

        /// <summary>
        /// Upload the files smaller than 32MBs by sending POST requests encoded as multipart/form-data to this endpoint (/files). 
        /// Each POST request must have a field named file containing the file to be analysed. The total payload size can not exceed 32MB.
        /// Files larger than 32MBs are uploaded by sending the POST request to the upload URL instead of sending it to /files.
        /// </summary>
        /// <param name="stream">Stream containing file content.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private async Task<ObjectDescriptorResult> UploadFileAsync(MemoryStream stream, string fileName)
        {
            ObjectDescriptorResult result;
            ShowProgress($"Uploading {fileName} ({stream.Length} bytes)...");

            if (stream.Length <= FileSizeLimit)
            {
                MultipartFormDataContent multi = new MultipartFormDataContent
                {
                    CreateFileContent(stream, fileName, true)
                };
                result = await GetResponse<ObjectDescriptorResult>("files", HttpMethod.Post, multi).ConfigureAwait(false);
            }
            else
            {
                LargeFileUpload uploadUrl = await GetResponse<LargeFileUpload>("files/upload_url", HttpMethod.Get, null);

                /* don't include size, otherwise fill uploading will fail (strange)*/
                MultipartFormDataContent multi = new MultipartFormDataContent
                {
                    CreateFileContent(stream, fileName,includeSize: false)
                };
                if (string.IsNullOrEmpty(uploadUrl.UploadUrl))
                    throw new Exception("Something went wrong while getting the upload URL.");

                result = await GetResponse<ObjectDescriptorResult>(uploadUrl.UploadUrl, HttpMethod.Post, multi).ConfigureAwait(false);
            }
            ShowProgress($"Uploading completed.");
            return result;
        }

        /// <summary>
        /// Retrieve information about a file or URL analysis.
        /// </summary>
        /// <param name="id">Analysis identifier.</param>
        /// <returns></returns>
        private async Task<FileAnalysisResult> GetFileAnalysisAsync(string id)
        {
            return await GetResponse<FileAnalysisResult>($"analyses/{id}", HttpMethod.Get, null).ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Reanalyzes a file already in VirusTotal.</para>
        /// Files that have been already uploaded to VirusTotal can be re-analyzed without uploading them again, 
        /// you can use this endpoint (https://www.virustotal.com/api/v3/files/{id}/analyse) for that purpose. 
        /// The response is an object descriptor for the new analysis as in the POST /files endpoint. 
        /// The ID contained in the descriptor can be used with the GET /analysis/{id} endpoint to get information about the analysis.
        /// See <see cref="https://developers.virustotal.com/v3.0/reference#files-analyse"/>.
        /// </summary>
        /// <param name="id">SHA-256, SHA-1 or MD5 identifying the file.</param>
        /// <returns></returns>
        private async Task<ObjectDescriptorResult> RetrieveFileDescriptorAsync(string id)
        {
            ShowProgress($"Checking the calculated hash existence on Virus Total...");
            return await GetResponse<ObjectDescriptorResult>($"files/{id}/analyse", HttpMethod.Post, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates multipart/form-data
        /// </summary>
        private HttpContent CreateFileContent(Stream stream, string fileName, bool includeSize = true)
        {
            StreamContent fileContent = new StreamContent(stream);

            ContentDispositionHeaderValue disposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = "\"" + fileName + "\""
            };

            if (includeSize)
                disposition.Size = stream.Length;

            fileContent.Headers.ContentDisposition = disposition;
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return fileContent;
        }

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

            HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken).ConfigureAwait(false);

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

        private void ShowProgress(string message)
        {
            if (OnProgressChanged == null)
                return;

            OnProgressChanged(message);
        }

        #endregion



        #endregion
    }
}
