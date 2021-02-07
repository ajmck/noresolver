using NoResolver.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoResolver.Core
{
    public class ImportAssigneeGroups
    {

        private static IList<AssigneeGroup> _groups;

        public static IList<AssigneeGroup> GetGroups()
        {
            return _groups;
        }


        /// <summary>
        /// Serialise a set of assignees and the queues from a string of JSON
        /// </summary>
        /// <param name="Assignee_Json"></param>
        /// <returns></returns>
        public async static Task<IList<AssigneeGroup>> LoadGroups(string Assignee_Json)
        {
            var serialised = JsonSerializer.Deserialize<List<AssigneeGroup>>(Assignee_Json);
            _groups = serialised;
            return serialised;   
        }


        /// <summary>
        /// Seriaises a set of assignees and the queues from a JSON file
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <returns></returns>
        public async static Task<IList<AssigneeGroup>> LoadGroups(FileStream jsonfile)
        {
            // using FileStream fs = File.OpenRead(@"Resources\assignees.json");
            var serialised = await JsonSerializer.DeserializeAsync<List<AssigneeGroup>>(jsonfile);
            _groups = serialised;
            return serialised;
        }

        /// <summary>
        /// Returns the JSON of all assignees. Used because we store the JSON back in the WPF application config.
        /// 
        /// Yes, it would be fine if we stored the exact input file we're using, 
        /// but because it's more graceful to open the json as a filestream rather than a plain string, this gives us a plain string for us to store
        /// </summary>
        /// <returns></returns>
        public static string GetGroupsAsJson()
        {
            return JsonSerializer.Serialize(_groups);
        }



    }
}
