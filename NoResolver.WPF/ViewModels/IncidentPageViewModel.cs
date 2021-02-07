using NoResolver.Core.Models;
using NoResolver.Core.Requesters;
using NoResolver.Core;
using Notifications.Wpf.Core;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using NoResolver.Core.Helpers;
using System.Windows.Controls;
using noresolver;
using NoResolver.WPF.Views;

namespace NoResolver.WPF.ViewModels
{
    public class IncidentPageViewModel : BindableBase
    {

        /// <summary>
        /// I think this is horrible but it's the most immediate way of getting the refresh button to work
        /// </summary>
        public static IncidentPageViewModel staticinstance;

        private static CancellationTokenSource timercancellationsource;

        private static int _refreshinterval;

        public IncidentPageViewModel()
        {

            staticinstance = this;
            _refreshinterval = SettingsHelper.REFRESH_INTERVAL;
            InitPageWithRefresh();

        }

        private void InitPageWithRefresh()
        {
            // pull cached incidents if they exist
            if (MultiRequester.Incidents != null) Incidents = MultiRequester.Incidents;

            // cancel the refresh thread if it's already running, skip if the cancellation source is null
            timercancellationsource?.Cancel();

            // do a timed refresh if it's enabled, simple if not
            if (SettingsHelper.REFRESH_INTERVAL != 0)
            {
                // create a new cancellation source
                timercancellationsource = new CancellationTokenSource();
                var ct = timercancellationsource.Token;

                // pass the token to the timed refresh helper
                if (Config.READY_ITSM)
                {
                    Task.Run(() => TimedNotifierLoad(ct));
                }
            }
            else
            {
                Task.Run(() => GetNotifier());
            }
        }

        private IList<ExtendedIncident> _incidents;

        public IList<ExtendedIncident> Incidents
        {
            get { return _incidents; }
            set { 
                SetProperty(ref _incidents, value);       
            }
        }

        private static ExtendedIncident _selectedIncident;

        public ExtendedIncident SelectedIncident
        {
            get { return _selectedIncident; }
            set 
            { 
                SetProperty(ref _selectedIncident, value);
                // also need to set it in the assignee popup
                AssignFlyoutViewModel.staticinstance.INC = _selectedIncident;
            }
        }

        private static SMFHistoryLine _selectedHistoryItem;

        public SMFHistoryLine SelectedHistoryItem
        {
            get { return _selectedHistoryItem; }
            set
            {
                
                SetProperty(ref _selectedHistoryItem, value);

                // Update the value in the SMF History add notes control
                // It'll enter in the worknotes on all tabs when clicking an incident, and clear them when clicking off the incident
                // NB: Double click handler in Views/SMFDetailControl.cs
                if (value != null)
                {
                    AddNoteBarViewModel.SetTextboxText(value.AlertKind + "     " + value.AlertContent);
                } else
                {
                    AddNoteBarViewModel.SetTextboxText("");
                }
            }
        }

        private bool _loading = false;

        /// <summary>
        /// If all notifier incidents have loaded, stop showing loading indicator
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
            set
            {
                SetProperty(ref _loading, value);
            }
        }



        /// <summary>
        /// Pulls the current notifier incidents from the core library, showing a loading indicator at the top
        /// </summary>
        /// <param name="forcerefresh">
        /// If true, it will make a fresh request to ITSM. Else, it'll just load the last loaded version.
        /// You want it to be true if manually clicking the refresh button
        /// </param>
        public async Task<IList<ExtendedIncident>> GetNotifier()
        { 
            Loading = true;
            var existingincidents = Incidents;
            var newincidents = await MultiRequester.GetIncidents();

            // workaround so the currently selected incident sticks around when the list is refreshed
            // grab the currently selected incident
            var tempselected = SelectedIncident;

            // update the main list
            Incidents = newincidents;

            // and if there's an incident selected and still in the new query, put it back to the currently selected one
            if (tempselected != null && newincidents.Contains(tempselected))
            {
                SelectedIncident = tempselected;
            }

            // Corner popup
            ShowNewIncidentPopup(existingincidents, newincidents);

            //// Also update count in menu bar
            RootWindowViewModel.UpdateTitleWithCount(newincidents.Count);

            Loading = false;
            return Incidents;
        }



        /// <summary>
        /// Shows a modal popup to say new incidents have arrived
        /// </summary>
        /// <param name="oldIncidents"></param>
        /// <param name="newIncidents"></param>
        private void ShowNewIncidentPopup(IList<ExtendedIncident> oldIncidents, IList<ExtendedIncident> newIncidents)
        {
            var message = NotificationNoteMaker.GenerateRefreshIncidentMessage(oldIncidents, newIncidents);
            if (message.Header == "") return;

            App.notificationManager.ShowAsync(new NotificationContent
            {
                Title = message.Header,
                Message = message.Body,
                Type = NotificationType.Information,
            }, expirationTime: new TimeSpan(0,0,5));
        }



        /// <summary>
        /// Runs an ITSM query every three minutes (180000ms) to refresh incidents
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task TimedNotifierLoad(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await GetNotifier();
                // convert minutes to milliseconds
                if (SettingsHelper.REFRESH_INTERVAL == 0) return;

                await Task.Delay(SettingsHelper.REFRESH_INTERVAL * 60 * 1000, ct);
            }
        }


        private async void OpenBrowser(ExtendedIncident inc)
        {
            if (SelectedIncident == null) return;
            // to open incident in browser
            //https://stackoverflow.com/a/58439029
            var psi = new ProcessStartInfo
            {
                //todo - switch for UAT
                FileName = $"/* REMOVED - link to ITSM *//arsys/servlet/ViewFormServlet?&form=HPD:Help+Desk&server=itsmarprod&qual='1000000161'=%22{inc.Incident_Number}%22",
                UseShellExecute = true
            };
            Process.Start(psi);
        }


        #region Button commands


        private bool OpenBrowserCommand_CanExecute(object context)
        {
            return true;
        }

        private void OpenBrowserCommand_Execute(object context)
        {
            Task.Run(() => OpenBrowser(SelectedIncident));
        }

        public ICommand OpenBrowserCommand
        {
            get { return new DelegateCommand<object>(OpenBrowserCommand_Execute, OpenBrowserCommand_CanExecute); }
        }



        private bool AssignToSelfCommand_CanExecute(object context)
        {
            // todo: programattically hide icon if unable to resolve item
            //return (SelectedHistoryItem != null && SelectedIncident != null);
            return true;
        }

        private void AssignToSelfCommand_Execute(object context)
        {
            Task.Run(() => ITSMRequester.AssignIncidentToSelf(SelectedIncident, Config.ITSMDISPLAYNAME));
        }

        public ICommand AssignToSelfCommand
        {
            get { return new DelegateCommand<object>(AssignToSelfCommand_Execute, AssignToSelfCommand_CanExecute); }
        }

        #endregion Button commands

    }
}
