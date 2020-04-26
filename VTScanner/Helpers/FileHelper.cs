using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VTScanner.Models;

namespace VTScanner.Helpers
{
    internal class FileHelper
    {
        internal const string _virusTotalOutputFileSuffix = "VTResults";
        internal const string _textFileExtension = ".txt";
        internal const string _jsonFileExtension = ".json";
        internal static readonly string _rootSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        ///  Reads the text file in the current working directory &amp; returns the first non empty line as Virus Total API key.
        /// </summary>
        /// <param name="fileName">This is the file in the current directory that contains the VirusTotal key.</param>
        /// <returns>Returns the first not empty line (as Virus Total API key) from the text file.</returns>
        internal static string ReadApiKeyFromCurrentDirectory(string fileName)
        {
            return File.ReadLines(Path.Combine(Directory.GetCurrentDirectory(), fileName))
                .Where(predicate: line => !string.IsNullOrWhiteSpace(line)).FirstOrDefault();
        }

        private static string GetOutputFileUniqueName(string filePath, string fileExtension)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileName = $"{fileNameWithoutExtension} {_virusTotalOutputFileSuffix}{fileExtension}";

            if (File.Exists(Path.Combine(_rootSaveDirectory, fileName.Trim())))
            {
                fileName = $"{fileNameWithoutExtension} {_virusTotalOutputFileSuffix}" +
                    $"{DateTime.Now: yyyy-MM-dd-HH-mm-ss}{fileExtension}";
            }

            return Path.Combine(_rootSaveDirectory, fileName.Trim());
        }

        internal async Task<string> WriteFileScanResultToTextFileAsync(string filePath, List<ScanEngineReport> fileScanResult, CancellationToken cancellationToken)
        {
            string outputTextFilePath = GetOutputFileUniqueName(filePath, _textFileExtension);

            using (FileStream sourceStream = new FileStream(outputTextFilePath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                byte[] encodedText = Encoding.Unicode
                        .GetBytes($"File Name ; Engine Name ; Is Detected ; Description{Environment.NewLine}");
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);

                foreach (var result in fileScanResult)
                {
                    encodedText = Encoding.Unicode
                        .GetBytes($"{result.FileName} ; {result.EngineName} ; {result.IsDetected} ; {result.Description}{Environment.NewLine}");
                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length, cancellationToken);
                }
            };
            return outputTextFilePath;
        }

        internal async Task<string> WriteFileScanResultToJsonFileAsync(string filePath, List<ScanEngineReport> fileScanResult, CancellationToken cancellationToken)
        {
            string outputJsonFilePath = GetOutputFileUniqueName(filePath, _jsonFileExtension);
            await Task.Run(() =>
            {
                string jsonData = JsonConvert.SerializeObject(fileScanResult, Formatting.Indented);
                File.WriteAllText(outputJsonFilePath, jsonData);
            }, cancellationToken).ConfigureAwait(false);
            return outputJsonFilePath;
        }
    }
}
