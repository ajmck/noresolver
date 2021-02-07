using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NoResolver.Core.Models;
using System.Threading;

#if UAT
using HPD_Incident = HPD_IncidentInterface_UAT;
using TCL_WorkLog = TCL_HPD_WorkLog_UAT;
using VFNZ_HPD_IncidentInterface_Mod_UAT;
using VFNZ_Incident = VFNZ_HPD_IncidentInterface_Mod_UAT;
using Incident = HPD_IncidentInterface_UAT.GetListOutputMapGetListValues;
using WorkInfo = TCL_HPD_WorkLog_UAT.OutputMappingGetListValues;
#else
using HPD_Incident = HPD_IncidentInterface;
using TCL_WorkLog = TCL_HPD_WorkLog;
using VFNZ_HPD_IncidentInterface_Mod;
using VFNZ_Incident = VFNZ_HPD_IncidentInterface_Mod;
using Incident = HPD_IncidentInterface.GetListOutputMapGetListValues;
using WorkInfo = TCL_HPD_WorkLog.OutputMappingGetListValues;
#endif



namespace NoResolver.Core.Requesters
{
    public enum ITSMConnectionStatus {
        Unknown,
        Success,
        Error,
        AuthFailure
    }

    public class ITSMRequester
    {

        public static ITSMConnectionStatus ConnectionStatus = ITSMConnectionStatus.Unknown;


        /// <summary>
        /// Loads list of open incidents, using the query set in Config.cs
        /// </summary>
        /// <returns>List of incidents</returns>
        internal static async Task<IList<ExtendedIncident>> LoadIncidentQuery(string query)
        {
            try
            {
                var client = new HPD_Incident.HPD_IncidentInterface_WSPortTypeClient();
                var interceptor = new SoapInterceptorEndpointBehaviour();
                client.Endpoint.EndpointBehaviors.Add(interceptor);

                bool hasmoreresults = true;
                Incident[] currentrequest;
                List<ExtendedIncident> resultlist = new List<ExtendedIncident>();

                for (int i = 0; hasmoreresults; i += 200)
                {
                    try
                    {
                        var results = await client.HelpDesk_QueryList_ServiceAsync(
                            Config.authHPD,
                            Qualification: Config.NOTIFIERQUERY,        // search string
                            startRecord: i.ToString(),                  // required if paginating results
                            maxLimit:200.ToString());

                        currentrequest = results.getListValues;
                        
                        // doesn't trigger - see exception
                        if (currentrequest.Length == 0) hasmoreresults = false;

                        // Convert ITSM SOAP items to custom app items
                        resultlist.AddRange(currentrequest.Select(i => new ExtendedIncident(i)));
                    }
                    catch (System.ServiceModel.FaultException e)
                    {
                        // this exception is thrown when there's no (more) results
                        hasmoreresults = false;

                        if (e.Message == "ARERR[623] Authentication failed")
                        {
                            ConnectionStatus = ITSMConnectionStatus.AuthFailure;
                            return null;
                        }
                    }
                }

                ConnectionStatus = ITSMConnectionStatus.Success;

                // reverse so newest INCs are at top / so newest INCs are checked before others
                resultlist.Reverse();
                return resultlist;
            }          
            catch (Exception e)
            {
                ConnectionStatus = ITSMConnectionStatus.Error;
                Console.WriteLine("Exception while loading notifier: " + e.Message);
                return null;
            }
        }


        /// <summary>
        /// Iterates over list of incidents, running GetWorkInfo on all incidents.
        /// </summary>
        /// <param name="tickets"></param>
        /// <returns></returns>
        internal static async Task<IList<ExtendedIncident>> GetAllWorkInfo(IList<ExtendedIncident> tickets)
        {
            if (tickets == null) return null;

            foreach (var i in tickets)
            {
                i.Work_Info = await GetWorkInfo(i);
            }
            return tickets;
        }


