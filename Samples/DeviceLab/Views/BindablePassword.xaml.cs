using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace DeviceLab
{
    /// <summary>
    /// Interaction logic for BindablePassword.xaml
    /// </summary>
    public partial class BindablePassword : UserControl
    {
        public SecureString Password
        {
            get
            {
                return (SecureString)GetValue(PasswordProperty);
            }
            set
            {
                SetValue(PasswordProperty, value);
            }
        }

        public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePassword),
            new PropertyMetadata(default(SecureString)));

        public BindablePassword()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).SecurePassword;
        }
    }
}
