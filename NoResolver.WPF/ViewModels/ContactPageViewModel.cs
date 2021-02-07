using NoResolver.Core.Models;
using NoResolver.Core.Requesters;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace NoResolver.WPF.ViewModels
{
    public class ContactPageViewModel : BindableBase
    {

        public ContactPageViewModel()
        {
            Contacts = new ObservableCollection<Contact>(ContactCache.GetAllContacts());
        }

        private ObservableCollection<Contact> _contacts;

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { SetProperty(ref _contacts, value);  }
        }
    }
}