        /// <summary>
        /// Get work info for each incident.
        /// 
        /// Note that the API retuns an exception on empty work notes, which is handled here to return an empty array.
        /// Consequently, the entire notifier query can't be run at once and incidents must be checked one by one.
        /// </summary>
        /// <param name="inc">Incident to retrieve worknotes for</param>
        /// <returns></returns>
        internal static async Task<WorkInfo[]> GetWorkInfo(ExtendedIncident inc)
        {
            try
            {
                var client = new TCL_WorkLog.TCL_HPD_WorkLogPortTypeClient();
                var interceptor = new SoapInterceptorEndpointBehaviour();
                client.Endpoint.EndpointBehaviors.Add(interceptor);

                // I've attempted to run the whole notifier query in SoapUI, but received a Qualification Line Error
                // therefore, run query one by one
                // 1000000161 is field for incident id
                var results = await client.OpGetListAsync(Config.authWL,
                    $"'1000000161' = \"{inc.Incident_Number}\"");
                //Console.WriteLine(results);
                return results.GetListValues;

            }
            catch (System.ServiceModel.FaultException e)
            {
                // If no worknotes, API returns the below
                // System.ServiceModel.FaultException: ERROR (302): Entry does not exist in database;
                // return empty WorkInfo in this case
                return new WorkInfo[0];
            }
            catch (Exception e)
            {
                ConnectionStatus = ITSMConnectionStatus.Error;
                Console.WriteLine("Exception while loading work info: " + e.Message);
                return null;
            }
        }


        /// <summary>
        /// Adds a work note entry to an incident.
        /// </summary>
        /// <param name="inc">Incident which will be updated</param>
        /// <param name="content">Text to be added as a worknote</param>
        /// <param name="refresh">Whether the work notes should be fetched from the server after being inserted</param>
        public static async void InsertWorkInfo(ExtendedIncident inc, string content, bool refresh = true)
        {
            // code smell - button in UI is still visible when no incident is selected
            if (inc == null || string.IsNullOrEmpty(content)) return;
            try
            {
                var client = new TCL_WorkLog.TCL_HPD_WorkLogPortTypeClient();
                var interceptor = new SoapInterceptorEndpointBehaviour();
                client.Endpoint.EndpointBehaviors.Add(interceptor);

                var result = await client.OpCreateAsync(Config.authWL,
                    TCL_WorkLog.Work_Info_SourceType.Web,
                    "Night Ops Resolver Tool",
                    "► " + content,  // icon indicator to show tool usage
                    inc.Incident_Number,
                    TCL_WorkLog.Secure_Work_LogType.No,
                    TCL_WorkLog.Work_Info_View_AccessType.Internal,
                    DateTime.Now,
                    TCL_WorkLog.Work_Info_TypeType.StatusUpdate);

                if (refresh)
                {
                    // retrieve work info after a successful update, and return the incident with the updated entry
                    Thread.Sleep(3000);

                    inc.Work_Info = await GetWorkInfo(inc);
                }
                
            } catch (Exception e)
            {
                Console.WriteLine("Exception raised while inserting work info: " + e.Message);
            }
        }


        /// <summary>
        /// Assigns the incident to an ITSM user in the Business and Government queue.
        /// </summary>
        /// <param name="inc"></param>
        /// <param name="assignee"></param>
        /// <returns></returns>
        public static async Task<HelpDesk_Assign_IncidentResponse> AssignIncidentToSelf(ExtendedIncident inc, string assignee="")
        {
            var results = await AssignIncident(inc, "/* REMOVED */", "Customer Services", "/* REMOVED */", assignee);
            return results;
        }


