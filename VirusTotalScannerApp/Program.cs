using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTScanner;
using VTScanner.Results;

namespace VirusTotalScannerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            args = new[] { @"F:\machine_learning.pdf", "Key.txt" };
#endif
            string apiKeyFileName = args[1];
            string fileToScan = args[0];

            Scanner scanner = new Scanner(apiKeyFileName) { UseTLS = true };
            var result = await scanner.ScanFileAsync(fileToScan).ConfigureAwait(false);

            if (result != null)
            {

            }

            Console.ReadLine();
        }
    }
}
