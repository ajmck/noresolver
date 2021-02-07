using NoResolver.Core.Models;
using NoResolver.WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoResolver.WPF.Views
{
    /// <summary>
    /// Interaction logic for IncidentPage.xaml
    /// </summary>
    public partial class IncidentPage: Page
    {
        public IncidentPage()
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
