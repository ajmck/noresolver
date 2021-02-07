using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoResolver.OnCall.Models
{
    public class OnCallLine : BindableBase
    {

        private string _group_name;
        private string _group_phone;
        private string _oncall_name;
        private string _oncall_phone;
        private string _manager_name;
        private string _manager_phone;
        private string _description;
        private string _email;


        public string GroupName
        {
            get { return _group_name; }
            set { SetProperty(ref _group_name, value); }
        }


        public string GroupPhone
        {
            get { return _group_phone; }
            set { SetProperty(ref _group_phone, value); }
        }


        public string OncallName
        {
            get { return _oncall_name; }
            set { SetProperty(ref _oncall_name, value); }
        }


        public string OncallPhone
        {
            get { return _oncall_phone; }
            set { SetProperty(ref _oncall_phone, value); }
        }


        public string ManagerName
        {
            get { return _manager_name; }
            set { SetProperty(ref _manager_name, value); }
        }


        public string ManagerPhone
        {
            get { return _manager_phone; }
            set { SetProperty(ref _manager_phone, value); }
        }


        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }


        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }


        public override string ToString()
        {
            return GroupName;
        }
    }
}