        /// <summary>
        /// Assigns the incident to a new group or user
        /// 
        /// Note that the group must be set before the assignee, therefore this call will run twice if not already in the right queue
        /// </summary>
        /// <param name="inc"></param>
        /// <param name="assigned_support_company"></param>
        /// <param name="assigned_support_organization"></param>
        /// <param name="assigned_group"></param>
        /// <param name="assignee"></param>
        /// <returns></returns>
        public static async Task<HelpDesk_Assign_IncidentResponse> AssignIncident(ExtendedIncident inc, string assigned_support_company, string assigned_support_organization, string assigned_group, string assignee = "")
        {
            
            // code smell - button in UI is still visible when no incident is selected
            if (inc == null) return null;

            // return empty answer if no action is required
            if (inc.Assigned_Group == assigned_group && inc.Assignee == assignee) return new HelpDesk_Assign_IncidentResponse();

            try
            {
                var client = new VFNZ_Incident.HPD_IncidentInterface_WSPortTypePortTypeClient();
                var interceptor = new SoapInterceptorEndpointBehaviour();
                client.Endpoint.EndpointBehaviors.Add(interceptor);

                var loadstatus = inc.Loaded;


                // had issues assigning to the user before the queue
                // if the group needs to be assigned, and this function has a user to assign to, and the user isn't already assigned to the incident, set group
                if (inc.Assigned_Group != assigned_group && assignee != "" && assignee != inc.Assignee)
                {
                    inc.Loaded = LoadStatus.AssigningQueue;
                    await AssignIncident(inc, assigned_support_company, assigned_support_organization, assigned_group);
                } else if (assignee != "")
                {
                    inc.Loaded = LoadStatus.AssigningPerson;
                }
                var results = await client.HelpDesk_Assign_IncidentAsync(Config.authVF,
                    Incident_Number: inc.Incident_Number,
                    Direct_Contact_Company: inc.INC.Company,
                    Action_Status: "ASSIGN_INCIDENT",
                    InterfaceAction: InterfaceActionType.UpdateIncident,
                    Status: StatusType.InProgress,                          // isn't working, only sets to assigned
                    Assigned_Support_Company: assigned_support_company,
                    Assigned_Support_Organization: assigned_support_organization,
                    Assigned_Group: assigned_group,
                    Assignee: assignee);

                inc.Loaded = loadstatus;

                // return so this function works synchrously
                return results;
            }
            catch (Exception e)
            {

                Console.WriteLine("Exception raised while reassigning incident: " + e.Message);
                return null;
            }
        }


        public static async Task<ExtendedIncident> AssignIncidentInProgress(ExtendedIncident inc, string assigned_support_company, string assigned_support_organization, string assigned_group, string assignee)
        {
            // var loadstatus = inc.Loaded;
            inc.Loaded = LoadStatus.Assigning;
            if (assigned_group != inc.Assigned_Group || assignee != inc.Assignee)
                _ = await AssignIncident(inc, assigned_support_company, assigned_support_organization, assigned_group, assignee);
            var two = await SetIncidentInProgress(inc);
            inc.Loaded = LoadStatus.Updated;
            return two;
        }


