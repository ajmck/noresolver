using Prism.Ioc;
using NoResolver.WPF.Views;
using System.Windows;
using NoResolver.WPF;
using Notifications.Wpf.Core;

namespace noresolver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        public static NotificationManager notificationManager;

        protected override Window CreateShell()
        {
            notificationManager = new NotificationManager();
            SettingsHelper.LoadAllSettings();
            // load the roster on first load in RootWindowViewModel, we want the window to be rendered first tho
            return Container.Resolve<RootWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
