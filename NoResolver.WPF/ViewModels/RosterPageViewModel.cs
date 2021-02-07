using NoResolver.OnCall;
using NoResolver.OnCall.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoResolver.WPF.ViewModels
{
    public class RosterPageViewModel : BindableBase
    {

        public static RosterPageViewModel staticinstance;


        public RosterPageViewModel()
        {
            staticinstance = this;
            Task.Run(() => GetRoster());
        }


        private ICollection<OnCallLine> _roster;

        public ICollection<OnCallLine> Roster
        {
            get { return _roster; }
            set { SetProperty(ref _roster, value); }
        }


        public async void GetRoster(bool forcerefresh = false)
        {
            Roster = await OnCallRequester.GetOnCallRoster(SettingsHelper.LAN_ID, SettingsHelper.LAN_PASSWORD, forcerefresh);
        }
    }
}