        public static async Task<ExtendedIncident> SetIncidentInProgress(ExtendedIncident inc)
        {
            if (inc == null) return null;
            try
            {
                var client = new HPD_Incident.HPD_IncidentInterface_WSPortTypeClient();
                // don't use the interceptor here, it's not really relevant as we're posting correct XML

                // This SOAP wants all the XML tags to be there, and if the tags are an empty string it'll clear the existing value
                // And, if the values are null (except the Work Info ones below as they're enum values), the request will throw an exception
                // Do note that making the Work Info values nullable requires editing the generated code for the endpoint (wrap args with System.Nullable<>)

                // grab original incident, but preprocess it so it's not trying to insert null values
                var i = ReplaceRequiredNullValuesWithEmptyStrings(inc.INC);

                // change LoadStatus for UI feedback
                var currentload = inc.Loaded;
                inc.Loaded = LoadStatus.SettingInProgress;

                var results = await client.HelpDesk_Modify_ServiceAsync(Config.authHPD,
                    Categorization_Tier_1: i.Categorization_Tier_1,
                    Categorization_Tier_2: i.Categorization_Tier_2,
                    Categorization_Tier_3: i.Categorization_Tier_3,
                    Closure_Manufacturer: i.Closure_Manufacturer,
                    Closure_Product_Category_Tier1: i.Closure_Product_Category_Tier1,
                    Closure_Product_Category_Tier2: i.Closure_Product_Category_Tier2,
                    Closure_Product_Category_Tier3: i.Closure_Product_Category_Tier3,
                    Closure_Product_Model_Version: i.Closure_Product_Model_Version,
                    Closure_Product_Name: i.Closure_Product_Name,
                    Company: i.Company,
                    Summary: i.Summary,
                    Notes: i.Notes,
                    Impact: i.Impact,
                    Manufacturer: i.Manufacturer,
                    Product_Categorization_Tier_1: i.Product_Categorization_Tier_1,
                    Product_Categorization_Tier_2: i.Product_Categorization_Tier_2,
                    Product_Categorization_Tier_3: i.Product_Categorization_Tier_3, 
                    Product_Model_Version: i.Product_Model_Version,
                    Product_Name: i.Product_Name,
                    Reported_Source: i.Reported_Source,
                    Resolution: i.Resolution,
                    Resolution_Category: i.Resolution_Category,
                    Resolution_Category_Tier_2: i.Resolution_Category_Tier_2,
                    Resolution_Category_Tier_3: i.Resolution_Category_Tier_3,
                    Resolution_Method: "",  // "First Contact Fix" normally
                    Service_Type: i.Service_Type,
                    Status: HPD_Incident.StatusType.InProgress, // i.Status
                    Urgency: i.Urgency,
                    Action: "",
                    Work_Info_Summary: "",
                    Work_Info_Notes: "",
                    Work_Info_Type: null,
                    Work_Info_Date: null,
                    Work_Info_Source: null,
                    Work_Info_Locked: null,
                    Work_Info_View_Access: null,
                    Incident_Number: i.Incident_Number);

                Console.WriteLine(results);

                // put LoadStatus back to what it was
                inc.Loaded = currentload;
                
                // return so this function works synchrously
                return inc;
            }
            catch (Exception e)
            {

                Console.WriteLine("Exception raised while placing incident in progress: " + e.Message);
                return null;
            }
        }


        public static async Task<ExtendedIncident> SetIncidentPending(ExtendedIncident inc)
        {
            throw new NotImplementedException();

            // thought it would be similar to how it is for setting "In Progress",
            // but the endpoint is missing the required field for Status Reason

            //var results = await client.HelpDesk_Modify_ServiceAsync(...,
            //    Status: HPD_Incident.StatusType.Pending,
            //    Status_Reason: HPD_Incident.Status_ReasonType.ClientHold); // Tried manually adding this to WDSL, no dice

            // we get an error +		$exception	{"ERROR (1291220): ; The Status Reason field requires a value when the status is pending. Select the status reason from the menu."}	System.ServiceModel.FaultException

            // probably requires ITSM API work

        }



        internal static Incident ReplaceRequiredNullValuesWithEmptyStrings(Incident i)
        {
            // below statements are equivalent to 
            // if x == null
            //     x = ""
            // else return x

            // can be shortened to x ??= "" in C# 8
            i.Categorization_Tier_1 = i.Categorization_Tier_1 ?? "";
            i.Categorization_Tier_2 = i.Categorization_Tier_2 ?? "";
            i.Categorization_Tier_3 = i.Categorization_Tier_3 ?? "";
            i.Closure_Manufacturer = i.Closure_Manufacturer ?? "";
            i.Closure_Product_Category_Tier1 = i.Closure_Product_Category_Tier1 ?? "";
            i.Closure_Product_Category_Tier2 = i.Closure_Product_Category_Tier2 ?? "";
            i.Closure_Product_Category_Tier3 = i.Closure_Product_Category_Tier3 ?? "";
            i.Closure_Product_Model_Version = i.Closure_Product_Model_Version ?? "";
            i.Closure_Product_Name = i.Closure_Product_Name ?? "";
            i.Company = i.Company ?? "";
            i.Summary = i.Summary ?? "";
            i.Notes = i.Notes ?? "";
            i.Manufacturer = i.Manufacturer ?? "";
            i.Product_Categorization_Tier_1 = i.Product_Categorization_Tier_1 ?? "";
            i.Product_Categorization_Tier_2 = i.Product_Categorization_Tier_2 ?? "";
            i.Product_Categorization_Tier_3 = i.Product_Categorization_Tier_3 ?? "";
            i.Product_Model_Version = i.Product_Model_Version ?? "";
            i.Product_Name = i.Product_Name ?? "";
            i.Resolution = i.Resolution ?? "";
            i.Resolution_Category = i.Resolution_Category ?? "";
            i.Resolution_Category_Tier_2 = i.Resolution_Category_Tier_2 ?? "";
            i.Resolution_Category_Tier_3 = i.Resolution_Category_Tier_3 ?? "";

            return i;
        }


