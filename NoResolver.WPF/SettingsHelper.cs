using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Meziantou.Framework.Win32;
using NoResolver.Core;
using NoResolver.Core.Requesters;

namespace NoResolver.WPF
{
    public class SettingsHelper
    {

        /// <summary>
        /// Loads application settings from NoResolver.WPF.AppSettings to NoResolver.Core.Config. 
        /// 
        /// Required because application logic occurs in the library NoResolver.Core, but application settings can not be stored in the class library, only the application itself.
        /// 
        /// Called on project startup in App.xaml.cs
        /// </summary>
        public static void LoadAllSettings()
        {
            // credentials
            Config.ITSMDISPLAYNAME = AppSettings.Default.ITSMDISPLAYNAME;
            Config.ITSMUSERNAME = CredentialManager.ReadCredential("NightOpsResolver_ITSM")?.UserName;
            Config.ITSMPASSWORD = CredentialManager.ReadCredential("NightOpsResolver_ITSM")?.Password;

            Config.TOKENUSERNAME = AppSettings.Default.TOKENUSERNAME;
            Config.TOKENCODE = AppSettings.Default.TOKENCODE;

            Config.SSHUSERNAME = AppSettings.Default.SSHUSERNAME;
            Config.SSHPASSWORD = AppSettings.Default.SSHPASSWORD;

            // Application level settings, not edited at runtime
            Config.ITSMLOADLIMIT = AppSettings.Default.ITSMLOADLIMIT;
            Config.SMFSSHHOST = AppSettings.Default.SMFSSHHOST;
            Config.SMFWEBURL = AppSettings.Default.SMFWEBURL;

            // Not in NoResolver.Core.Config as it's in the seperate library NoResolver.OnCall
            ENABLE_ROSTER = AppSettings.Default.ENABLE_ROSTER;
            LAN_ID = CredentialManager.ReadCredential("NightOpsResolver_LANID")?.UserName;
            LAN_PASSWORD = CredentialManager.ReadCredential("NightOpsResolver_LANID")?.UserName;

            // need to cast as AppSettings stores this as an int rather than the enum directly
            QUERY = (QuerySwitch) AppSettings.Default.QUERY;

            REFRESH_INTERVAL = AppSettings.Default.REFRESH_INTERVAL;
            ASSIGNEE_JSON = AppSettings.Default.ASSIGNEE_JSON;
            CONTACTS_CSV = AppSettings.Default.CONTACTS_CSV;

            if (!string.IsNullOrEmpty(ASSIGNEE_JSON))
            {
                // discard to suppress async warning
                _ = ImportAssigneeGroups.LoadGroups(ASSIGNEE_JSON);
            }

            if (!string.IsNullOrEmpty(CONTACTS_CSV))
            {
                ContactCache.LoadContacts(CONTACTS_CSV);
            }
        }


#region Setters
        // These fields will set the values in both NoResolver.Core.Config and AppSettings at the same time

        public static string ITSMDISPLAYNAME
        {
            get { return Config.ITSMDISPLAYNAME; }
            set { 
                Config.ITSMDISPLAYNAME = value;
                AppSettings.Default.ITSMDISPLAYNAME = value;
                AppSettings.Default.Save();
            }
        }


        public static string ITSMUSERNAME
        {
            get { return Config.ITSMUSERNAME; }
            set
            {
                Config.ITSMUSERNAME = value;
            }
        }


        public static string ITSMPASSWORD
        {
            get { return Config.ITSMPASSWORD; }
            set
            {
                Config.ITSMPASSWORD = value;
            }
        }


        public static string TOKENUSERNAME
        {
            get { return Config.TOKENUSERNAME; }
            set
            {
                Config.TOKENUSERNAME = value;
                AppSettings.Default.TOKENUSERNAME = value;
                AppSettings.Default.Save();
            }
        }


        public static string TOKENCODE
        {
            get { return Config.TOKENCODE; }
            set
            {
                Config.TOKENCODE = value;
                // we don't want to persist this in settings because it's only valid once for 60 seconds
                //AppSettings.Default.ITSMDISPLAYNAME = value;
                //AppSettings.Default.Save();
            }
        }


        public static string SSHUSERNAME
        {
            get { return Config.SSHUSERNAME; }
            set
            {
                Config.SSHUSERNAME = value;
                AppSettings.Default.SSHUSERNAME = value;
                AppSettings.Default.Save();
            }
        }


        public static string SSHPASSWORD
        {
            get { return Config.SSHPASSWORD; }
            set
            {
                Config.SSHPASSWORD = value;
                AppSettings.Default.SSHPASSWORD = value;
                AppSettings.Default.Save();
            }
        }



        private static bool _enable_roster;

        public static bool ENABLE_ROSTER
        {
            get { return _enable_roster; }
            set
            {
                _enable_roster = value;
                AppSettings.Default.ENABLE_ROSTER = value;
                AppSettings.Default.Save();
            }
        }

        // autoproperty as these values don't need to interact with Config.cs

        public static string LAN_ID { get; set; }

        public static string LAN_PASSWORD { get; set; }


        private static QuerySwitch _query;

        public static QuerySwitch QUERY
        { 
            get { return _query; }
            set
            {
                _query = value;
                AppSettings.Default.QUERY = (int) value;
                Config.Query = value;
                AppSettings.Default.Save();
            }
        }


        private static int _refresh_interval;

        public static int REFRESH_INTERVAL {
            get { return _refresh_interval; }
            set
            {
                _refresh_interval = value;
                AppSettings.Default.REFRESH_INTERVAL = _refresh_interval;
                AppSettings.Default.Save();
            }
        }


        private static string _assignee_json;

        public static string ASSIGNEE_JSON { 
            get { return _assignee_json; }
            set
            {
                _assignee_json = value;
                AppSettings.Default.ASSIGNEE_JSON = _assignee_json;
                AppSettings.Default.Save();
            }
        }


        private static string _contacts_csv;

        public static string CONTACTS_CSV
        {
            get { return _contacts_csv; }
            set
            {
                _contacts_csv = value;
                AppSettings.Default.CONTACTS_CSV = _contacts_csv;
                AppSettings.Default.Save();
            }
        }

        #endregion Setters

        public static bool PersistCredentials()
        {
            // todo - handle invalid creds
            if (!string.IsNullOrEmpty(ITSMUSERNAME) || !string.IsNullOrEmpty(ITSMPASSWORD))
            {
                CredentialManager.WriteCredential("NightOpsResolver_ITSM", ITSMUSERNAME, ITSMPASSWORD, CredentialPersistence.LocalMachine);
            }

            if (!string.IsNullOrEmpty(LAN_ID) || !string.IsNullOrEmpty(LAN_PASSWORD))
            {
                CredentialManager.WriteCredential("NightOpsResolver_LANID", LAN_ID, LAN_PASSWORD, CredentialPersistence.LocalMachine);
            }
            return true;
        }

    }
}
