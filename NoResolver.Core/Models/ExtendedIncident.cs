using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoResolver.Core.Helpers;
using Prism.Mvvm;
using NoResolver.Core.Requesters;

// Conditionally switch between UAT and regular incidents
#if UAT
using HPD_IncidentInterface_UAT;
using Incident = HPD_IncidentInterface_UAT.GetListOutputMapGetListValues;
using WorkInfo = TCL_HPD_WorkLog_UAT.OutputMappingGetListValues;
#else
using HPD_IncidentInterface;
using Incident = HPD_IncidentInterface.GetListOutputMapGetListValues;
using WorkInfo = TCL_HPD_WorkLog.OutputMappingGetListValues;
#endif

namespace NoResolver.Core.Models
{


    /// <summary>
    /// Wraps the Incident model provided by the ITSM service with extra fields
    /// </summary>
    public class ExtendedIncident : BindableBase
    {
        public ExtendedIncident() { }


        public ExtendedIncident(Incident i)
        {
            INC = i;

            // Check that alert was logged by SMF before extracting values
            if (i.Reported_Source == Reported_SourceType.SystemsManagement)
            {
                Event_Source = TextExtractor.GetEventSource(i.Notes);
                Event_General_Error = TextExtractor.GetGeneralError(i.Notes);
                Event_TimeStamp = TextExtractor.GetEventTimestamp(i.Notes);
            }

            if (string.IsNullOrEmpty(Event_Source))
            {
                Loaded = LoadStatus.InfoUnavailable;
            }
            else
            {
                //History = new ObservableCollection<SMFHistoryLine>();
                Device = DeviceCache.GetOrCreateDevice(this);

                if (Device != null)
                { 
                    Loaded = Device.LoadStatus;
                } else
                {
                    // redundant as this is the enum with a value of 0
                    Loaded = LoadStatus.NotLoaded;
                }
            }

        }

        /// <summary>
        /// ITSM incident as returned by the API, no extra info
        /// </summary>
        public Incident INC { get; private set; }

        #region Variables set from device

        public Device Device { get; internal set; }

        private ObservableCollection<SMFHistoryLine> _history;

        public ObservableCollection<SMFHistoryLine> History
        {
            get
            {
                if (_history == null) return Device?.DeviceHistory;
                else return _history;
            }
            set
            {
                SetProperty(ref _history, value);
            }
        }


        private ObservableCollection<SMFPastIncidentLine> _pastIncidents;

        public ObservableCollection<SMFPastIncidentLine> PastIncidents
        {
            get
            {
                if (_pastIncidents == null) return Device?.PastITSMIncidents;
                else return _pastIncidents;
            }
            set
            {
                SetProperty(ref _pastIncidents, value);
            }
        }


        private LoadStatus _loaded;

        /// <summary>
        /// Status of the device history
        /// i.e. waiting to be loaded, loading, loaded, timed out, invalid, so on
        /// </summary>
        public LoadStatus Loaded
        {
            get { return _loaded; }
            set { SetProperty(ref _loaded, value); }
        }


        private DateTime? _lastSMFUpdate;

        public DateTime? LastSMFUpdate
        {
            get
            {
                if (_lastSMFUpdate == null) return Device?.LastUpdatedAt;
                else return _lastSMFUpdate;
            }
            set
            {
                SetProperty(ref _lastSMFUpdate, value);
            }
        }


        #endregion

        // Following accesors are here so it's easier to reference them in the DataGrid
        public string Notes => INC.Notes;
        public string Incident_Number => INC.Incident_Number;
        public string Summary => INC.Summary;
        public string Assignee => INC.Assignee;
        public StatusType Status => INC.Status;
        public string Company => INC.Company;
        public string FQDN => TextExtractor.GetEventSource(INC.Notes);   // isn't this is the same as Event_Source?
        public string Assigned_Group => INC.Assigned_Group;


        /// <summary>
        /// Returns an integer of the incident ID for comparison purposes.
        /// Substring of 3 means it's stripping the characters "INC"
        /// </summary>
        public int Incident_Integer => int.Parse(INC.Incident_Number.Substring(3));


        // values extracted from ITSM notes
        // these shouldn't change after the incident is loaded, so no real need to run SetProperty
        public string Event_Source { get; private set; }            // fully qualified device name 
        public string Event_General_Error { get; private set; }     // general name of error (ie: SYSTEMPINGFAIL, CISCOSYSLOGBGPERROR)
        public DateTime? Event_TimeStamp { get; private set; }



        public IEnumerable<Contact> Contacts => ContactCache.GetContactsByCustomer(Company);



        private ICollection<WorkInfo> _work_info = new WorkInfo[0];


        /// <summary>
        /// The work notes for this incident
        /// </summary>
        public ICollection<WorkInfo> Work_Info
        {
            get { return _work_info; }
            set { SetProperty(ref _work_info, value); }
        }





        /// <summary>
        /// Compares the incident by its ticket number
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this.GetType().Equals(obj.GetType()))
            {
                var comp = (ExtendedIncident)obj;
                return (comp.Incident_Number == this.Incident_Number);
            }
            return base.Equals(obj);
        }



        // WIP - SLA display - refer to Git Wiki/Feature Implementation Tips
        public SLAValue SLAStatus { get; internal set; }
        public DateTime? SLATimer { get; internal set; }

    }
}
