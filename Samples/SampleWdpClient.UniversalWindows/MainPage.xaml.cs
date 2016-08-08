using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

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
        private void ConnectToDevice_Click(object sender, RoutedEventArgs e)
        {
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            this.ClearOutput();

            portal = new DevicePortal(
                new DefaultDevicePortalConnection(
                    this.address.Text,
                    this.username.Text,
                    this.password.Password));

            StringBuilder sb = new StringBuilder();
            Task connectTask = new Task(
                async () =>
                {
                    sb.Append(this.MarshalGetCommandOutput());
                    sb.AppendLine("Connecting...");
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    portal.ConnectionStatus += (portal, connectArgs) =>
                    {
                        if (connectArgs.Status == DeviceConnectionStatus.Connected) { 
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
                    
                        await portal.Connect();

                      
                });

            Task continuationTask = connectTask.ContinueWith(
                (t) =>
                {
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    this.MarshalEnableDeviceControls(true);
                    this.MarshalEnableConnectionControls(true);
                });

            connectTask.Start();
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
        private void GetIPConfig_Click(object sender, RoutedEventArgs e)
        {
            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            Task getTask = new Task( 
                async () =>
                {
                    sb.Append(this.MarshalGetCommandOutput());
                    sb.AppendLine("Getting IP configuration...");
                    this.MarshalUpdateCommandOutput(sb.ToString());

                    try
                    {
                        IpConfiguration ipconfig = await portal.GetIpConfig();

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
                    catch(Exception ex)
                    {
                        sb.AppendLine("Failed to get IP config info.");
                        sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                    }

                });

            Task continuationTask = getTask.ContinueWith(
                (t) =>
                {
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    this.MarshalEnableDeviceControls(true);
                    this.MarshalEnableConnectionControls(true);
                });

            getTask.Start();
        }

        /// <summary>
        /// Click handler for the getWifiInfo button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private void GetWifiInfo_Click(object sender, RoutedEventArgs e)
        {
            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            Task getTask = new Task(
                async () =>
                {
                    sb.Append(this.MarshalGetCommandOutput());
                    sb.AppendLine("Getting WiFi interfaces and networks...");
                    this.MarshalUpdateCommandOutput(sb.ToString());

                    try
                    {
                        WifiInterfaces wifiInterfaces = await portal.GetWifiInterfaces();
                        sb.AppendLine("WiFi Interfaces:");
                        foreach (WifiInterface wifiInterface in wifiInterfaces.Interfaces)
                        {
                            sb.Append(" ");
                            sb.AppendLine(wifiInterface.Description);
                            sb.Append("  GUID: ");
                            sb.AppendLine(wifiInterface.Guid.ToString());

                            WifiNetworks wifiNetworks = await portal.GetWifiNetworks(wifiInterface.Guid);
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
                    catch(Exception ex)
                    {
                        sb.AppendLine("Failed to get WiFi info.");
                        sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                    }
                });

            Task continuationTask = getTask.ContinueWith(
                (t) =>
                {
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    this.MarshalEnableDeviceControls(true);
                    this.MarshalEnableConnectionControls(true);
                });

            getTask.Start();
        }

        /// <summary>
        /// Executes the EnabledConnectionControls method on the UI thread.
        /// </summary>
        /// <param name="enable">True to enable the controls, false to disable them.</param>
        private void  MarshalEnableConnectionControls(bool enable)
        {
            Task t = this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    this.EnableConnectionControls(enable);
                }).AsTask();
            t.Wait();
        }

        /// <summary>
        /// Executes the EnabledDeviceControls method on the UI thread.
        /// </summary>
        /// <param name="enable">True to enable the controls, false to disable them.</param>
        private void  MarshalEnableDeviceControls(bool enable)
        {
            Task t = this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    this.EnableDeviceControls(enable);
                }).AsTask();
            t.Wait();
        }

        /// <summary>
        /// Executes the fetching of the text displayed in the command output UI element on the UI thread.
        /// </summary>
        /// <returns>The contents of the command output UI element.</returns>
        private string MarshalGetCommandOutput()
        {
            string output = string.Empty;

            Task t = this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    output = this.commandOutput.Text;
                }).AsTask();
            t.Wait();

            return output;
        }

        /// <summary>
        /// Executes the update of the text displayed in the command output UI element ont he UI thread.
        /// </summary>
        /// <param name="output">The text to display in the command output UI element.</param>
        private void MarshalUpdateCommandOutput(string output)
        {
            Task t = this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    this.commandOutput.Text = output;
                }).AsTask();
            t.Wait();
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
        private void RebootDevice_Click(object sender, RoutedEventArgs e)
        {
            bool reenableDeviceControls = false;

            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            Task rebootTask = new Task(
                async () =>
                {
                    sb.Append(this.MarshalGetCommandOutput());
                    sb.AppendLine("Rebooting the device");
                    this.MarshalUpdateCommandOutput(sb.ToString());

                    try
                    {
                        await portal.Reboot();
                    }
                    catch(Exception ex)
                    {
                        sb.AppendLine("Failed to reboot the device.");
                        sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                        reenableDeviceControls = true;
                    }
                });

            Task continuationTask = rebootTask.ContinueWith(
                (t) =>
                {
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    this.MarshalEnableDeviceControls(reenableDeviceControls);
                    this.MarshalEnableConnectionControls(true);
                });

            rebootTask.Start();
        }

        /// <summary>
        /// Click handler for the shutdownDevice button.
        /// </summary>
        /// <param name="sender">The caller of this method.</param>
        /// <param name="e">The arguments associated with this event.</param>
        private void ShutdownDevice_Click(object sender, RoutedEventArgs e)
        {
            bool reenableDeviceControls = false;

            this.ClearOutput();
            this.EnableConnectionControls(false);
            this.EnableDeviceControls(false);

            StringBuilder sb = new StringBuilder();
            Task shutdownTask = new Task(
                async () =>
                {
                    sb.Append(this.MarshalGetCommandOutput());
                    sb.AppendLine("Shutting down the device");
                    this.MarshalUpdateCommandOutput(sb.ToString());

                    try
                    {
                        await portal.Shutdown();
                    }
                    catch(Exception ex)
                    {
                        sb.AppendLine("Failed to shut down the device.");
                        sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                        reenableDeviceControls = true;
                    }
                });

            Task continuationTask = shutdownTask.ContinueWith(
                (t) =>
                {
                    this.MarshalUpdateCommandOutput(sb.ToString());
                    this.MarshalEnableDeviceControls(reenableDeviceControls);
                    this.MarshalEnableConnectionControls(true);
                });

            shutdownTask.Start();
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
    }
}
