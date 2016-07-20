using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using TestApp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestAppHL.UniversalWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DevicePortal portal;

        public MainPage()
        {
            this.InitializeComponent();

            portal = new DevicePortal(
                new DevicePortalConnection(
                    this.address.Text,
                    this.username.Text,
                    this.password.Text));
            //portal.ConnectionStatus += DevicePortal_ConnectionStatus;
            //portal.AppInstallStatus += DevicePortal_AppInstallStatus;
        }

        private async void ConnectToDevice()
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                sb.AppendLine("Connecting...");
                this.testOutput.Text = sb.ToString();
                string ssid = !string.IsNullOrWhiteSpace(this.ssid.Text) ? this.ssid.Text : null;
                string key = !string.IsNullOrWhiteSpace(this.networkKey.Text) ? this.networkKey.Text : null;
                bool updateConnection = this.updateDeviceConnection.IsChecked.HasValue ? this.updateDeviceConnection.IsChecked.Value : false;
                await portal.Connect(
                    ssid,
                    key,
                    updateConnection);

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
                this.testOutput.Text = sb.ToString();

                this.getName.IsEnabled = true;
                this.getBatteryLevel.IsEnabled = true;
                this.getPowerState.IsEnabled = true;
                this.getIpd.IsEnabled = true;
                this.setIpd.IsEnabled = true;
                this.ipdValue.IsEnabled = true;
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed device connection.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                this.testOutput.Text = sb.ToString();
            }

            this.connectToDevice.IsEnabled = true;
        }

        private void DevicePortal_AppInstallStatus(
            object sender,
            ApplicationInstallStatusEventArgs args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(testOutput.Text);
            sb.AppendLine(args.Message);
            this.testOutput.Text = sb.ToString();
        }

        private void DevicePortal_ConnectionStatus(
            object sender, 
            DeviceConnectionStatusEventArgs args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(testOutput.Text);
            sb.AppendLine(args.Message);
            this.testOutput.Text = sb.ToString();
        }

        private void connectToDevice_Click(object sender, RoutedEventArgs e)
        {
            this.getName.IsEnabled = false;
            this.getBatteryLevel.IsEnabled = false;
            this.getPowerState.IsEnabled = false;
            this.getIpd.IsEnabled = false;
            this.setIpd.IsEnabled = false;
            this.ipdValue.IsEnabled = false;

            this.connectToDevice.IsEnabled = false;

            this.ConnectToDevice();
        }

        private async void getDeviceName_Click(object sender, RoutedEventArgs e)
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                string deviceName = await portal.GetDeviceName();
                sb.Append("Device name: ");
                sb.AppendLine(deviceName);
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to get IPD.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void getBatteryLevel_Click(object sender, RoutedEventArgs e)
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                BatteryState batteryState = await portal.GetBatteryState();
                sb.Append("Battery level: ");
                sb.AppendLine(batteryState.Level.ToString());
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to get IPD.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void getPowerState_Click(object sender, RoutedEventArgs e)
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                PowerState powerState = await portal.GetPowerState();
                sb.Append("In low power state: ");
                sb.AppendLine(powerState.InLowPowerState.ToString());
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to get IPD.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void getIpd_Click(object sender, RoutedEventArgs e)
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                float ipd = await portal.GetInterPupilaryDistance();
                sb.Append("IPD: ");
                sb.AppendLine(ipd.ToString());
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to get IPD.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void setIpd_Click(object sender, RoutedEventArgs e)
        {
            bool clearOutput = this.clearOutput.IsChecked.HasValue ? this.clearOutput.IsChecked.Value : false;
            if (clearOutput)
            { 
                this.testOutput.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.testOutput.Text);

            try
            {
                float ipd = float.Parse(this.ipdValue.Text);
                sb.AppendLine("Setting IPD to : " + ipd.ToString());
                await portal.SetInterPupilaryDistance(ipd);
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to set IPD.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }
    }
}
