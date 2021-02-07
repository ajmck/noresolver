using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoResolver.Core.Models
{

    /// <summary>
    /// Represents the different ITSM queues and their assignees
    /// </summary>
    public class AssigneeGroup : BindableBase
    {


        /// <summary>
        /// Parent menu entry when selecting the group 
        /// </summary>
        public string AssignedSupportCompany { get; set; }


        /// <summary>
        /// Child menu when selecting the group (i.e. "Business & Government", "Networks & Services")
        /// </summary>
        public string AssignedSupportOrganization { get; set; }


        /// <summary>
        /// The actual queue name (i.e. "Business & Government Service Desk -tcl01")
        /// </summary>
        public string AssignedGroup { get; set; }


        private List<string> _assignees;


        /// <summary>
        /// The assignees within each group
        /// </summary>
        public List<string> Assignees
        {
            get { return _assignees; }
            set { SetProperty(ref _assignees, value); }
        }


        // empty constructer needed for Json Serialisation
        public AssigneeGroup() { }


        public AssigneeGroup(string Assigned_Support_Company, string Assigned_Support_Organization, string Assigned_Group, List<string> Assignees_List) {
            AssignedSupportCompany = Assigned_Support_Company;
            AssignedSupportOrganization = Assigned_Support_Organization;
            AssignedGroup = Assigned_Group;
            Assignees = Assignees_List;
        }


        // return the actual queue name in the UI
        public override string ToString()
        {
            return AssignedGroup;
        }
    }
}
