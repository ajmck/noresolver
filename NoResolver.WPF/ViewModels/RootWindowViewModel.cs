using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoResolver.WPF.ViewModels
{
    public class RootWindowViewModel : BindableBase
    {

        private static RootWindowViewModel staticinstance;

        public RootWindowViewModel()
        {
            staticinstance = this;

            // load the roster on first load - more so we have feedback on the settings page
            Task.Run(() => NoResolver.OnCall.OnCallRequester.GetOnCallRoster(SettingsHelper.LAN_ID, SettingsHelper.ITSMPASSWORD));
        }

        private string _title = "Night Ops Resolver Tool";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        public static void UpdateTitleWithCount(int count=0)
        {
            if (count == 0) staticinstance.Title = "Night Ops Resolver Tool";
            else if (count == 1) staticinstance.Title = $"Night Ops Resolver Tool ({count} incident)";
            else staticinstance.Title = $"Night Ops Resolver Tool ({count} incidents)";
        }


    }
}
