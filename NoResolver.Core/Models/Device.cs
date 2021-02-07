using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NoResolver.Core.Models
{
    public class Device : BindableBase
    {


        public Device(string fqdn)
        {
            FQDN = fqdn;
        }


        // hostname of device
        public string FQDN { get; private set; }


        private DateTime? _lastUpdatedAt;

        public DateTime? LastUpdatedAt
        {
            get { return _lastUpdatedAt; }
            set
            {
                SetProperty(ref _lastUpdatedAt, value);
                foreach (var i in Incidents) i.LastSMFUpdate = _lastUpdatedAt;
            }
        }


        // current stage of device loading 

        private LoadStatus _loadStatus;

        public LoadStatus LoadStatus
        {
            get { return _loadStatus; }
            set
            {
                SetProperty(ref _loadStatus, value);
                foreach (var i in Incidents) i.Loaded = _loadStatus;
            }
        }


        /// <summary>
        /// Used to gracefully clear the status when a bunch of alerts have timed out
        /// </summary>
        internal void ResetLoadStatus()
        {
            if (DeviceHistory != null || PastITSMIncidents != null) LoadStatus = LoadStatus.Ready;
            else LoadStatus = LoadStatus.NotLoaded;
        }

        private ObservableCollection<SMFHistoryLine> _deviceHistory;

        /// <summary>
        /// Device's event summary as loaded from SMF
        /// </summary>
        public ObservableCollection<SMFHistoryLine> DeviceHistory
        {
            get { return _deviceHistory; }
            set { 
                SetProperty(ref _deviceHistory, value);
                foreach (var i in Incidents) 
                    i.History = _deviceHistory;
            }
        }


        private ObservableCollection<SMFPastIncidentLine> _pastITSMIncidents;

        /// <summary>
        /// Device's past incidents as loaded from SMF
        /// </summary>
        public ObservableCollection<SMFPastIncidentLine> PastITSMIncidents
        {
            get { return _pastITSMIncidents; }
            set { 
                SetProperty(ref _pastITSMIncidents, value);
                foreach (var i in Incidents) i.PastIncidents = _pastITSMIncidents;
            }
        }


        // transitive relation between devices and incidents
        public List<ExtendedIncident> Incidents = new List<ExtendedIncident>();

    }
}
