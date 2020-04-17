using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VTScanner.Enums;
using VTScanner.Exceptions;
using VTScanner.Internals.EnumHelpers;

namespace VTScanner.Helpers
{
    public static class ResourcesHelper
    {
        public static bool IsNumeric(string input)
        {
            return !string.IsNullOrEmpty(input) && input.All(x => x >= 48 && x <= 57);
        }

        public static string ValidateResourcea(string resource, ResourceType type)
        {
            if (string.IsNullOrWhiteSpace(resource))
                throw new InvalidResourceException("Resource is invalid");

            string sanitized = resource;
            bool valid = false;

            if (type.HasFlag(ResourceType.MD5))
                valid |= IsValidMD5(resource);
            if (type.HasFlag(ResourceType.SHA1))
                valid |= IsValidSHA1(resource);
            if (type.HasFlag(ResourceType.SHA256))
                valid |= IsValidSHA256(resource);
            //if (type.HasFlag(ResourceType.ScanId))
            //    valid |= IsValidScanId(resource);

            if (!valid)
                throw new InvalidResourceException($"Resource '{resource}' has to be one of the following: {string.Join(", ", type.GetIndividualFlags())}");

            return sanitized;
        }

        public static bool IsReanalyseEndpoint(string url, string expression = @"^/api/v3/files/[a-zA-Z0-9]{64}/analyse")
        {
            Regex rg = new Regex(expression);
            return rg.IsMatch(url);
        }

        public static bool IsValidMD5(string resource)
        {
            return resource.Length == 32 && IsAlphaNumeric(resource);
        }

        public static bool IsValidSHA1(string resource)
        {
            return resource.Length == 40 && IsAlphaNumeric(resource);
        }

        public static bool IsValidSHA256(string resource)
        {
            return resource.Length == 64 && IsAlphaNumeric(resource);
        }

        public static bool IsAlphaNumeric(string input)
        {
            return input.All(char.IsLetterOrDigit);
        }

    }
}
