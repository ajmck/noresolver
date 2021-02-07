using NoResolver.Core.Helpers;
using NoResolver.Core.Models;
using NoResolver.WPF.ViewModels;
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
    /// Interaction logic for SMFDetailControl.xaml
    /// </summary>
    public partial class SMFDetailControl : UserControl
    {
        public SMFDetailControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Enter nicely formatted worknotes when doubleclicking an incident
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceEventRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/a/22791784/7466296
            SMFHistoryLine smfline = (sender as DataGrid)?.SelectedItem as SMFHistoryLine;
            if (smfline == null) return;
            //// Some operations with this row
            var prettynotes = ResolutionNoteMaker.GenerateResolutionNotes(IncidentPageViewModel.staticinstance.SelectedIncident, smfline);
            AddNoteBarViewModel.SetTextboxText(prettynotes);
        }

    }
}
