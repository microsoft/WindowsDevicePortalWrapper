//----------------------------------------------------------------------------------------------
// <copyright file="AthensBluetooth.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Threading;
namespace Microsoft.Tools.WindowsDevicePortal
{

    public partial class DevicePortal
    {
        public static readonly string AvailableBluetoothDevicesApi = "api/iot/bt/getavailable";
        public static readonly string PairedBluetoothDevicesApi = "api/iot/bt/getpaired";
        public static readonly string PairBluetoothDevicesApi = "api/iot/bt/pair";
        public static readonly string UnpairBluetoothDevicesApi = "api/iot/bt/unpair";
        public static readonly string EnableDiscoverableModeBluetoothDeviceApi = "api/iot/bt/discoverable";

        /// <summary>
        /// Web socket to get list of bluetooth devices.
        /// </summary>
        private WebSocket<AvailableBluetoothDevicesInfo> BluetoothWebSocket;

        public AvailableBluetoothDevicesInfo GetAvailableBluetoothDevicesInfo()
        {
            return GetBluetoothDevicesInfo(AvailableBluetoothDevicesApi);
        }

        public AvailableBluetoothDevicesInfo GetPairedBluetoothDevicesInfo()
        {
            return GetBluetoothDevicesInfo(PairedBluetoothDevicesApi);
        }

        /// <summary>
        /// Gets or sets the bluetooth list of devices received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<AvailableBluetoothDevicesInfo> BluetoothDeviceListReceived;

        /// <summary>
        /// Gets the list of available bluetooth devices.
        /// </summary>
        /// <returns>AvailableBluetoothDevicesInfo object containing the list of bluetooth devices is returned.</returns>
        public AvailableBluetoothDevicesInfo GetBluetoothDevicesInfo(string bluetoothApi)
        {    
            AvailableBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent BluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<AvailableBluetoothDevicesInfo> bluetoothReceivedHandler =
                delegate (DevicePortal sender, WebSocketMessageReceivedEventArgs<AvailableBluetoothDevicesInfo> BluetoothArgs)
                {
                    if (BluetoothArgs.Message != null)
                    {
                        bluetooth = BluetoothArgs.Message;
                        BluetoothReceived.Set();
                    }
                };
            this.BluetoothDeviceListReceived += bluetoothReceivedHandler;
       
            Task startListeningForBluetooth = this.StartListeningForBluetooth(bluetoothApi);
            startListeningForBluetooth.Wait();

            BluetoothReceived.WaitOne();

            Task stopListeningForBluetooth = this.StopListeningForBluetooth();
            stopListeningForBluetooth.Wait();

            this.BluetoothDeviceListReceived -= bluetoothReceivedHandler;
            return bluetooth;
        }

    

        /// <summary>
        /// Starts listening for bluetooth list of devices returned from the BluetoothDeviceListReceived handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForBluetooth(string bluetoothApi)
        {
            if (this.BluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.BluetoothWebSocket = new WebSocket<AvailableBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.BluetoothWebSocket = new WebSocket<AvailableBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.BluetoothWebSocket.WebSocketMessageReceived += this.BluetoothReceivedHandler;
            }
            else
            {
                if (this.BluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.BluetoothWebSocket.StartListeningForMessages(bluetoothApi);
        }

        /// <summary>
        /// Stop listening for the list of bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for bluetooth devices and disconnecting from the websocket .</returns>
        public async Task StopListeningForBluetooth()
        {
            if (this.BluetoothWebSocket == null || !this.BluetoothWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.BluetoothWebSocket.StopListeningForMessages();
        }

        /// <summary>
        /// Handler for the list of bluetooth devices received event to pass it on to the BluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{AvailableBluetoothDevicesInfo}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void BluetoothReceivedHandler(
            WebSocket<AvailableBluetoothDevicesInfo> sender,
            WebSocketMessageReceivedEventArgs<AvailableBluetoothDevicesInfo> args)
        {
            if (args.Message != null)
            {
                this.BluetoothDeviceListReceived?.Invoke(
                            this,
                            args);
            }
        }

        #region Data contract

        /// <summary>
        /// Remote Settings Status information.
        /// </summary>
        [DataContract]
        public class AvailableBluetoothDevicesInfo
        {
            /// <summary>
            /// Returns true if the service is running
            /// </summary>
            [DataMember(Name = "AvailableDevices")]
            public AvailableDevice[] AvailableDevices;
        }
        public partial class AvailableDevice
        {
            /// <summary>
            /// Returns true if the service is running
            /// </summary>
            [DataMember(Name = "ID")]
            public string ID { get; set; }

            /// <summary>
            /// Returns true if the service is running
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; set; }
        }
        #endregion // Data contract
    }
}
