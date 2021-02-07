using NoResolver.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NoResolver.Core.Requesters
{
    /// <summary>
    /// Functions that call both ITSMRequester and SMFRequester.
    /// 
    /// Also includes stateful functions (ie: updating a list if you've already got one loaded)
    /// </summary>
    public class MultiRequester
    {


        /// <summary>
        /// Static instance for this class
        /// </summary>
        private static readonly MultiRequester _instance = new MultiRequester();
        public static MultiRequester Instance => _instance;


        /// <summary>
        /// Cached list of incidents - required so there's content ready when switching back to the Current Incidents tab 
        /// </summary>
        public static IList<ExtendedIncident> Incidents;


        // NOTE - collection of devices has been moved in to the Device model


        /// <summary>
        /// Queue for loading devices - ensures that SMF isn't being choked with multiple web requests concurrently
        /// </summary>
        private static Queue<Device> _devicesToLoad = new Queue<Device>();

        private static bool _isLoadingFromSMF = false;


        /// <summary>
        /// Gets the current list of incidents, and then makes a seperate request for each ticket to update the work info.
        /// 
        /// Uses the query set in Config.cs
        /// </summary>
        /// <returns></returns>
        public static async Task<IList<ExtendedIncident>> GetIncidents()
        {
            Incidents = await ITSMRequester.LoadIncidentQuery(Config.NOTIFIERQUERY);
            if (Incidents == null) return new List<ExtendedIncident>();

            //todo - convert this in to batch query rather than one by one check
            // although, it's fast enough as is, so no big deal
            _ = Task.Run(async () => await ITSMRequester.GetAllWorkInfo(Incidents));

            return Incidents;
        }



        /// <summary>
        /// Resolves an incident using the API
        /// </summary>
        /// <param name="inc"></param>
        /// <param name="resolution"></param>
        public async static void ResolveIncident(ExtendedIncident inc, string resolution = "Alert resolved")
        {
            await ITSMRequester.ResolveIncident(inc, resolution);
            // Incidents.Remove(inc);   // Too buggy to keep around, need to properly notify changes to UI
            // Instance.NotifyPropertyChanged("Incidents");  
        }


        /// <summary>
        /// Starts loading incidents from SMF, and if a device is already being loaded it'll wait its turn
        /// </summary>
        /// <param name="inc"></param>
        public async void QueueIncidentLoad(ExtendedIncident inc)
        {
            QueueDeviceLoad(inc.Device);
        }

        private void QueueDeviceLoad(Device device)
        {
            if (device == null) return;

            // don't load the same incident multiple times
            if (_devicesToLoad.Contains(device)) return;

            _devicesToLoad.Enqueue(device);
            device.LoadStatus = LoadStatus.Queued;

            if (!_isLoadingFromSMF)
            {
                LoadDevicesFromQueue();
            }
        }


        /// <summary>
        /// Loads the devices waiting to be loaded, and does nothing if it's working in another thread already
        /// </summary>
        private async void LoadDevicesFromQueue()
        {
            _isLoadingFromSMF = true;
            Device result;

            while (_devicesToLoad.Count!= 0)
            {
                
                // clear queue on timeout
                if (SMFRequester.Instance.Authenticated == SMFLoginState.TimedOut)
                {
                    result = _devicesToLoad.Dequeue();
                    result.ResetLoadStatus();
                    continue;
                }

                result = await SMFRequester.Instance.LoadDevice(_devicesToLoad.Dequeue());


            }
            _isLoadingFromSMF = false;
        }
    }
}
