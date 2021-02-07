using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Globalization;

namespace NoResolver.Core.Models
{

    /// <summary>
    /// Single entry of the device's event summary
    /// </summary>
    public class SMFHistoryLine : BindableBase
    {

        public SMFHistoryLine(string timestamp, string alertkind, string alertcontent)
        {
            AlertKind = alertkind;
            AlertContent = alertcontent;
            TimestampString = timestamp;
        }

        private string _alertKind;

        /// <summary>
        /// Generalised alert, i.e. SYSTEMPINGFAIL, CISCOSYSLOGBGPERROR, NAGIOSSYSTEMMEMORYWARN
        /// </summary>
        [JsonProperty("event_classification")]
        public string AlertKind
        {
            get { return _alertKind; }
            set { SetProperty(ref _alertKind, value); }
        }


        private string _alertContent;

        /// <summary>
        /// Full content of the event - will look like 
        /// nzps1nms1.syt.sytecnms.net [local3.info]: service-notify-by-event-manager: 1605875940 PCN-321-A-GND-SW01.POL.SYTECNMS.NET HOST_ALIVE 3 HARD 5.725 [Notification-Type: PROBLEM]: PING CRITICAL - Packet loss = 16%, RTA = 1011.13 ms
        /// </summary>
        [JsonProperty("event_message")]
        public string AlertContent
        {
            get { return _alertContent; }
            set { SetProperty(ref _alertContent, value); }
        }


        private DateTime? _timestamp;

        /// <summary>
        /// DateTime object of when this event occured
        /// </summary>
        public DateTime? Timestamp
        {
            get { return _timestamp; }
            set { SetProperty(ref _timestamp, value); }
        }


        // If day has only a single digit, it is replaced by whitespace. Two parse rules required. 
        private string[] FORMATSTRINGS = new string[2] { "MMM  d HH:mm:ss", "MMM dd HH:mm:ss" };


        /// <summary>
        /// String of the event timestamp.
        /// 
        /// Should only be required when serialising from JSON, as loading SMF from JSON doesn't return a JSON timestamp
        /// </summary>
        [JsonProperty("event_time")]
        public string TimestampString
        {
            get { return Timestamp?.ToString(); }
            set
            {
                DateTime tmpTimestamp;
                bool result = DateTime.TryParseExact(value, FORMATSTRINGS, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out tmpTimestamp);
                if (result) Timestamp = tmpTimestamp;
            }
        }


        public override string ToString()
        {
            return AlertContent;
        }
    }
}
