using System.IO;
using System.Linq;

namespace VTScanner.Helpers
{
    internal static class FileHelper
    {
        /// <summary>
        ///  Reads the text file in the current working directory &amp; returns the first non empty line as Virus Total API key.
        /// </summary>
        /// <param name="fileName">This is the file in the current directory that contains the VirusTotal key.</param>
        /// <returns>Returns the first not empty line (as Virus Total API key) from the text file.</returns>
        internal static string ReadApiKeyFromCurrentDirectory(string fileName)
        {
            return File.ReadLines(Path.Combine(Directory.GetCurrentDirectory(), fileName))
                .Where(predicate: line => line.Trim() != "").FirstOrDefault();
        }
    }
}
