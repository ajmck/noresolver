using System.ComponentModel;

namespace NoResolver.Core.Models
{
    /// <summary>
    /// Describes the status of each incident as it's loaded by SMF
    /// </summary>
    public enum LoadStatus
    {
        /// <summary>
        /// No history loaded, not waiting
        /// </summary>
        NotLoaded,

        /// <summary>
        /// In queue to load data from SMF
        /// </summary>
        [Description("Waiting...")]
        Waiting,

        /// <summary>
        /// Currently retrieving data from SMF
        /// </summary>
        [Description("SMF Login Required")]
        SMFLoginRequired,

        /// <summary>
        /// Currently retrieving data from SMF
        /// </summary>
        [Description("Loading...")]
        LoadInProgress,
        LoadingHistory,

        LoadingIncidents,

        /// <summary>
        /// Device history has been preloaded
        /// </summary>
        [Description("History Loaded")]
        Ready,

        /// <summary>
        /// Failed to load info as request timed out
        /// </summary>
        [Description("Request timed out")]
        TimedOut,

        /// <summary>
        /// A recovery event (SYSTEMPINGOK, bgp up) has been matched automatically
        /// </summary>
        [Description("Recovery detected")]
        Recovered,

        /// <summary>
        /// For entries not logged by SMF / unable to extract from notes
        /// </summary>
        [Description("Info unavailable")]
        InfoUnavailable,

        /// <summary>
        /// For incidents not logged by SMF
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// Exception was caused on loading
        /// </summary>
        [Description("Error Loading")]
        Error,
        Queued,

        // below values used when updating incident
        Assigning,
        SettingInProgress,
        SettingPending,
        Resolving,
        Resolved, // should be removing this from the INC list in this case, but it's too glitchy in its current build
        AssigningQueue,
        AssigningPerson,
        Updated
    }
}
