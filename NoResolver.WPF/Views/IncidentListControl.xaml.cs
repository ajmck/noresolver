using NoResolver.Core.Models;
using NoResolver.Core.Requesters;
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
    /// Interaction logic for IncidentListControl.xaml
    /// </summary>
    public partial class IncidentListControl : UserControl
    {
        public IncidentListControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Queue device loading when double clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncidentLineDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/a/22791784/7466296
            ExtendedIncident inc = (sender as DataGrid)?.SelectedItem as ExtendedIncident;
            if (inc == null) return;
            // inc.Loaded = LoadStatus.Queued; // NOPE, this is now set in core
            Task.Run(() => MultiRequester.Instance.QueueIncidentLoad(inc));
        }
    }
}
