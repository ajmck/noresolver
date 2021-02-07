using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NoResolver.Core.Helpers
{
    /// <summary>
    /// Text extractor to parse info from an incident's notes
    /// </summary>
    public class TextExtractor
    {
        /// <summary>
        /// SMF returns its dates in the format "Jan 11 23:45:23" or "Jan  1 23:45:32" (leading day is replaced by space rather than 0". 
        /// Account for both of these formats
        /// </summary>
        private static string[] FORMATSTRINGS = new string[2] { "MMM  d HH:mm:ss", "MMM dd HH:mm:ss" };


        /// <summary>
        /// Extracts the fully qualified domain name from the ITSM notes
        /// </summary>
        /// <param name="note_body">ITSM notes field</param>
        /// <returns>Fully qualified domain name of the device which raised the alert</returns>
        public static string GetEventSource(string note_body)
        {
            // rule: match all characters in the line after "Event Source: "
            Regex rule = new Regex(@"(?<=Event Source: ).*");
            return rule.Match(note_body).Value;
        }


        /// <summary>
        /// Extracts the error (ie: SYSTEMPINGFAIL) from the ITSM notes
        /// </summary>
        /// <param name="note_body">ITSM notes field</param>
        /// <returns>General error type for this incident</returns>
        public static string GetGeneralError(string note_body)
        {
            // rule - gets first word in notes
            // https://stackoverflow.com/a/49868808
            // only will work for incidents logged from SMF account, as they have the error as the first word in the INC notes
            Regex rule = new Regex(@"^([\w]+)");
            var extract = rule.Match(note_body).Value;
            return extract;
        }


        /// <summary>
        /// Extracts the timestamp of when the alert was raised from the ITSM notes
        /// </summary>
        /// <param name="note_body">ITSM notes field</param>
        /// <returns>DateTime of when the alert was raised, or null if it couldn't be found</returns>
        public static DateTime? GetEventTimestamp(string note_body)
        {
            // rule: match characters after "Event Data: ", where . is any character and \d is digit
            // Jan  1 14:44:46
            Regex rule = new Regex(@"(?<=Event Data: ).....\d.\d\d:\d\d:\d\d");
            var parsetext = rule.Match(note_body);

            if (!parsetext.Success) return null;

            DateTime result;
            bool parsesuccess = DateTime.TryParseExact(parsetext.Value, FORMATSTRINGS, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out result);

            if (!parsesuccess) return null;
            return result;
        }

    }
}
