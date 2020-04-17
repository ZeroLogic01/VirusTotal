using System;

namespace VTScanner.Exceptions
{
    public class InvalidDateTimeException : Exception
    {
        public InvalidDateTimeException(string message) : base(message) { }
    }
}