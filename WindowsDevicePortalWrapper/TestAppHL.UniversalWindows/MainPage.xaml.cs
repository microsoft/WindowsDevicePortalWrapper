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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void RunTests()
        {
            this.testOutput.Text = string.Empty;

            DevicePortal portal = new DevicePortal(
                new DevicePortalConnection(
                    this.address.Text,
                    this.username.Text,
                    this.password.Text));
            //portal.ConnectionStatus += DevicePortal_ConnectionStatus;
            //portal.AppInstallStatus += DevicePortal_AppInstallStatus;

            StringBuilder sb = new StringBuilder();
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

            string deviceName = await portal.GetDeviceName();
            sb.Append("Device name: ");
            sb.AppendLine(deviceName);
            this.testOutput.Text = sb.ToString();

            //await portal.SetInterPupilaryDistance(67.5f);
            float ipd = await portal.GetInterPupilaryDistance();
            sb.Append("IPD: ");
            sb.AppendLine(ipd.ToString());
            this.testOutput.Text = sb.ToString();

            BatteryState batteryState = await portal.GetBatteryState();
            sb.Append("Battery level: ");
            sb.AppendLine(batteryState.Level.ToString());
            this.testOutput.Text = sb.ToString();

            PowerState powerState = await portal.GetPowerState();
            sb.Append("In low power state: ");
            sb.AppendLine(powerState.InLowPowerState.ToString());
            this.testOutput.Text = sb.ToString();

            this.runTests.IsEnabled = true;
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

        private void runTests_Click(object sender, RoutedEventArgs e)
        {
            this.runTests.IsEnabled = false;

            this.RunTests();
        }
    }
}
