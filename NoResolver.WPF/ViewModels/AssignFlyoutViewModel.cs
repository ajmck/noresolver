using NoResolver.Core;
using NoResolver.Core.Models;
using NoResolver.Core.Requesters;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NoResolver.WPF.ViewModels
{
    public class AssignFlyoutViewModel : BindableBase
    {


        /// <summary>
        /// cuz we need to pass this flyout the currently selected incident and this has been the modus operandi for the rest of the project
        /// </summary>
        public static AssignFlyoutViewModel staticinstance;


        public AssignFlyoutViewModel()
        {
            staticinstance = this;
            Task.Run(() => GetGroups());
        }


        public void GetGroups()
        {
            Groups = ImportAssigneeGroups.GetGroups();
        }

        #region Variables + Bindings

        private ExtendedIncident _inc;

        // when setting the INC, also set the selected queue and asssignee
        public ExtendedIncident INC
        {
            get { return _inc; }
            set
            {
                _inc = value;
                AssignedGroup = _inc?.Assigned_Group;
                Assignee = _inc?.Assignee;
            }
        }


        // all groups loadde in to this project
        private IList<AssigneeGroup> _groups;

        public IList<AssigneeGroup> Groups
        {
            get { return _groups; }
            set { SetProperty(ref _groups, value); }
        }


        // plain string required as otherwise it chokes on queues that aren't in the Groups object above
        private string _assignedGroup = "";

        public string AssignedGroup
        {
            get { return _assignedGroup; }
            set { SetProperty(ref _assignedGroup, value); }
        }


        // and an AssigneeGroup object also required so we can get the assignees underneath this
        private AssigneeGroup _selectedGroup;

        public AssigneeGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set { SetProperty(ref _selectedGroup, value); }
        }


        // Currently selected assignee (works fine ATM since the object above has assignees as a list of strings, 
        // if converting them to an object with phone numbers the XAML might need an update
        private string _assignee = "";

        public string Assignee
        {
            get { return _assignee; }
            set { SetProperty(ref _assignee, value); }
        }



        #endregion

        // button "Self"

        private bool AssignToSelfCommand_CanExecute(object context)
        {
            return true;
        }

        private void AssignToSelfCommand_Execute(object context)
        {
            if (INC == null) return;
            Task.Run(() => ITSMRequester.AssignIncidentToSelf(INC, Config.ITSMDISPLAYNAME));
            AssignedGroup = Config.ITSMDEFAULTQUEUE;
            Assignee = Config.ITSMDISPLAYNAME;

        }

        public ICommand AssignToSelfCommand
        {
            get { return new DelegateCommand<object>(AssignToSelfCommand_Execute, AssignToSelfCommand_CanExecute); }
        }


        // button "Assign"

        private bool AssignCommand_CanExecute(object context)
        {
            return true;
        }

        private void AssignCommand_Execute(object context)
        {
            if (_selectedGroup == null || INC == null) return;

            // if there's an assignee, put it in progress, else just assign it
            if (string.IsNullOrEmpty(Assignee)) Task.Run(() => ITSMRequester.AssignIncident(INC, SelectedGroup.AssignedSupportCompany, SelectedGroup.AssignedSupportOrganization, SelectedGroup.AssignedGroup, Assignee));
            else                      Task.Run(() => ITSMRequester.AssignIncidentInProgress(INC, SelectedGroup.AssignedSupportCompany, SelectedGroup.AssignedSupportOrganization, SelectedGroup.AssignedGroup, Assignee));
        }

        public ICommand AssignCommand
        {
            get { return new DelegateCommand<object>(AssignCommand_Execute, AssignCommand_CanExecute); }
        }


        // TODO: button "Pend"
    }
}
