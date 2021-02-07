using NoResolver.Core.Models;
using NoResolver.Core.Requesters;
using NoResolver.WPF.ViewModels;
using NoResolver.WPF.Views;
using System;
using System.Windows;
using System.Windows.Data;
using ModernWpf.Controls;
using NoResolver.Core;
using System.Windows.Input;
using Prism.Commands;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NoResolver.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainNav : UserControl
    {

        private static MainNav staticInstance;

        public MainNav()
        {
            InitializeComponent();
            staticInstance = this;  
            DataContext = this;

            // Open settings page on default if no credentials entered
            if (!Config.READY_ITSM)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                contentFrame.Navigate(typeof(IncidentPage));
            }
        }



        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var selectedItem = (ModernWpf.Controls.NavigationViewItem)args.SelectedItem;
                // contentFrame.Navigate(typeof(IncidentPage));
                if (selectedItem != null)
                {
                    string selectedItemTag = (string)selectedItem.Tag;
                    //sender.Header = "Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                    string pageName = "NoResolver.WPF.Views." + selectedItemTag;
                    Type pageType = typeof(NoResolver.WPF.Views.IncidentPage).Assembly.GetType(pageName);
                    contentFrame.Navigate(pageType);
                }
            }
        }


        // fully aware that this should be in the ViewModel, but I was getting Binding path errors
        private bool RefreshCommand_CanExecute(object context)
        {
            return true;
        }

        private void RefreshCommand_Execute(object context)
        {
            Console.WriteLine("refreshing from MWVM");
            // TODO - reset timed refresh
            Task.Run(() => IncidentPageViewModel.staticinstance?.GetNotifier());

            Task.Run(() => RosterPageViewModel.staticinstance?.GetRoster(true));
        }

        public ICommand RefreshCommand
        {
            get { return new DelegateCommand<object>(RefreshCommand_Execute, RefreshCommand_CanExecute); }
        }



        //public static void UpdateIncidentCountDisplay(int count=0)
        //{
        //    if (count == 0) staticInstance.IncidentTab.Content = "Current Incidents";
        //    else staticInstance.IncidentTab.Content = $"Current Incidents ({count})";
        //}

    }
}
