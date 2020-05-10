using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI
{
    public static class ExceptionHelper
    {
        public static string ExtractExceptionMessage(Exception ex)
        {
            StringBuilder exceptionText = new StringBuilder();
            exceptionText.Append(ex.Message);
            while (ex.InnerException != null)
            {
                exceptionText.Append($" {ex.InnerException.Message}");
                ex = ex.InnerException;
            }
            return exceptionText.ToString();
        }
    }
}
