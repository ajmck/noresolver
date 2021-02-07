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
    public class AddNoteBarViewModel : BindableBase
    {

        /// <summary>
        /// Because this bar is in three different tabs, and each tab has their own viewmodel, keep a list of them and replicate changes to the textbox across all instances
        /// 
        /// Possibly a cleaner way to accomplish this...
        /// </summary>
        private static List<AddNoteBarViewModel> instances = new List<AddNoteBarViewModel>();
        
        public AddNoteBarViewModel()
        {
            instances.Add(this);
        }


        #region AddBoxContent - the textbar which takes up the length of this control
        private string _addBoxContent;

        public string AddBoxContent
        {
            get { return _addBoxContent; }
            set
            {
                SetProperty(ref _addBoxContent, value);
            }
        }
        
        /// <summary>
        /// Updates the text of all AddBoxContent fields across multiple viewmodels
        /// </summary>
        /// <param name="str"></param>
        internal static void SetTextboxText (string str)
        {
            foreach (AddNoteBarViewModel vm in instances)
            {
                vm.AddBoxContent = str;
            }
        }
        
        #endregion


        private bool AddWorkNoteCommand_CanExecute(object context)
        {
            // todo: programattically hide icon if unable to add item
            //return (SelectedHistoryItem != null && SelectedIncident != null);
            return true;
        }

        private void AddWorkNoteCommand_Execute(object context)
        {
            Task.Run(() => ITSMRequester.InsertWorkInfo(IncidentPageViewModel.staticinstance.SelectedIncident, AddBoxContent));
            //Task.Run(() => ITSMRequester.InsertWorkInfo(SelectedIncident, SelectedHistoryItem?.AlertKind + "\n" + SelectedHistoryItem?.AlertContent));
        }

        public ICommand AddWorkNoteCommand
        {
            get { return new DelegateCommand<object>(AddWorkNoteCommand_Execute, AddWorkNoteCommand_CanExecute); }
        }



        private bool ResolveCommand_CanExecute(object context)
        {
            // todo: programattically hide icon if unable to resolve item
            //return (SelectedHistoryItem != null && SelectedIncident != null);
            return true;
        }

        private void ResolveCommand_Execute(object context)
        {
            Task.Run(() => MultiRequester.ResolveIncident(IncidentPageViewModel.staticinstance.SelectedIncident, AddBoxContent));
            //Task.Run(() => ITSMRequester.ResolveIncident(SelectedIncident, ResolutionNoteMaker.GenerateResolutionNotes(SelectedIncident, SelectedHistoryItem)));
        }

        public ICommand ResolveCommand
        {
            get { return new DelegateCommand<object>(ResolveCommand_Execute, ResolveCommand_CanExecute); }
        }


    }
}
