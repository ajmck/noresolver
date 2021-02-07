using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NoResolver.Core.Helpers;
using NoResolver.Core.Models;
using System.Threading;
using System.Xml;
using System.Text;

namespace NoResolver.Core.Requesters
{

    public enum SMFLoginState
    {
        Unknown,
        Success,
        Failed,
        TimedOut
    }

    public class SMFRequester
    {
        /// <summary>
        /// Singleton pattern
        /// </summary>
        private static readonly SMFRequester _instance = new SMFRequester();
        public static SMFRequester Instance => _instance;

        public SMFLoginState Authenticated { get; private set; } = SMFLoginState.Unknown;

        private HtmlWeb _web;
        private NetworkCredential _nc;
        private CookieContainer _cookies;

        // keep track of timeouts
        private int _timeoutcount = 0;

        static SMFRequester() { }
        /// <summary>
        /// Creates connection to SMF using login values set in Config
        /// </summary>
        private SMFRequester()
        {
            Init();
        }

        public void Init()
        {
            _nc = new NetworkCredential(Config.TOKENUSERNAME, Config.TOKENCODE);
            _web = new HtmlWeb();
            _cookies = new CookieContainer();
            _web.UseCookies = true;
            _timeoutcount = 0;

            // allow security exception due to cert issue on intranet
            // https://stackoverflow.com/a/2675183/7466296
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Add network credentials
            // https://stackoverflow.com/a/23308210/7466296
            _web.PreRequest += (request) =>
            {
                request.Credentials = _nc;
                request.PreAuthenticate = true;
                // cookies required to make more than one request on a single tokencode
                // TODO - clear cookies on an invalid auth attempt and prompt for new credentials
                request.CookieContainer = _cookies;

                //// Use a Authorization header rather than NetworkCredentials as it chokes past the first request
                //// https://stackoverflow.com/a/6339609
                //request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.TOKENUSERNAME + ":" + Config.TOKENCODE))); 

                return true;
            };

            _web.PostResponse += (request, response) =>
            {
                _cookies.Add(response.Cookies);
            };
        }


