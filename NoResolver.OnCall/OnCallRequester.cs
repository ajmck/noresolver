using HtmlAgilityPack;
using NoResolver.OnCall.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NoResolver.OnCall
{

    public enum OnCallLoginStatus
    {
        Unknown,
        Success,
        Failure
    }

    public class OnCallRequester
    {

        public static OnCallLoginStatus LoginStatus { get; internal set;}

        private static ICollection<OnCallLine> roster;


        /// <summary>
        /// Gathers the on call roster as unformatted HTML
        /// </summary>
        /// <param name="username">LAN ID</param>
        /// <param name="password">LAN Password</param>
        /// <returns></returns>
        private static async Task<string> GetRosterHTML(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;

            // Web request solution for login - https://stackoverflow.com/a/56565936/7466296
            using (var handler = new HttpClientHandler() { CookieContainer = new CookieContainer() })
            {
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(/* REMOVED - URL to on call roster */) })
                {

                    // Load login page first - stops error when posting because we've retrieved valid session cookies
                    _ = await client.GetAsync(/* REMOVED - SSO login */);

                    //add credentials
                    var body = new List<KeyValuePair<string, string>>
                    {
                            new KeyValuePair<string, string>("username", username),
                            new KeyValuePair<string, string>("password", password)
                    };

                    // post username and password to /Login
                    var response = await client.PostAsync(/* REMOVED - SSO login */, new FormUrlEncodedContent(body));

                    if (response.IsSuccessStatusCode)
                    {
                        // OC roster returns a hidden form with a field "xplib", inside a tag <BODY OnLoad="autoSubmit()">
                        // get the HTML response, parse it with HTMLAgilityPack to get the value of this hidden form, and then post it back
                        var initalresponse = response.Content.ReadAsStringAsync().Result;
                        var rosterformdoc = new HtmlDocument();
                        rosterformdoc.LoadHtml(initalresponse);
                        var xplib = rosterformdoc.DocumentNode.SelectSingleNode("//input").GetAttributeValue("value", "");
                        var rosterformbody = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("xplib", xplib) };
                        response = await client.PostAsync(/* REMOVED - URL to roster */, new FormUrlEncodedContent(rosterformbody));

                        // if we got this far, the login is OK, and the next issue to watch for is parsing the table
                        LoginStatus = OnCallLoginStatus.Success;


                    } else
                    {
                        LoginStatus = OnCallLoginStatus.Failure;
                    }

                    // nb - will also return if it's a failed login
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
        }


        public static async Task<ICollection<OnCallLine>> GetOnCallRoster(string username, string password, bool forcerefresh=false)
        {
            if (!forcerefresh && roster != null) return roster;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;


            var html = await GetRosterHTML(username, password);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var list = new List<OnCallLine>();

            foreach (var row in doc.DocumentNode.Descendants("tr"))
            {
                try
                {
                    // skip if this line is a header
                    if (row.Descendants("th").GetEnumerator().MoveNext() == true) continue;

                    // on call is index 4, and their number is formatted as <a title="#########">
                    string oc_number = "";
                    var oc_enum = row.ChildNodes[4].Descendants("a").GetEnumerator();
                    if (oc_enum.MoveNext()) oc_number = oc_enum.Current.GetAttributeValue("title", "");

                    // manager is index 6, number as above
                    string mgr_number = "";
                    var mgr_enum = row.ChildNodes[6].Descendants("a").GetEnumerator();
                    if (mgr_enum.MoveNext()) mgr_number = mgr_enum.Current.GetAttributeValue("title", "");

                    // The line breaks between <td> entries counts as a #text element, hence the weird indexing
                    var oc = new OnCallLine
                    {
                        GroupName = row.ChildNodes[0].InnerText.Trim(),
                        GroupPhone = row.ChildNodes[2].InnerText.Trim(),
                        OncallName = row.ChildNodes[4].InnerText.Trim(),
                        OncallPhone = oc_number,
                        ManagerName = row.ChildNodes[6].InnerText.Trim(),
                        ManagerPhone = mgr_number,
                        Description = row.ChildNodes[8].InnerText.Trim(),
                        Email = row.ChildNodes[9].InnerText.Trim()
                    };

                    list.Add(oc);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to parse: " + row.InnerHtml);
                    LoginStatus = OnCallLoginStatus.Failure;
                }
            }

            roster = list;
            return list; 
        }




    }
}
