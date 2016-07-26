using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

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
            portal.ConnectionStatus += DevicePortal_ConnectionStatus;
            portal.AppInstallStatus += DevicePortal_AppInstallStatus;
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

                this.EnableTestControls(true);
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed device connection.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
                this.testOutput.Text = sb.ToString();
            }

            this.connectToDevice.IsEnabled = true;
        }

        private void EnableTestControls(bool enable)
        {
            this.rebootDevice.IsEnabled = enable;
            this.shutdownDevice.IsEnabled = enable;
            this.getName.IsEnabled = enable;
            this.nameValue.IsEnabled = enable;
            this.setName.IsEnabled = enable;
            this.getIpd.IsEnabled = enable;
            this.ipdValue.IsEnabled = enable;
            this.setIpd.IsEnabled = enable;
            this.getBatteryLevel.IsEnabled = enable;
            this.getPowerState.IsEnabled = enable;
            this.getThermalStage.IsEnabled = enable;
            this.getIPConfig.IsEnabled = enable;
            this.getWiFiInfo.IsEnabled = enable;
            this.takeMrcPhoto.IsEnabled = enable;
            this.photoHolograms.IsEnabled = enable;
            this.photoColor.IsEnabled = enable;
            this.startMrcRecording.IsEnabled = enable;
            this.videoHolograms.IsEnabled = enable;
            this.videoColor.IsEnabled = enable;
            this.videoMicrophone.IsEnabled = enable;
            this.videoAudio.IsEnabled = enable;
            this.stopMrcRecording.IsEnabled = enable;
            this.listMrcFiles.IsEnabled = enable;
            //this.installApplication.IsEnabled = enable;
            //this.appNameValue.IsEnabled = enable;
            //this.uninstallPreviousVersion.IsEnabled = enable;
            //this.packageNameValue.IsEnabled = enable;
            //this.browsePackages.IsEnabled = enable;
            //this.dependencyFolderValue.IsEnabled = enable;
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
            this.EnableTestControls(false);
            this.getWiFiInfo.IsEnabled = false;

            this.connectToDevice.IsEnabled = false;

            this.ConnectToDevice();
        }

        private async void RebootDevice_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Rebooting the device");
                await portal.Reboot();
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to reboot the device.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void ShutdownDevice_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Shutting down the device");
                await portal.Shutdown();
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to shutdown the device.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void GetDeviceName_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Failed to get device name.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void SetDeviceName_Click(object sender, RoutedEventArgs e)
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
                if (String.IsNullOrWhiteSpace(nameValue.Text))
                {
                    throw new InvalidOperationException("The device name must be a valid, non-whitespace string.");
                }

                sb.AppendLine("Setting device name to : " + nameValue.Text);
                sb.AppendLine(" The name change will be reflected after a reboot.");
                await portal.SetDeviceName(nameValue.Text, false);
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to set device name.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void GetBatteryLevel_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Failed to get battery level.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void GetPowerState_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Failed to get power state.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void GetIpd_Click(object sender, RoutedEventArgs e)
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

        private async void SetIpd_Click(object sender, RoutedEventArgs e)
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

        private async void GetThermalStage_Click(object sender, RoutedEventArgs e)
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
                ThermalStages thermalStage = await portal.GetThermalStage();
                sb.Append("Thermal stage: ");
                sb.AppendLine(thermalStage.ToString());
            }
            catch(Exception ex)
            {
                sb.AppendLine("Failed to get thermal stage.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void GetIPConfig_Click(object sender, RoutedEventArgs e)
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
                IpConfiguration ipconfig = await portal.GetIpConfig();
                sb.AppendLine("IP Configuration:");
                foreach (NetworkAdapterInfo adapterInfo in ipconfig.Adapters)
                {
                    sb.Append(" ");
                    sb.AppendLine(adapterInfo.Description);
                    sb.Append("  MAC address :");
                    sb.AppendLine(adapterInfo.MacAddress);
                    foreach (IpAddressInfo address in adapterInfo.IpAddresses)
                    {
                        sb.Append ("  IP address :");
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

            this.testOutput.Text = sb.ToString();
        }

        private async void GetWifiInfo_Click(object sender, RoutedEventArgs e)
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
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to get WiFi info.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void TakeMrcPhoto_Click(object sender, RoutedEventArgs e)
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
                bool holograms = photoHolograms.IsChecked.Value;
                bool color = photoColor.IsChecked.Value;

                sb.AppendLine("Taking MRC photo");
                sb.AppendLine("Holograms: " + (holograms ? "On" : "Off"));
                sb.AppendLine("Color    : " + (color ? "On" : "Off"));
                await portal.TakeMrcPhoto(
                    holograms, 
                    color);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to take MRC photo.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void ListMrcFiles_Click(object sender, RoutedEventArgs e)
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
                MrcFileList fileList = await portal.GetMrcFileList();
                sb.AppendLine("MRC Files: " + fileList.Files.Count.ToString());
                foreach (MrcFileInformation fileInfo in fileList.Files)
                {
                    sb.Append(" Name: ");
                    sb.AppendLine(fileInfo.FileName);
                    sb.Append(" Date: ");
                    sb.AppendLine(fileInfo.Created.ToString());
                    sb.Append(" Size: ");
                    sb.AppendLine(fileInfo.FileSize.ToString());
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to MRC file list.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void StartMrcRecording_Click(object sender, RoutedEventArgs e)
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
                bool holograms = videoHolograms.IsChecked.Value;
                bool color = videoColor.IsChecked.Value;
                bool microphone = videoMicrophone.IsChecked.Value;
                bool audio = videoAudio.IsChecked.Value;

                sb.AppendLine("Starting MRC recording");
                sb.AppendLine("Holograms : " + (holograms ? "On" : "Off"));
                sb.AppendLine("Color     : " + (color ? "On" : "Off"));
                sb.AppendLine("Microphone: " + (microphone ? "On" : "Off"));
                sb.AppendLine("App audio : " + (audio ? "On" : "Off"));
                await portal.StartMrcRecording(
                    holograms,
                    color,
                    microphone,
                    audio);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to start MRC recording.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void StopMrcRecording_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine("Stopping MRC recording");
                await portal.StopMrcRecording();
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to stop MRC recording.");
                sb.AppendLine(ex.GetType().ToString() + " - " + ex.Message);
            }

            this.testOutput.Text = sb.ToString();
        }

        private async void InstallApplication_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BrowsePackages_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BrowseDependencies_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
