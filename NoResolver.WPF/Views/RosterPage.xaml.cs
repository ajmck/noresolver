using NoResolver.OnCall.Models;
using NoResolver.WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for RosterPage.xaml
    /// </summary>
    public partial class RosterPage : Page
    {
        public RosterPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Copy a contact's phone number with the prefix when double clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCallDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/a/22791784/7466296
            OnCallLine c = (sender as DataGrid)?.SelectedItem as OnCallLine;
            if (c == null) return;
            ClipboardHelper.ParseAndCopyRosterNumber(c.OncallPhone);
        }
    }
}
