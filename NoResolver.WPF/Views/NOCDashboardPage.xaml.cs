using System;
using System.Collections.Generic;
using System.Reflection;
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
    /// Interaction logic for PriorityDashboardPage.xaml
    /// </summary>
    public partial class NOCDashboardPage : Page
    {
        public NOCDashboardPage()
        {
            InitializeComponent();

            /// Suppress JavaScript errors
            /// https://stackoverflow.com/a/28464363/7466296
            HideScriptErrors(this.browsercontrol, true);
            
        }


        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser)
                .GetField("_axIWebBrowser2",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember(
                "Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] { Hide });
        }

    }
}
