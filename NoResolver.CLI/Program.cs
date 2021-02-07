using NoResolver.Core;
using NoResolver.Core.Requesters;
using System;

namespace NoResolver.CLI
{
    class Program
    {
        /// <summary>
        /// Console application for testing the API 
        /// 
        /// Useful when testing new endpoints.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Normally, config values are set in the WPF project. 
            // Set these values if using CLI, and avoid commiting them to the repo:
            /*
            Config.ITSMUSERNAME = "";
            Config.ITSMDISPLAYNAME = "";
            Config.ITSMPASSWORD = "";

            // if using SMFRequesterSSH, set these
            // Config.SSHUSERNAME;
            // Config.SSHPASSWORD;
            // Config.SMFSSHHOST;
            // SMFRequesterSSH smf = SMFRequesterSSH.Instance;

            // if using token version
            Config.TOKENUSERNAME = "";
            Config.SMFWEBURL = "";
            Console.WriteLine($"Enter tokencode for {Config.TOKENUSERNAME}");
            Config.TOKENCODE = Console.ReadLine();
            SMFRequester smf = SMFRequester.Instance;
            

            // start app
            Console.WriteLine("Loading Notifier...\n" + Config.NOTIFIERQUERY);
            var notifier = ITSMRequester.LoadNotifier().Result;


            // print notifier entries (INC and summary)
            foreach (var ticket in notifier)
            {
                Console.WriteLine(ticket.Incident_Number + "\t" + ticket.Summary);
            }
            Console.WriteLine("");


            // test something on each ticket
            foreach (var ticket in notifier)
            {
                try
                {
                    Console.WriteLine(ticket.Incident_Number + "\t" + ticket.Summary);

                    //var workinfo = ITSMRequester.GetWorkInfo(ticket).Result;
                    //foreach (var entry in workinfo)
                    //{
                    //    // If no worknotes, API returns
                    //    // System.ServiceModel.FaultException: ERROR (302): Entry does not exist in database;
                    //    Console.WriteLine(entry.Detailed_Description);
                    //}

                    var loadedTicket = smf.LoadDeviceHistoryFromIncident(ticket).Result;
                    foreach (var entry in loadedTicket.History)
                    {
                        Console.WriteLine(entry.Timestamp.ToString() + "\t" + entry.AlertKind + "\t" + entry.AlertContent);
                    }
                    Console.WriteLine("");
                } 
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }          
            */

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
