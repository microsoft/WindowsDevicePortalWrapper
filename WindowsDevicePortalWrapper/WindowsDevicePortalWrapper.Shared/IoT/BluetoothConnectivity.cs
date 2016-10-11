//----------------------------------------------------------------------------------------------
// <copyright file="BluetoothConnectivity.cs" company="Microsoft Corporation">
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
    /// <content>
    /// Wrappers for Bluetooth Connectivity.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Available bluetooth device list API.
        /// </summary>
        public static readonly string AvailableBluetoothDevicesApi = "api/iot/bt/getavailable";

        /// <summary>
        /// Paired bluetooth devices list API.
        /// </summary>
        public static readonly string PairedBluetoothDevicesApi = "api/iot/bt/getpaired";

        /// <summary>
        /// API to pair a bluetooth device.
        /// </summary>
        public static readonly string PairBluetoothDevicesApi = "api/iot/bt/pair";

        /// <summary>
        /// API to unpair a bluetooth device.
        /// </summary>
        public static readonly string UnpairBluetoothDevicesApi = "api/iot/bt/unpair";

        /// <summary>
        /// Web socket to get list of bluetooth devices.
        /// </summary>
        private WebSocket<AvailableBluetoothDevicesInfo> BluetoothWebSocket;

        /// <summary>
        /// Gets or sets the list of bluetooth devices received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<AvailableBluetoothDevicesInfo> BluetoothDeviceListReceived;

        /// <summary>
        /// Web socket to get list of paired bluetooth devices.
        /// </summary>
        private WebSocket<PairedBluetoothDevicesInfo> PairedBluetoothWebSocket;

        /// <summary>
        /// Gets or sets the list of paired bluetooth devices received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<PairedBluetoothDevicesInfo> PairedBluetoothDeviceListReceived;

        /// <summary>
        /// Web socket to get the results of bluetooth device pairing.
        /// </summary>
        private WebSocket<PairBluetoothDevicesInfo> PairBluetoothWebSocket;

        /// <summary>
        /// Gets the results of pairing bluetooth device received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<PairBluetoothDevicesInfo> PairBluetoothDeviceListReceived;

        /// <summary>
        /// Gets the available bluetooth device information.
        /// </summary>
        /// <returns>List of Available bluetooth devices</returns>
        public AvailableBluetoothDevicesInfo GetAvailableBluetoothDevicesInfo()
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

            Task startListeningForBluetooth = this.StartListeningForBluetoothAsync(AvailableBluetoothDevicesApi);
            startListeningForBluetooth.Wait();

            BluetoothReceived.WaitOne();

            Task stopListeningForBluetooth = this.StopListeningForBluetoothAsync();
            stopListeningForBluetooth.Wait();

            this.BluetoothDeviceListReceived -= bluetoothReceivedHandler;
            return bluetooth;
        }


        /// <summary>
        /// Gets the paired bluetooth device information.
        /// </summary>
        /// <returns>List of paired bluetooth devices</returns>
        public PairedBluetoothDevicesInfo GetPairedBluetoothDevicesInfo()
        {
            PairedBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent PairedBluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<PairedBluetoothDevicesInfo> pairedBluetoothReceivedHandler =
                delegate (DevicePortal sender, WebSocketMessageReceivedEventArgs<PairedBluetoothDevicesInfo> BluetoothArgs)
                {
                    if (BluetoothArgs.Message != null)
                    {
                        bluetooth = BluetoothArgs.Message;
                        PairedBluetoothReceived.Set();
                    }
                };
            this.PairedBluetoothDeviceListReceived += pairedBluetoothReceivedHandler;

            Task startListeningForPairedBluetooth = this.StartListeningForPairedBluetoothAsync(PairedBluetoothDevicesApi);
            startListeningForPairedBluetooth.Wait();

            PairedBluetoothReceived.WaitOne();

            Task stopListeningForPairedBluetooth = this.StopListeningForPairedBluetoothAsync();
            stopListeningForPairedBluetooth.Wait();

            this.PairedBluetoothDeviceListReceived -= pairedBluetoothReceivedHandler;
            return bluetooth;
        }

        /// <summary>
        /// Gets the results for pairing a bluetooth device.
        /// </summary>
        /// <returns>Results of pairing a bluetooth device</returns>
        public PairBluetoothDevicesInfo GetPairBluetoothDevicesInfo(string deviceId)
        {
            PairBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent PairBluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<PairBluetoothDevicesInfo> pairBluetoothReceivedHandler =
                delegate (DevicePortal sender, WebSocketMessageReceivedEventArgs<PairBluetoothDevicesInfo> BluetoothArgs)
                {
                    if (BluetoothArgs.Message != null)
                    {
                        bluetooth = BluetoothArgs.Message;
                        PairBluetoothReceived.Set();
                    }
                };
            this.PairBluetoothDeviceListReceived += pairBluetoothReceivedHandler;

            Task startListeningForPairBluetooth = this.StartListeningForPairBluetoothAsync(PairBluetoothDevicesApi, string.Format("deviceId={0}", Utilities.Hex64Encode(deviceId)));
            startListeningForPairBluetooth.Wait();

            PairBluetoothReceived.WaitOne();

            Task stopListeningForPairBluetooth = this.StopListeningForPairBluetoothAsync();
            stopListeningForPairBluetooth.Wait();

            this.PairBluetoothDeviceListReceived -= pairBluetoothReceivedHandler;
            return bluetooth;
        }

        /// <summary>
        /// Starts listening for bluetooth list of devices returned from the BluetoothDeviceListReceived handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForBluetoothAsync(string bluetoothApi)
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

            await this.BluetoothWebSocket.StartListeningForMessagesAsync(bluetoothApi);
        }

        /// <summary>
        /// Stop listening for the list of bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for bluetooth devices and disconnecting from the websocket .</returns>
        public async Task StopListeningForBluetoothAsync()
        {
            if (this.BluetoothWebSocket == null || !this.BluetoothWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.BluetoothWebSocket.StopListeningForMessagesAsync();
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

        /// <summary>
        /// Starts listening for paired bluetooth list of devices returned from the PairedBluetoothDeviceListReceived handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForPairedBluetoothAsync(string bluetoothApi)
        {
            if (this.PairedBluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.PairedBluetoothWebSocket = new WebSocket<PairedBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.PairedBluetoothWebSocket = new WebSocket<PairedBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.PairedBluetoothWebSocket.WebSocketMessageReceived += this.PairedBluetoothReceivedHandler;
            }
            else
            {
                if (this.PairedBluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.PairedBluetoothWebSocket.StartListeningForMessagesAsync(bluetoothApi);
        }

        /// <summary>
        /// Stop listening for the list of paired bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for bluetooth devices and disconnecting from the websocket .</returns>
        public async Task StopListeningForPairedBluetoothAsync()
        {
            if (this.PairedBluetoothWebSocket == null || !this.PairedBluetoothWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.PairedBluetoothWebSocket.StopListeningForMessagesAsync();
        }

        /// <summary>
        /// Handler for the list of paired bluetooth devices received event to pass it on to the PairedBluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{PairedBluetoothDevicesInfo}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void PairedBluetoothReceivedHandler(
            WebSocket<PairedBluetoothDevicesInfo> sender,
            WebSocketMessageReceivedEventArgs<PairedBluetoothDevicesInfo> args)
        {
            if (args.Message != null)
            {
                this.PairedBluetoothDeviceListReceived?.Invoke(
                            this,
                            args);
            }
        }

        /// <summary>
        /// Starts listening for the result to pair the bluetooth device returned from the PairBluetoothDeviceListReceived handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForPairBluetoothAsync(string bluetoothApi, string payload)
        {
            if (this.PairBluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.PairBluetoothWebSocket = new WebSocket<PairBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.PairBluetoothWebSocket = new WebSocket<PairBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.PairBluetoothWebSocket.WebSocketMessageReceived += this.PairBluetoothReceivedHandler;
            }
            else
            {
                if (this.PairBluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.PairBluetoothWebSocket.StartListeningForMessagesAsync(bluetoothApi, payload);
        }

        /// <summary>
        /// Stop listening for the results for pairing bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for the bluetooth device and disconnecting from the websocket .</returns>
        public async Task StopListeningForPairBluetoothAsync()
        {
            if (this.PairBluetoothWebSocket == null || !this.PairBluetoothWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.PairBluetoothWebSocket.StopListeningForMessagesAsync();
        }

        /// <summary>
        /// Handler for the results to pair a bluetooth device received event to pass it on to the PairBluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{PairBluetoothDevicesInfo}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void PairBluetoothReceivedHandler(
            WebSocket<PairBluetoothDevicesInfo> sender,
            WebSocketMessageReceivedEventArgs<PairBluetoothDevicesInfo> args)
        {
            if (args.Message != null)
            {
                this.PairBluetoothDeviceListReceived?.Invoke(
                            this,
                            args);
            }
        }

        /// <summary>
        /// Unpairs the bluetooth device.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<ErrorInformation> UnPairBluetoothDeviceAsync(string deviceId)
        {
            return await this.PostAsync<ErrorInformation>(
                 UnpairBluetoothDevicesApi,
                string.Format("deviceId={0}", Utilities.Hex64Encode(deviceId)));
        }

        #region Data contract

        /// <summary>
        /// List of available bluetooth devices.
        /// </summary>
        [DataContract]
        public class AvailableBluetoothDevicesInfo 
        {
            /// <summary>
            /// Returns a list of available devices
            /// </summary>
            [DataMember(Name = "AvailableDevices")]
            public BluetoothDeviceInfo[] AvailableDevices;
        }
        public class BluetoothDeviceInfo 
        {
            /// <summary>
            /// Returns the bluetooth device ID
            /// </summary>
            [DataMember(Name = "ID")]
            public string ID { get; private set; }

            /// <summary>
            /// Returns the bluetooth device name
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }
        }

        /// <summary>
        /// List of paired bluetooth devices.
        /// </summary>
        [DataContract]
        public class PairedBluetoothDevicesInfo 
        {
            /// <summary>
            /// Returns a list of paired devices
            /// </summary>
            [DataMember(Name = "PairedDevices")]
            public BluetoothDeviceInfo[] PairedDevices;
        }

        /// <summary>
        /// Information about device to be paired.
        /// </summary>
        [DataContract]
        public class PairBluetoothDevicesInfo 
        {
            /// <summary>
            /// Returns the pair results
            /// </summary>
            [DataMember(Name = "PairResult")]
            public PairResult PairResult;
        }

        public class PairResult
        {
            /// <summary>
            /// Returns the result about the device pairing
            /// </summary>
            [DataMember(Name = "Result")]
            public string Result { get; private set; }

            /// <summary>
            /// Returns the deviceId to be paired
            /// </summary>
            [DataMember(Name = "deviceId")]
            public string deviceId { get; private set; }

            /// <summary>
            /// Returns the pin
            /// </summary>
            [DataMember(Name = "Pin")]
            public string Pin { get; private set; }
        }
        #endregion // Data contract
    }
}
