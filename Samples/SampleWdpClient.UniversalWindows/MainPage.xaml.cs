using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace SampleWdpClient.UniversalWindows
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The device portal to which we are connecting.
        /// </summary>
        private DevicePortal portal;

        private Certificate certificate;

        /// <summary>
        /// The main page constructor.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.EnableDeviceControls(false);
        }

        /// <summary>
        /// TextChanged handler for the address text box.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableConnectButton();
        }

        /// <summary>
        /// If specified in the UI, clears the test output display, otherwise does nothing.
        /// </summary>
        private void ClearOutput()
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            {
                this.commandOutput.Text = string.Empty;
            }
        }

        /// <summary>
        /// Click handler for the connectToDevice button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void ConnectToDevice_Click(object sender, RoutedEventArgs e)
        {
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            this.ClearOutput();

            bool allowUntrusted = this.allowUntrustedCheckbox.IsChecked.Value;

            portal = new DevicePortal(
                new DefaultDevicePortalConnection(
                    this.address.Text,
                    this.username.Text,
                    this.password.Password));

            StringBuilder sb = new StringBuilder();

            sb.Append(this.commandOutput.Text);
            sb.AppendLine("Connecting...");
            this.commandOutput.Text = sb.ToString();
            portal.ConnectionStatus += (portal, connectArgs) =>
            {
                if (connectArgs.Status == DeviceConnectionStatus.Connected)
                {
                    sb.Append("Connected to: ");
                    sb.AppendLine(portal.Address);
                    sb.Append("OS version: ");
                    sb.AppendLine(portal.OperatingSystemVersion);
                    sb.Append("Device family: ");
                    sb.AppendLine(portal.DeviceFamily);
                    sb.Append("Platform: ");
                    sb.AppendLine(String.Format("{0} ({1})",
                        portal.PlatformName,
                        portal.Platform.ToString()));
                }
                else if (connectArgs.Status == DeviceConnectionStatus.Failed)
                {
                    sb.AppendLine("Failed to connect to the device.");
                    sb.AppendLine(connectArgs.Message);
                }
            };

            try
            {
                // If the user wants to allow untrusted connections, make a call to GetRootDeviceCertificate
                // with acceptUntrustedCerts set to true. This will enable untrusted connections for the
                // remainder of this session.
                if (allowUntrusted)
                {
                    this.certificate = await portal.GetRootDeviceCertificateAsync(true);
                }
                await portal.ConnectAsync(manualCertificate: this.certificate);
            }
            catch (Exception exception)
            {
                sb.AppendLine(exception.Message);
            }

            this.commandOutput.Text = sb.ToString();
            EnableDeviceControls(true);
            EnableConnectionControls(true);
        }

        /// <summary>
        /// Enables or disables the Connect button based on the current state of the
        /// Address, User name and Password fields.
        /// </summary>
        private void EnableConnectButton()
        {
            bool enable = (!string.IsNullOrWhiteSpace(this.address.Text) &&
                        !string.IsNullOrWhiteSpace(this.username.Text) &&
                        !string.IsNullOrWhiteSpace(this.password.Password));

            this.connectToDevice.IsEnabled = enable;
        }

        /// <summary>
        /// Sets the IsEnabled property appropriately for the connection controls.
        /// </summary>
        /// <param name="enable">True to enable the controls, false to disable them.</param>
        private void EnableConnectionControls(bool enable)
        {
            this.address.IsEnabled = enable;
            this.username.IsEnabled = enable;
            this.password.IsEnabled = enable;

            this.connectToDevice.IsEnabled = enable;
        }

        /// <summary>
        /// Sets the IsEnabled property appropriately for the device command controls.
        /// </summary>
        /// <param name="enable">True to enable the controls, false to disable them.</param>
        private void EnableDeviceControls(bool enable)
        {
            this.rebootDevice.IsEnabled = enable;
            this.shutdownDevice.IsEnabled = enable;

            this.getIPConfig.IsEnabled = enable;
            this.getWiFiInfo.IsEnabled = enable;
        }

        /// <summary>
        /// Click handler for the getIpConfig button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void GetIPConfig_Click(object sender, RoutedEventArgs e)
        {
            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            sb.Append(commandOutput.Text);
            sb.AppendLine("Getting IP configuration...");
            commandOutput.Text = sb.ToString();

            try
            {
                IpConfiguration ipconfig = await portal.GetIpConfigAsync();

                foreach (NetworkAdapterInfo adapterInfo in ipconfig.Adapters)
                {
                    sb.Append(" ");
                    sb.AppendLine(adapterInfo.Description);
                    sb.Append("  MAC address :");
                    sb.AppendLine(adapterInfo.MacAddress);
                    foreach (IpAddressInfo address in adapterInfo.IpAddresses)
                    {
                        sb.Append("  IP address :");
                        sb.AppendLine(address.Address);
                    }
                    sb.Append("  DHCP address :");
                    sb.AppendLine(adapterInfo.Dhcp.Address.Address);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to get IP config info.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            commandOutput.Text = sb.ToString();
            EnableDeviceControls(true);
            EnableConnectionControls(true);
        }

        /// <summary>
        /// Click handler for the getWifiInfo button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void GetWifiInfo_Click(object sender, RoutedEventArgs e)
        {
            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();

            sb.Append(commandOutput.Text);
            sb.AppendLine("Getting WiFi interfaces and networks...");
            commandOutput.Text = sb.ToString();

            try
            {
                WifiInterfaces wifiInterfaces = await portal.GetWifiInterfacesAsync();
                sb.AppendLine("WiFi Interfaces:");
                foreach (WifiInterface wifiInterface in wifiInterfaces.Interfaces)
                {
                    sb.Append(" ");
                    sb.AppendLine(wifiInterface.Description);
                    sb.Append("  GUID: ");
                    sb.AppendLine(wifiInterface.Guid.ToString());

                    WifiNetworks wifiNetworks = await portal.GetWifiNetworksAsync(wifiInterface.Guid);
                    sb.AppendLine("  Networks:");
                    foreach (WifiNetworkInfo network in wifiNetworks.AvailableNetworks)
                    {
                        sb.Append("   SSID: ");
                        sb.AppendLine(network.Ssid);
                        sb.Append("   Profile name: ");
                        sb.AppendLine(network.ProfileName);
                        sb.Append("   is connected: ");
                        sb.AppendLine(network.IsConnected.ToString());
                        sb.Append("   Channel: ");
                        sb.AppendLine(network.Channel.ToString());
                        sb.Append("   Authentication algorithm: ");
                        sb.AppendLine(network.AuthenticationAlgorithm);
                        sb.Append("   Signal quality: ");
                        sb.AppendLine(network.SignalQuality.ToString());
                    }
                };
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to get WiFi info.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            commandOutput.Text = sb.ToString();
            EnableDeviceControls(true);
            EnableConnectionControls(true);
        }

        /// <summary>
        /// PasswordChanged handler for the password text box.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EnableConnectButton();
        }

        /// <summary>
        /// Click handler for the rebootDevice button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void RebootDevice_Click(object sender, RoutedEventArgs e)
        {
            bool reenableDeviceControls = false;

            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();

            sb.Append(commandOutput.Text);
            sb.AppendLine("Rebooting the device");
            commandOutput.Text = sb.ToString();

            try
            {
                await portal.RebootAsync();
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to reboot the device.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                reenableDeviceControls = true;
            }

            commandOutput.Text = sb.ToString();
            EnableDeviceControls(reenableDeviceControls);
            EnableConnectionControls(true);
        }

        /// <summary>
        /// Click handler for the shutdownDevice button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void ShutdownDevice_Click(object sender, RoutedEventArgs e)
        {
            bool reenableDeviceControls = false;

            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            sb.Append(commandOutput.Text);
            sb.AppendLine("Shutting down the device");
            commandOutput.Text = sb.ToString();
            try
            {
                await portal.ShutdownAsync();
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to shut down the device.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                reenableDeviceControls = true;
            }

            commandOutput.Text = sb.ToString();
            EnableDeviceControls(reenableDeviceControls);
            EnableConnectionControls(true);
        }

        /// <summary>
        /// TextChanged handler for the username text box.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableConnectButton();
        }

        /// <summary>
        /// Loads a cert file for cert validation.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private async void LoadCertificate_Click(object sender, RoutedEventArgs e)
        {
            await LoadCertificate();
        }

        /// <summary>
        /// Loads a certificates asynchronously (runs on the UI thread).
        /// </summary>
        /// <returns></returns>
        private async Task LoadCertificate()
        {
            try
            {
                FileOpenPicker filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".cer");

                StorageFile file = await filePicker.PickSingleFileAsync();

                if (file != null)
                {
                    IBuffer cerBlob = await FileIO.ReadBufferAsync(file);

                    if (cerBlob != null)
                    {
                        certificate = new Certificate(cerBlob);
                    }
                }
            }
            catch (Exception exception)
            {
                this.commandOutput.Text = "Failed to get cert file: " + exception.Message;
            }
        }
    }
}