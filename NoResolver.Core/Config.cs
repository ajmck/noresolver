// Alias to switch between UAT classes for auth
#if UAT
using IHPD_Incident = HPD_IncidentInterface_UAT;
using IVF_Incident = VFNZ_HPD_IncidentInterface_Mod_UAT;
using ITCL_HPD_WorkLog = TCL_HPD_WorkLog_UAT;
#else
using IHPD_Incident = HPD_IncidentInterface;
using IVF_Incident = VFNZ_HPD_IncidentInterface_Mod; //todo: import prod
using ITCL_HPD_WorkLog = TCL_HPD_WorkLog;
#endif

namespace NoResolver.Core
{

    /// <summary>
    /// Options for the different valid search strings in the application
    /// </summary>
    public enum QuerySwitch
    {
        NightOps,
        SEM
    }

    /// <summary>
    /// Configuration settings used at runtime - namely user credentials.
    /// 
    /// These are set in the WPF project (AppSettings.settings and SettingsHelper), as Application Settings can not be stored in a class library.
    /// Alternatively, credentials can be hard coded for test purposes, this has been omitted from the repo...
    /// </summary>
    public class Config
    {

        public static string ITSMUSERNAME;
        public static string ITSMDISPLAYNAME;
        public static string ITSMDEFAULTQUEUE /* REMOVED */;   // used for assigning incidents to self
        public static string ITSMPASSWORD;

        public static string TOKENUSERNAME;
        public static string TOKENCODE;

        public static string SSHUSERNAME;
        public static string SSHPASSWORD;

        public static string SMFSSHHOST;
        public static string SMFWEBURL;

        /// <summary>
        /// Used to select between the Night Ops and SEM search strings
        /// </summary>
        public static QuerySwitch Query;

        // ITSM queries through SOAP must use the actual ID of the database field rather than the field name
        // Schema found at http://www.softwaretoolhouse.com/freebies/index.html
        //HPD:Help Desk  1000000082  Contact Company  Company*+
        //HPD:Help Desk  1000000000  Description  Summary*
        // 1000000161 Incident ID*+
        // Security.SecurityElement.Ecsape(); // useful to put orig string and password in
        // TODO - move to AppSettings.cs like above values
#if UAT
// retrieve a single incident from UAT instance
        public static string NOTIFIERQUERY = @"'1000000161' = ""INC000001014563""";
#else

        private static string NIGHTOPSQUERY = @"('Status' < ""Pending"" AND 'Priority' < ""Medium"")"; /* REMOVED - full query excludes specific alerts and clients */
        private static string SEMQUERY = @"('Priority' = ""High"" OR 'Priority' = ""Critical"") AND 'Status' < ""Resolved"" AND ('1000000150' != ""Pending Original Incident"" OR '1000000150' = NULL)"; /* REMOVED - full query excludes specific alerts and clients */

        public static string NOTIFIERQUERY
        {
            get
            {
                switch (Query)
                {
                    case QuerySwitch.SEM:
                        return SEMQUERY;

                    case QuerySwitch.NightOps:
                    default:
                        return NIGHTOPSQUERY;
                }
            }
        }
#endif
        public static int ITSMLOADLIMIT = 200;
        public static int SMFLOADSHOWMANYDAYS = 30;

        // below is how many requests to SMF can time out in a short span before backing off
        // a successful request will reduce this
        public static int SMFTIMEOUTCOUNT = 5;

        // Authentication objects as each SOAP endpoint use their own identical class for authentication
        // Create a new AuthenticationInfo object each time as the static fields above can change

#region AuthenticationInfo Objects
        public static IHPD_Incident.AuthenticationInfo authHPD
        {
            get
            {
                return new IHPD_Incident.AuthenticationInfo
                {
                    userName = ITSMUSERNAME,
                    password = ITSMPASSWORD
                };
            }
        }

        public static IVF_Incident.AuthenticationInfo authVF
        {
            get
            {
                return new IVF_Incident.AuthenticationInfo
                {
                    userName = ITSMUSERNAME,
                    password = ITSMPASSWORD
                };
            }
        }


        public static ITCL_HPD_WorkLog.AuthenticationInfo authWL
        {
            get
            {
                return new ITCL_HPD_WorkLog.AuthenticationInfo
                {
                    userName = ITSMUSERNAME,
                    password = ITSMPASSWORD
                };
            }
        }

#endregion


#region Ready checks

        // used to indicate if credentials have been loaded or not. Note that this does not indicate if the credentials are valid and working, just that they're present.

        /// <summary>
        /// Returns true if credentials are present for ITSM. 
        /// </summary>
        public static bool READY_ITSM
        {
            get { return (!string.IsNullOrEmpty(ITSMUSERNAME)) && (!string.IsNullOrEmpty(ITSMPASSWORD)) && (!string.IsNullOrEmpty(ITSMDISPLAYNAME)); }
        }


        /// <summary>
        /// Returns true if credentials are present for SMF via Token access. 
        /// </summary>
        public static bool READY_SMF_WEB
        {
            get { return (!string.IsNullOrEmpty(SMFWEBURL)) && (!string.IsNullOrEmpty(TOKENUSERNAME)) && (!string.IsNullOrEmpty(TOKENCODE)); }
        }


        /// <summary>
        /// Returns true if credentials have been entered for SMF via SSH access.
        /// </summary>
        public static bool READY_SMF_SSH
        {
            get { return (!string.IsNullOrEmpty(SMFSSHHOST)) && (!string.IsNullOrEmpty(SSHUSERNAME)) && (!string.IsNullOrEmpty(SSHPASSWORD)); }
        }

        #endregion
    }
}
