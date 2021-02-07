using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace NoResolver.WPF.Helpers
{
    public class ClipboardHelper
    {
        public static void SetClipboard(string text)
        {
            Clipboard.SetText(text);
        }


        public static void SetClipboardWithWDEPrefix(string text)
        {
            SetClipboard("999" + text.Trim());
        }

        internal static void ParseAndCopyRosterNumber(string oncallPhone)
        {
            // set international dialing prefix
            var processed = oncallPhone.Replace("+", "00");
            // and strip out all non digits
            processed = Regex.Replace(processed, "\\D", "");
            SetClipboardWithWDEPrefix(processed);
        }
    }
}
