using NoResolver.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoResolver.Core.Requesters
{
    public class DeviceCache
    {
        static DeviceCache()
        {
            _devices = new Dictionary<string, Device>();
        }


        /// <summary>
        /// Cached devices in dictionary - key is FQDN
        /// </summary>
        private static IDictionary<string, Device> _devices;


        /// <summary>
        /// Gets a device from the cache if it exists, otherwise creates a cache entry 
        /// </summary>
        /// <param name="fqdn">Hostname of device (used as key in dictionary)</param>
        /// <returns>Device object, or null if request isn't valid</returns>
        internal static Device GetOrCreateDevice(string fqdn)
        {

            if (string.IsNullOrEmpty(fqdn)) return null;
            if (_devices == null) throw new Exception("Device cache is null");

            if (_devices.ContainsKey(fqdn))
            {
                return _devices[fqdn];
            }

            var dev = new Device(fqdn);
            if (dev == null) return null;
            // The below line has a tendency to throw a null reference exception, even when _devices fqdn and dev are all OK
            // and wrapping this function in a try-catch doesn't actually catch it
            // beats me tbh...
            _devices.Add(fqdn, dev);
            return dev;

        }


        /// <summary>
        /// Gets a device from the cache if it exists, and associates the incident to the device for when the data is updated
        /// </summary>
        /// <param name="inc"></param>
        /// <returns></returns>
        internal static Device GetOrCreateDevice(ExtendedIncident inc)
        {
            var d = GetOrCreateDevice(inc.FQDN);

            d.Incidents.Add(inc);

            // ExtendedIncident.Device is set in the ExtendedIncident constructor so don't need to assign it to INC here

            // If this incident is already in the cache, replace it with the new instance of the ExtendedIncident object
            // This works as the .Equals method in ExtendedIncident just compares the incident ID
            //if (d != null)
            //{
            //    var existingInc = d.Incidents.IndexOf(inc);
            //    if (existingInc != -1)
            //    {
            //        d.Incidents[existingInc] = inc;
            //    }
            //}

            // TODO - remove devices if they have no more references around
            // We've probably got a bunch of disused references being created on each refresh

            return d;
        }

    }
}
