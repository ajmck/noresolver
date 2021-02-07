using NoResolver.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoResolver.Core.Requesters
{
    public class ContactCache
    {

        public static IList<Contact> Contacts = new List<Contact>();



        public static IList<Contact> GetAllContacts()
        {
            return Contacts;
        }


        public static IEnumerable<Contact> GetContactsByCustomer(string customer)
        {
            // linq query to filter list by the Customer field
            return from entry in Contacts where entry.Customer == customer select entry;

        }


        /// <summary>
        /// Reads in a CSV formatted string and converts it to a list of Contact objects
        /// </summary>
        /// <param name="csv"></param>
        public static void LoadContacts(string csv)
        {
            var list = new List<Contact>();

            // probably ought to put some input validation on this.
            // Also, it will import the header on the file if there is one
            try
            {
                foreach (var line in csv.Split('\n'))
                {
                    var entry = line.Split(',');
                    var contact = new Contact(entry[0], entry[1], entry[2]);
                    list.Add(contact);
                }

            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Contacts = list;
        }
    }
}
