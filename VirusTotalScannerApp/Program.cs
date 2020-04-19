using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VTScanner;
using VTScanner.Results;
using VTScanner.Results.AnalysisResults;

namespace VirusTotalScannerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            args = new[] { @"", "Key.txt" };
#endif
            Scanner scanner = null;
            try
            {
                string apiKeyFileName = args[1];
                string fileToScan = args[0];

                scanner = new Scanner(apiKeyFileName) { UseTLS = true, Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) };
                scanner.OnProgressChanged += Scanner_OnProgressMade;
                await scanner.ScanFileAndGenrateOutputAsync(fileToScan).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                StringBuilder exceptionText = new StringBuilder();
                exceptionText.Append(ex.Message);
                while (ex.InnerException != null)
                {
                    exceptionText.Append($" {ex.InnerException.Message}");
                    ex = ex.InnerException;
                }
                Console.Error.WriteLine(exceptionText.ToString());
            }
            finally
            {
                if (scanner != null)
                {
                    scanner.OnProgressChanged -= Scanner_OnProgressMade;
                    scanner.Dispose();
                }
            }
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        private static void Scanner_OnProgressMade(string message)
        {
            Console.WriteLine(message);
        }


    }
}