        /// <summary>
        /// Tries requesting a single page to check if the SMF credentials are valid or not
        /// </summary>
        /// <returns></returns>
        public bool TestSMFCredentials()
        {
            try
            {
                var testpage = _web.Load(Config.SMFWEBURL + "/app/", "GET");
                if (_web.StatusCode != HttpStatusCode.OK)
                {
                    Authenticated = SMFLoginState.Failed;
                }
                else
                {
                    Authenticated = SMFLoginState.Success;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception raised while testing SMF credentials: " + e.Message);
                Authenticated = SMFLoginState.Failed;
            }
            return (Authenticated == SMFLoginState.Success);

        }


        /// <summary>
        /// Pulls the alert history from SMF, from the time the alert was logged plus the preceding 48 hours.
        /// </summary>
        /// <param name="FQDN">Fully Qualified Domain Name of device to be checked (ie: HOSTNAME.COMPANY.SYTECNMS.NET)</param>
        /// <param name="AlertTimestamp">Time the original alert was raised (to cut off older entries)</param>
        /// <returns></returns>
        internal async Task<ICollection<SMFHistoryLine>> LoadEventSummary(string FQDN, DateTime? AlertTimestamp = null)
        {
            if (String.IsNullOrEmpty(FQDN)) return null;
            if (Authenticated != SMFLoginState.Success)
            {
                Init();
                return null;
            }
            var loadeddevice = new List<SMFHistoryLine>();

            // if no timestamp is provided, use current time to limit history
            if (AlertTimestamp == null) AlertTimestamp = DateTime.Now;
            try
            {
                // show the prior x amount days of event history before this alert occurred
                var comparisonTimestamp = AlertTimestamp?.Subtract(new TimeSpan(days:Config.SMFLOADSHOWMANYDAYS, 0,0,0));

                var devicehistoryuri = Config.SMFWEBURL + "/app/deviceLogView.php?name=" + FQDN;
                Console.WriteLine(devicehistoryuri);


                HtmlDocument page;

                // lock -  so only one request is made at once
                lock (_web)
                {
                    page = _web.Load(devicehistoryuri, "GET");

                    if (_web.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Authenticated = SMFLoginState.Failed;
                        throw new WebException("Failed to access SMF");
                    }

                }

                // SMF table of device history is identified by a class, not HTML ID
                // select table by class
                // https://html-agility-pack.net/knowledge-base/23040482/how-to-get-element-by-class-in-htmlagilitypack
                var table = page.DocumentNode.SelectSingleNode("//table[@class='dataTableJsf']");

                foreach (var row in table?.Descendants("tr"))
                {
                    // create new line, and add the table entries, trimming whitespace
                    var devhistoryline = new SMFHistoryLine(
                        row.ChildNodes[0].InnerText.Trim(),  // timestamp
                        row.ChildNodes[1].InnerText.Trim(),  // alertkind
                        row.ChildNodes[2].InnerText.Trim()  // alertcontent
                        );

                    loadeddevice.Add(devhistoryline);

                    // stop processing entries once we've hit the timestamp
                    //if (AlertTimestamp == null || devhistoryline.Timestamp == null) break;
                    if (devhistoryline.Timestamp < comparisonTimestamp) break;
                }


                // on successful loads, just decrement the failure tracker but don't go below 0
                if (--_timeoutcount <= 0) _timeoutcount = 0;
            }
            catch (Exception e)
            {
                _timeoutcount++;
                // TODO: handle System.Net.WebException (the operation has timed out)
                Console.WriteLine("Exception raised while loading device history from SMF: " + e.Message);

                if (_timeoutcount == Config.SMFTIMEOUTCOUNT) Authenticated = SMFLoginState.TimedOut;
                
            }


            return loadeddevice;
        }


        internal async Task<ICollection<SMFPastIncidentLine>> LoadHistoricITSMIncidents(string FQDN)
        {
            if (String.IsNullOrEmpty(FQDN)) return null; ;
            if (Authenticated != SMFLoginState.Success)
            {
                Init();
                return null;
                //return "not logged in";
            }

            try
            {
                var uri = Config.SMFWEBURL + "/app/deviceIncidentView.php?name=" + FQDN;

                HtmlDocument page;

                // lock - attmept to restrain it so only one request is made at once
                lock (_web)
                {
                    page = _web.Load(uri, "GET");

                    if (_web.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Authenticated = SMFLoginState.Failed;
                        throw new WebException("Failed to access SMF");
                    }

                }

                // SMF table of device history is identified by a class, not HTML ID
                // select table by class
                // https://html-agility-pack.net/knowledge-base/23040482/how-to-get-element-by-class-in-htmlagilitypack
                var table = page.DocumentNode.SelectSingleNode("//table[@class='dataTableJsf']");

                ICollection<SMFPastIncidentLine> pastincidents = new List<SMFPastIncidentLine>();

                foreach (var row in table?.Descendants("tr"))
                {
                    // create new line, and add the table entries, trimming whitespace

                    var itsminc = new SMFPastIncidentLine(
                        row.ChildNodes[0].InnerText.Trim(),  // Timestamp
                        row.ChildNodes[1].InnerText.Trim(),  // INC
                        row.ChildNodes[2].InnerText.Trim(),  // General error
                        row.ChildNodes[3].InnerText.Trim()); // Error desc

                    pastincidents.Add(itsminc);
                }

                // on successful loads, decrement the failure tracker but don't go below 0
                if (--_timeoutcount <= 0) _timeoutcount = 0;

                return pastincidents;
            }
            catch (Exception e)
            {
                // TODO: handle System.Net.WebException (the operation has timed out)
                _timeoutcount++;
                Console.WriteLine("Exception raised while loading past incidents from SMF: " + e.Message);
                if (_timeoutcount == Config.SMFTIMEOUTCOUNT) Authenticated = SMFLoginState.TimedOut;
                return null;
            }

        }




        internal async Task<Device> LoadDevice(Device d)
        {
            try
            {
                var prior = d.LoadStatus;

                if (String.IsNullOrEmpty(d.FQDN))
                {
                    d.LoadStatus = LoadStatus.InfoUnavailable;
                    return d;
                };

                d.LastUpdatedAt = DateTime.Now;

                d.LoadStatus = LoadStatus.LoadingHistory;
                //var timestamp = TextExtractor.GetEventTimestamp(d);
                var devhistory = await LoadEventSummary(d.FQDN);
                d.DeviceHistory = new ObservableCollection<SMFHistoryLine>(devhistory);

                d.LoadStatus = LoadStatus.LoadingIncidents;
                var pastITSM = await LoadHistoricITSMIncidents(d.FQDN);
                d.PastITSMIncidents = new ObservableCollection<SMFPastIncidentLine>(pastITSM);

                

                // if you've tried loading it but you're not logged in
                if (devhistory == null && Authenticated == SMFLoginState.TimedOut)
                {
                    d.LoadStatus = LoadStatus.TimedOut;
                    return d;
                }

                if (devhistory == null && Authenticated != SMFLoginState.Success)
                {
                    d.LoadStatus = LoadStatus.SMFLoginRequired;
                    return d;
                }

                if (devhistory.Count == 0 || pastITSM.Count == 0)
                {
                    // happens on timeout
                    d.LoadStatus = LoadStatus.TimedOut;
                    return d;
                }

                d.LoadStatus = LoadStatus.Ready;
            }
            catch (Exception e)
            {
                if (Authenticated != SMFLoginState.Success)
                {
                    d.LoadStatus = LoadStatus.SMFLoginRequired;
                } else { 
                    Console.WriteLine("Exception raised while loading device history from SMF: " + e.Message);
                    d.LoadStatus = LoadStatus.Error;
                }
            }

            return d;
        }

    }
}
