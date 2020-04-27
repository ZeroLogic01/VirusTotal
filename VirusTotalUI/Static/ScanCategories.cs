using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.Static
{
    public static class ScanCategories
    {
        public static string ConfirmedTimeout { get; } = "confirmed-timeout";
        public static string Failure { get; } = "failure";
        public static string Harmless { get; } = "harmless";
        public static string Malicious { get; } = "malicious";
        public static string Suspicious { get; } = "suspicious";
        public static string Timeout { get; } = "timeout";
        public static string TypeUnsupported { get; } = "type-unsupported";
        public static string Undetected { get; } = "undetected";
    }
}