        /// <summary>
        /// Resolves an incident.
        /// 
        /// If there is no assignee, it will assign the incident to self before proceeding.
        /// </summary>
        /// <param name="inc">Incident to be resolved</param>
        /// <param name="resolution">Text to be entered in resolution notes of incident</param>
        internal static async Task<HelpDesk_Resolve_IncidentResponse> ResolveIncident(ExtendedIncident inc, string resolution)
        {
            // code smell - button in UI is still visible when no incident is selected
            if (inc == null) return null;
            try
            {
                var client = new VFNZ_Incident.HPD_IncidentInterface_WSPortTypePortTypeClient();
                var interceptor = new SoapInterceptorEndpointBehaviour();
                client.Endpoint.EndpointBehaviors.Add(interceptor);

                inc.Loaded = LoadStatus.Resolving;
                
                // Assign to self if no assignee
                if (string.IsNullOrEmpty(inc.INC.Assignee))
                {
                    await AssignIncidentToSelf(inc, Config.ITSMDISPLAYNAME);
                }

                var results = await client.HelpDesk_Resolve_IncidentAsync(Config.authVF,
                    Incident_Number: inc.Incident_Number,
                    Direct_Contact_Company: inc.INC.Company, //?
                    Action_Status: "",
                    InterfaceAction: InterfaceActionType.ResolveIncident,
                    Status: StatusType.Resolved,
                    Status_Reason: Status_ReasonType.NoFurtherActionRequired,
                    Work_Info_Notes: null,
                    Work_Info_Summary: null,
                    Work_Info_Type: null,
                    Work_Info_Date: null,                   // must modify WDSL to make this nullable
                    Work_Info_Communication_Source: null,
                    Work_Info_Locked: null,
                    Work_Info_View_Access: null,
                    Resolution: resolution,           // generally "Device recovered without intervention."
                    Cause: "Alert",                     // connectivity / hardware / nff / power / environmental
                    Resolution_Method: "First Contact Fix",
                    Resolution_Category_Tier_1:     inc.INC.Resolution_Category,
                    Resolution_Category_Tier_2:     inc.INC.Resolution_Category_Tier_2,
                    Resolution_Category_Tier_3:     inc.INC.Resolution_Category_Tier_3,
                    Closure_Product_Category_Tier1: inc.INC.Closure_Product_Category_Tier1,
                    Closure_Product_Category_Tier2: inc.INC.Closure_Product_Category_Tier2,
                    Closure_Product_Category_Tier3: inc.INC.Closure_Product_Category_Tier3,
                    Closure_Product_Name:           inc.INC.Closure_Product_Name,
                    Closure_Product_Model_Version:  inc.INC.Closure_Product_Model_Version,
                    Closure_Manufacturer:           inc.INC.Closure_Manufacturer);

                inc.Loaded = LoadStatus.Resolved;

                return results;
            }
            catch (Exception e)
            {
                ConnectionStatus = ITSMConnectionStatus.Error;
                Console.WriteLine("Exception raised while resolving incident: " + e.Message);
                return null;
            }
            
        }
    }
}
