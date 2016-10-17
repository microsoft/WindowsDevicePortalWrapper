//----------------------------------------------------------------------------------------------
// <copyright file="BindablePassword.xaml.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Interaction logic for BindablePassword.xaml
    /// </summary>
    public partial class BindablePassword : UserControl
    {
        //-------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePassword" /> class.
        /// </summary>
        public BindablePassword()
        {
            this.InitializeComponent();
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        // Dependency Properties
        //-------------------------------------------------------------------
        #region Dependency Properties
        #region Password Dependency Property
        /// <summary>
        /// Gets or sets the Password dependency property
        /// </summary>
        public SecureString Password
        {
            get
            {
                return (SecureString)GetValue(PasswordProperty);
            }

            set
            {
                this.SetValue(PasswordProperty, value);
            }
        }

        /// <summary>
        /// Password Dependeny Property static association
        /// </summary>
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                "Password",
                typeof(SecureString),
                typeof(BindablePassword),
                new PropertyMetadata(default(SecureString)));

        /// <summary>
        /// Forwards change events to the password box's secure string to the Password dependency property
        /// </summary>
        /// <param name="sender">object that originated the event</param>
        /// <param name="e">Parameters for the event</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.Password = ((PasswordBox)sender).SecurePassword;
        }
        #endregion // Password Dependency Property
        #endregion // Dependency Properties
    }
}
