using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VTScanner;
using VTScanner.Results;
using VTScanner.Results.AnalysisResults;

namespace VirusTotalScannerApp
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        static async Task Main(string[] args)
        {
            IntPtr wow64Value = IntPtr.Zero;

            // Disable redirection.
            Wow64DisableWow64FsRedirection(ref wow64Value);

            System.IO.FileInfo file = new System.IO.FileInfo(@"C:\Windows\System32\DrtmAuth12.bin");

            if (file.Exists)
            {
                Console.WriteLine($"{file.FullName} ");

            }

            int counter = 1;
            foreach (var s in file.Directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                if (s.Exists)
                {
                    Console.WriteLine($"{counter} | {s.FullName} ");
                }
                counter++;
            }

            string exePath = Path.Combine(Directory.GetCurrentDirectory(), "Release", "VirusTotalUI.exe");

            if (file.Exists)
            {
                Console.WriteLine("File Exist");
            }
            //take ownership of the file, code assumes file you want to delete is toBeDeleted.txt
            //ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/k takeown /f {file.FullName} && icacls {file.FullName} /grant %username%:F")
            //{
            //    UseShellExecute = true,
            //    Verb = "runas",
            //    FileName = exePath,
            //    Arguments = $"\"{file.FullName}\" \"Key.txt\" \".5\" \".2\" \".6\""
            //};
            //try
            //{
            //    if (file.Exists)
            //    {
            //        Console.WriteLine("File Exist");
            //    }
            //    Process.Start(processInfo);
            //    // a prompt will be presented to user continue with deletion action
            //    // you may want to have some other checks before deletion
            //    //File.Delete(@"C:\Windows\System32\toBeDeleted.txt");

            //}
            //catch (Win32Exception)
            //{
            //    //Do nothing as user cancelled UAC window.
            //}
        }

        //        static async Task Main(string[] args)
        //        {
        //#if DEBUG
        //            args = new[] { @"F:\Speech To Text\transcribe-sample.5fc2109bb28268d10fbc677e64b7e59256783d3c.mp3", "Key.txt" };
        //#endif
        //            Scanner scanner = null;
        //            try
        //            {
        //                string apiKeyFileName = args[1];
        //                string fileToScan = args[0];

        //                scanner = new Scanner(apiKeyFileName, CancellationToken.None) { UseTLS = true, Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)};
        //                scanner.OnProgressChanged += Scanner_OnProgressMade;
        //                await scanner.ScanFileAndGenrateOutputAsync(fileToScan).ConfigureAwait(false);
        //            }
        //            catch (Exception ex)
        //            {
        //                StringBuilder exceptionText = new StringBuilder();
        //                exceptionText.Append(ex.Message);
        //                while (ex.InnerException != null)
        //                {
        //                    exceptionText.Append($" {ex.InnerException.Message}");
        //                    ex = ex.InnerException;
        //                }
        //                Console.Error.WriteLine(exceptionText.ToString());
        //            }
        //            finally
        //            {
        //                if (scanner != null)
        //                {
        //                    scanner.OnProgressChanged -= Scanner_OnProgressMade;
        //                    scanner.Dispose();
        //                }
        //            }
        //            Console.WriteLine("Press enter to exit...");
        //            Console.ReadLine();
        //        }

        private static void Scanner_OnProgressMade(string message)
        {
            Console.WriteLine(message);
        }


    }
}
