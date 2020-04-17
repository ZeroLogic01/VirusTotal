using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTScanner.ResponseCodes
{
    public static class FileAnalysisResponseStatus
    {
        /// <summary>
        /// The file is still being scanned
        /// </summary>
        public const string Queued = "queued";

        /// <summary>
        /// The file scan is completed.
        /// </summary>
        public const string Completed = "completed";
    }
}
