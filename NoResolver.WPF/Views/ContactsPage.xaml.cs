using NoResolver.WPF.Helpers;
using System.Windows.Controls;
using System.Windows.Input;
using NoResolver.Core.Models;

namespace NoResolver.WPF.Views
{
    /// <summary>
    /// Interaction logic for ContactsPage.xaml
    /// </summary>
    public partial class ContactsPage : Page
    {
        public ContactsPage()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Copy a contact's phone number with the prefix when double clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContactDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/a/22791784/7466296
            Contact c = (sender as DataGrid)?.SelectedItem as Contact;
            if (c == null) return;
            ClipboardHelper.SetClipboardWithWDEPrefix(c.Phone);
        }
    }
}
