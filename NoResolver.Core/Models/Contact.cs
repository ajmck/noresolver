using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoResolver.Core.Models
{
    /// <summary>
    /// Phone numbers imported to represent customer contacts, service desks, and SDMs
    /// </summary>
    public class Contact : BindableBase
    {

        public Contact() { }

        public Contact(string customer, string name, string phone)
        {
            Customer = customer;
            Name = name;
            Phone = phone;
        }

        private string _customer;
        private string _name;
        private string _phone;

        /// <summary>
        /// Customer name as set in ITSM. This is required to cross reference the contacts for a given incident.
        /// </summary>
        public string Customer { get { return _customer; } set { SetProperty(ref _customer, value); } }

        
        /// <summary>
        /// Description of the phone number (ie: SDM On Call, Service Desk, name of SDM)
        /// </summary>
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }


        /// <summary>
        /// Phone number. Use standard domestic format (0210000000, not 6421000000), as its designed to be copied straight in to WDE 
        /// </summary>
        public string Phone { get { return _phone; } set { SetProperty(ref _phone, value); } }
    }
}
