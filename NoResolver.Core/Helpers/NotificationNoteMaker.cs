using NoResolver.Core.Models;
using System.Collections.Generic;

namespace NoResolver.Core.Helpers
{
    /// <summary>
    /// Text helper for modal popup notifications
    /// </summary>
    public class NotificationNoteMaker
    {

        /// <summary>
        /// Used to generate the text for a notification when incidents are refreshed. 
        /// 
        /// Gives you a header with a count of the newest incidents, and a body with the summary of the newest 5 INCs
        /// </summary>
        /// <param name="oldIncidents"></param>
        /// <param name="newIncidents"></param>
        /// <returns>A tuple of two strings representing the header and body</returns>
        public static (string Header, string Body) GenerateRefreshIncidentMessage(IList<ExtendedIncident> oldIncidents, IList<ExtendedIncident> newIncidents)
        {
            (string Header, string Body) empty = ("", "");
            if (oldIncidents == null || newIncidents == null) return empty;
            if (newIncidents.Count == 0) return empty;

            // cast to list so we can get 0th index
            int oldIndex = oldIncidents[0].Incident_Integer;
            // list with only the new incidents
            var newOnly = new List<ExtendedIncident>();

            // add any incidents which came ahead of the first entry in the existing list
            for (int i = 0; i < newIncidents.Count; i++)
            {
                if (newIncidents[i].Incident_Integer <= oldIndex) break;
                newOnly.Add(newIncidents[i]);
            }

            // don't show popup unless there's new incidents
            if (newOnly.Count == 0) return empty;

            // generate text for body of popup - no more than five incidents 
            string messageText = "";
            for (int i = 0; i < newOnly.Count && i <= 5; i++)
            {
                messageText += newOnly[i].Summary + "\n";
            }

            // grammar rules
            if (newOnly.Count == 1)
                return ($"{newOnly.Count} new incident", messageText);

            return ($"{newOnly.Count} new incidents", messageText);
        }

    }
}
