using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NoResolver.WPF.Views
{
    /// <summary>
    /// WPF Password box doesn't allow you to bind the password property, mostly for security reasons
    /// 
    /// This class masks the password on entry, binds it to a string
    /// While ideally we'd maintain a SecureString throughout, apparently this is now deprecated
    /// Credentials are stored securely in Windows Credential Manager now anyways
    /// 
    /// https://www.youtube.com/watch?v=G9niOcc5ssw
    /// https://github.com/SingletonSean/wpf-mvvm-password-box/blob/master/PasswordBoxMVVM/Components/BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        private bool _isPasswordChanging;

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PasswordPropertyChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        private static void PasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox passwordBox)
            {
                passwordBox.UpdatePassword();
            }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _isPasswordChanging = true;
            Password = passwordBox.Password;
            _isPasswordChanging = false;
        }

        private void UpdatePassword()
        {
            if (!_isPasswordChanging)
            {
                passwordBox.Password = Password;
            }
        }
    }
}
