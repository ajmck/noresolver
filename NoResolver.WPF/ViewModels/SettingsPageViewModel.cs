using Microsoft.Win32;
using NoResolver.Core;
using NoResolver.Core.Requesters;
using NoResolver.WPF.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NoResolver.WPF.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {

        public SettingsPageViewModel()
        {
            // show success messages if we've got cached credentials
            ITSM_SUCCESS_MESSAGE = (ITSMRequester.ConnectionStatus == ITSMConnectionStatus.Success);
            ROSTER_SUCCESS_MESSAGE = (OnCall.OnCallRequester.LoginStatus == OnCall.OnCallLoginStatus.Success);
            SMF_TIMEOUT_MESSAGE = (SMFRequester.Instance?.Authenticated == SMFLoginState.TimedOut);
            ASSIGNEE_JSON_OK = !string.IsNullOrEmpty(SettingsHelper.ASSIGNEE_JSON);
            CONTACT_CSV_OK = !string.IsNullOrEmpty(SettingsHelper.CONTACTS_CSV);
        }

#region Login success / error message visibility bindings

        private bool _smf_success_message = false;

        public bool SMF_SUCCESS_MESSAGE
        {
            get => _smf_success_message;
            set => SetProperty(ref _smf_success_message, value);
        }

        private bool _smf_timeout_message = false;

        public bool SMF_TIMEOUT_MESSAGE
        {
            get => _smf_timeout_message;
            set => SetProperty(ref _smf_timeout_message, value);
        }


        private bool _smf_failure_message = false;

        public bool SMF_FAILURE_MESSAGE
        {
            get => _smf_failure_message;
            set => SetProperty(ref _smf_failure_message, value);
        }


        private bool _itsm_success_message = false;

        public bool ITSM_SUCCESS_MESSAGE
        {
            get => _itsm_success_message;
            set => SetProperty(ref _itsm_success_message, value);
        }


        private bool _itsm_failure_message = false;

        public bool ITSM_FAILURE_MESSAGE
        {
            get => _itsm_failure_message;
            set => SetProperty(ref _itsm_failure_message, value);
        }


        private bool _roster_success_message = false;

        public bool ROSTER_SUCCESS_MESSAGE
        {
            get => _roster_success_message;
            set => SetProperty(ref _roster_success_message, value);
        }


        private bool _roster_failure_message = false;

        public bool ROSTER_FAILURE_MESSAGE
        {
            get => _roster_failure_message;
            set => SetProperty(ref _roster_failure_message, value);
        }

        private static string _login_button_text = "Login";

        public string LOGIN_BUTTON_TEXT
        {
            get => _login_button_text;
            set => SetProperty(ref _login_button_text, value);
        }

        private static bool _assignee_json_ok = false;

        public bool ASSIGNEE_JSON_OK
        {
            get => _assignee_json_ok;
            set => SetProperty(ref _assignee_json_ok, value);
        }

        private static bool _contact_csv_ok = false;

        public bool CONTACT_CSV_OK
        {
            get => _contact_csv_ok;
            set => SetProperty(ref _contact_csv_ok, value);
        }

        #endregion 


        private string _itsm_username = SettingsHelper.ITSMUSERNAME;
        private string _itsm_displayname = SettingsHelper.ITSMDISPLAYNAME;
        private string _itsm_password = ""; // SettingsHelper.ITSMPASSWORD;
        private string _token_username = SettingsHelper.TOKENUSERNAME;
        private string _tokencode = SettingsHelper.TOKENCODE;
        private bool _enable_roster = true;// SettingsHelper.ENABLE_ROSTER;
        private string _lan_id = SettingsHelper.LAN_ID;
        private string _lan_password = ""; // SettingsHelper.LAN_PASSWORD;
        private NoResolver.Core.QuerySwitch _query = SettingsHelper.QUERY;
        private int _refresh_interval = SettingsHelper.REFRESH_INTERVAL;


        public string ITSM_USERNAME
        {
            get => _itsm_username;
            set => SetProperty(ref _itsm_username, value);
        }

        public string ITSM_DISPLAYNAME
        {
            get => _itsm_displayname;
            set => SetProperty(ref _itsm_displayname, value);
        }

        public string ITSM_PASSWORD
        {
            get => _itsm_password;
            set => SetProperty(ref _itsm_password, value);
        }

        public string TOKEN_USERNAME
        {
            get => _token_username;
            set => SetProperty(ref _token_username, value);
        }

        public string TOKENCODE
        {
            get => _tokencode;
            set => SetProperty(ref _tokencode, value);
        }


        public string LAN_ID
        {
            get => _lan_id;
            set => SetProperty(ref _lan_id, value);
        }


        public string LAN_PASSWORD
        {
            get => _lan_password;
            set => SetProperty(ref _lan_password, value);
        }


        public bool ENABLE_ROSTER
        {
            get => _enable_roster;
            set => SetProperty(ref _enable_roster, value);
        }


        public NoResolver.Core.QuerySwitch QUERY
        {
            get => _query;
            set
            {
                SetProperty(ref _query, value);
                SettingsHelper.QUERY = value; // so the query can change without logging in again
            }
        }


        public string REFRESH_INTERVAL
        {
            get { return _refresh_interval.ToString();  }
            set
            {
                if(int.TryParse(value, out _refresh_interval)) {
                    SetProperty(ref _refresh_interval, _refresh_interval);
                    SettingsHelper.REFRESH_INTERVAL = _refresh_interval;
                }
            }
        }


        private void LoadAssigneeJsonCommand_Execute(object context)
        {
            Task.Run(() => LoadAssignees());
        }


        private bool LoadAssigneeJsonCommand_CanExecute(object context)
        {
            return true;
        }


        public ICommand LoadAssigneeJsonCommand
        {
            get { return new DelegateCommand<object>(LoadAssigneeJsonCommand_Execute, LoadAssigneeJsonCommand_CanExecute); }
        }


        private void LoadContactsCSVCommand_Execute(object context)
        {
            Task.Run(() => LoadContactsFile());
        }


        private bool LoadContactsCSVCommand_CanExecute(object context)
        {
            return true;
        }


        public ICommand LoadContactsCSVCommand
        {
            get { return new DelegateCommand<object>(LoadContactsCSVCommand_Execute, LoadContactsCSVCommand_CanExecute); }
        }


        private void LoginCommand_Execute(object context)
        {
            Task.Run(() => Login());
        }

        private bool LoginCommand_CanExecute(object context)
        {
            return true;
        }

        public ICommand LoginCommand
        {
            get { return new DelegateCommand<object>(LoginCommand_Execute, LoginCommand_CanExecute); }
        }


        private async void LoadAssignees()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON file (*.json)|*.json";

            if (ofd.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                {
                    
                    await ImportAssigneeGroups.LoadGroups(fs);
                    // then we're reserialising the JSON and storing it in the settings
                    var loadedJsonString = ImportAssigneeGroups.GetGroupsAsJson();
                    SettingsHelper.ASSIGNEE_JSON = loadedJsonString;

                    // feedback on successful load
                    ASSIGNEE_JSON_OK = !string.IsNullOrEmpty(SettingsHelper.ASSIGNEE_JSON);

                    // update the UI if this is occuring at runtime
                    AssignFlyoutViewModel.staticinstance?.GetGroups();
                }
            }
        }


        private void LoadContactsFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV file (*.csv)|*.csv";

            if (ofd.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        var csv = sr.ReadToEnd();
                        SettingsHelper.CONTACTS_CSV = csv;

                        ContactCache.LoadContacts(csv);

                        // update button if loaded
                        CONTACT_CSV_OK = ContactCache.Contacts?.Count != 0;

                    }
                }
            }
        }


        private async void Login()
        {
            ClearFeedbackMessages();
            LOGIN_BUTTON_TEXT = "Logging in...";
            SettingsHelper.QUERY = QUERY;
            SettingsHelper.ENABLE_ROSTER = ENABLE_ROSTER;

            SettingsHelper.ITSMDISPLAYNAME = ITSM_DISPLAYNAME;
            SettingsHelper.ITSMUSERNAME = ITSM_USERNAME;
            // if there's input, not just a blank password field, the user has updated the password
            if (!string.IsNullOrEmpty(ITSM_PASSWORD)) SettingsHelper.ITSMPASSWORD = ITSM_PASSWORD;

            SettingsHelper.LAN_ID = LAN_ID;
            if (!string.IsNullOrEmpty(LAN_PASSWORD)) SettingsHelper.LAN_PASSWORD = LAN_PASSWORD;
            // if (!string.IsNullOrEmpty(LAN_PASSWORD) && ENABLE_ROSTER) SettingsHelper.LAN_PASSWORD = LAN_PASSWORD; // ENABLE_ROSTER currently disused

            SettingsHelper.TOKENUSERNAME = TOKEN_USERNAME;
            SettingsHelper.TOKENCODE = TOKENCODE;

            // Required for ITSM credentials and LAN ID these are stored in the Windows credential manager
            SettingsHelper.PersistCredentials();

            if (!(SMFRequester.Instance.Authenticated == SMFLoginState.Success)) SMFRequester.Instance.Init();
            var smfok = SMFRequester.Instance.TestSMFCredentials();
            if (smfok)
            {
                SMF_SUCCESS_MESSAGE = true;
            }
            else SMF_FAILURE_MESSAGE = true;

            ITSM_FAILURE_MESSAGE = (ITSMRequester.ConnectionStatus == ITSMConnectionStatus.AuthFailure);
            ITSM_SUCCESS_MESSAGE = (ITSMRequester.ConnectionStatus == ITSMConnectionStatus.Success);
            ROSTER_SUCCESS_MESSAGE = (OnCall.OnCallRequester.LoginStatus == OnCall.OnCallLoginStatus.Success);
            ROSTER_FAILURE_MESSAGE = (OnCall.OnCallRequester.LoginStatus == OnCall.OnCallLoginStatus.Failure);


            LOGIN_BUTTON_TEXT = "Login";
        }

        private void ClearFeedbackMessages()
        {
            SMF_FAILURE_MESSAGE = false;
            SMF_SUCCESS_MESSAGE = false;
            SMF_TIMEOUT_MESSAGE = false;
            ITSM_FAILURE_MESSAGE = false;
            ITSM_SUCCESS_MESSAGE = false;
            ROSTER_SUCCESS_MESSAGE = false;
            ROSTER_FAILURE_MESSAGE = false;

        }
    }
}
