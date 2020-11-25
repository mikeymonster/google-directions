using System;

namespace poc.Google.Directions.Extensions
{
    public static class StringExtensions
    {
        public static string ToLetterOrDigit(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : 
               new string(Array.FindAll(value.ToCharArray(), char.IsLetterOrDigit));
        }
    }
}