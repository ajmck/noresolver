using NoResolver.Core.Models;

namespace NoResolver.Core.Helpers
{
    /// <summary>
    /// Text helper for an incident's resolution notes
    /// </summary>
    public class ResolutionNoteMaker
    {

        /// <summary>
        /// Generates a nice resolution note with the selected recovery alert and its timestamp. 
        /// </summary>
        /// <param name="inc">Incident we're resolving, used to extract the original timestamp</param>
        /// <param name="recovery">SMF recovery alert. Optional, will fall back to  "Incident Resolved" if null</param>
        /// <returns>Recovery message in the format "SYSTEMPINGOK recovery alert occured in 00:02:40"</returns>
        public static string GenerateResolutionNotes(ExtendedIncident inc, SMFHistoryLine recovery)
        {
            if (recovery == null) return "Incident resolved";

            return $"{recovery.AlertKind} recovery alert occurred in {recovery.Timestamp - inc.Event_TimeStamp}";
        }
    }
}
