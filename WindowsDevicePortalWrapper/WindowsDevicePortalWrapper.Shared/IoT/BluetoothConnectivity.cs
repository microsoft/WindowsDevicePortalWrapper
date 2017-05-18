//----------------------------------------------------------------------------------------------
// <copyright file="BluetoothConnectivity.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
        private WebSocket<AvailableBluetoothDevicesInfo> bluetoothWebSocket;

        /// <summary>
        /// Web socket to get list of paired bluetooth devices.
        /// </summary>
        private WebSocket<PairedBluetoothDevicesInfo> pairedBluetoothWebSocket;

        /// <summary>
        /// Web socket to get the results of bluetooth device pairing.
        /// </summary>
        private WebSocket<PairBluetoothDevicesInfo> pairBluetoothWebSocket;

        /// <summary>
        /// Gets or sets the list of bluetooth devices received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<AvailableBluetoothDevicesInfo> BluetoothDeviceListReceived;

        /// <summary>
        /// Gets or sets the list of paired bluetooth devices received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<PairedBluetoothDevicesInfo> PairedBluetoothDeviceListReceived;

        /// <summary>
        /// Gets the results of pairing bluetooth device received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<PairBluetoothDevicesInfo> PairBluetoothDeviceListReceived;

        /// <summary>
        /// Gets the available bluetooth device information.
        /// </summary>
        /// <returns>List of Available bluetooth devices</returns>
        public async Task<AvailableBluetoothDevicesInfo> GetAvailableBluetoothDevicesInfoAsync()
        {
            AvailableBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent bluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<AvailableBluetoothDevicesInfo> bluetoothReceivedHandler =
                delegate(DevicePortal sender, WebSocketMessageReceivedEventArgs<AvailableBluetoothDevicesInfo> bluetoothArgs)
                {
                    if (bluetoothArgs.Message != null)
                    {
                        bluetooth = bluetoothArgs.Message;
                        bluetoothReceived.Set();
                    }
                };
            this.BluetoothDeviceListReceived += bluetoothReceivedHandler;

            await this.StartListeningForBluetoothAsync(AvailableBluetoothDevicesApi);

            bluetoothReceived.WaitOne();

            await this.StopListeningForBluetoothAsync();

            this.BluetoothDeviceListReceived -= bluetoothReceivedHandler;
            return bluetooth;
        }

        /// <summary>
        /// Gets the paired bluetooth device information.
        /// </summary>
        /// <returns>List of paired bluetooth devices</returns>
        public async Task<PairedBluetoothDevicesInfo> GetPairedBluetoothDevicesInfoAsync()
        {
            PairedBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent pairedBluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<PairedBluetoothDevicesInfo> pairedBluetoothReceivedHandler =
                delegate(DevicePortal sender, WebSocketMessageReceivedEventArgs<PairedBluetoothDevicesInfo> bluetoothArgs)
                {
                    if (bluetoothArgs.Message != null)
                    {
                        bluetooth = bluetoothArgs.Message;
                        pairedBluetoothReceived.Set();
                    }
                };
            this.PairedBluetoothDeviceListReceived += pairedBluetoothReceivedHandler;

            await this.StartListeningForPairedBluetoothAsync(PairedBluetoothDevicesApi);

            pairedBluetoothReceived.WaitOne();

            await this.StopListeningForPairedBluetoothAsync();

            this.PairedBluetoothDeviceListReceived -= pairedBluetoothReceivedHandler;
            return bluetooth;
        }

        /// <summary>
        /// Gets the results for pairing a bluetooth device.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <returns>Results of pairing a bluetooth device</returns>
        public async Task<PairBluetoothDevicesInfo> GetPairBluetoothDevicesInfoAsync(string deviceId)
        {
            PairBluetoothDevicesInfo bluetooth = null;
            ManualResetEvent pairBluetoothReceived = new ManualResetEvent(false);
            WebSocketMessageReceivedEventHandler<PairBluetoothDevicesInfo> pairBluetoothReceivedHandler =
                delegate(DevicePortal sender, WebSocketMessageReceivedEventArgs<PairBluetoothDevicesInfo> bluetoothArgs)
                {
                    if (bluetoothArgs.Message != null)
                    {
                        bluetooth = bluetoothArgs.Message;
                        pairBluetoothReceived.Set();
                    }
                };
            this.PairBluetoothDeviceListReceived += pairBluetoothReceivedHandler;

            await this.StartListeningForPairBluetoothAsync(PairBluetoothDevicesApi, string.Format("deviceId={0}", Utilities.Hex64Encode(deviceId)));

            pairBluetoothReceived.WaitOne();

            await this.StopListeningForPairBluetoothAsync();

            this.PairBluetoothDeviceListReceived -= pairBluetoothReceivedHandler;
            return bluetooth;
        }

        /// <summary>
        /// Starts listening for bluetooth list of devices returned from the BluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="bluetoothApi">The relative portion of the uri path that specifies the API to call.</param>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForBluetoothAsync(string bluetoothApi)
        {
            if (this.bluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.bluetoothWebSocket = new WebSocket<AvailableBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.bluetoothWebSocket = new WebSocket<AvailableBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.bluetoothWebSocket.WebSocketMessageReceived += this.BluetoothReceivedHandler;
            }
            else
            {
                if (this.bluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.bluetoothWebSocket.ConnectAsync(bluetoothApi);
            await this.bluetoothWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for the list of bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for bluetooth devices and disconnecting from the websocket .</returns>
        public async Task StopListeningForBluetoothAsync()
        {
            await this.bluetoothWebSocket.CloseAsync();
        }

        /// <summary>
        /// Starts listening for paired bluetooth list of devices returned from the PairedBluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="bluetoothApi">The relative portion of the uri path that specifies the API to call.</param>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForPairedBluetoothAsync(string bluetoothApi)
        {
            if (this.pairedBluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.pairedBluetoothWebSocket = new WebSocket<PairedBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.pairedBluetoothWebSocket = new WebSocket<PairedBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.pairedBluetoothWebSocket.WebSocketMessageReceived += this.PairedBluetoothReceivedHandler;
            }
            else
            {
                if (this.pairedBluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.pairedBluetoothWebSocket.ConnectAsync(bluetoothApi);
            await this.pairedBluetoothWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for the list of paired bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for bluetooth devices and disconnecting from the websocket .</returns>
        public async Task StopListeningForPairedBluetoothAsync()
        {
            if (this.pairedBluetoothWebSocket == null || !this.pairedBluetoothWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.pairedBluetoothWebSocket.CloseAsync();
        }

        /// <summary>
        /// Starts listening for the result to pair the bluetooth device returned from the PairBluetoothDeviceListReceived handler.
        /// </summary>
        /// <param name="bluetoothApi">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForPairBluetoothAsync(string bluetoothApi, string payload)
        {
            if (this.pairBluetoothWebSocket == null)
            {
#if WINDOWS_UWP
                this.pairBluetoothWebSocket = new WebSocket<PairBluetoothDevicesInfo>(this.deviceConnection);
#else
                this.pairBluetoothWebSocket = new WebSocket<PairBluetoothDevicesInfo>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.pairBluetoothWebSocket.WebSocketMessageReceived += this.PairBluetoothReceivedHandler;
            }
            else
            {
                if (this.pairBluetoothWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.pairBluetoothWebSocket.ConnectAsync(bluetoothApi, payload);
            await this.pairBluetoothWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for the results for pairing bluetooth device.
        /// </summary>
        /// <returns>Task to stop listening for the bluetooth device and disconnecting from the websocket .</returns>
        public async Task StopListeningForPairBluetoothAsync()
        {
            await this.pairBluetoothWebSocket.CloseAsync();
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

        #region Data contract

        /// <summary>
        /// List of available bluetooth devices.
        /// </summary>
        [DataContract]
        public class AvailableBluetoothDevicesInfo 
        {
            /// <summary>
            /// Gets a list of available devices
            /// </summary>
            [DataMember(Name = "AvailableDevices")]
            public List<BluetoothDeviceInfo> AvailableDevices { get; private set; }
        }

        /// <summary>
        /// Information about a bluetooth device.
        /// </summary>
        public class BluetoothDeviceInfo 
        {
            /// <summary>
            /// Gets the bluetooth device ID
            /// </summary>
            [DataMember(Name = "ID")]
            public string ID { get; private set; }

            /// <summary>
            /// Gets the bluetooth device name
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
            /// Gets a list of paired devices
            /// </summary>
            [DataMember(Name = "PairedDevices")]
            public List<BluetoothDeviceInfo> PairedDevices { get; private set; }
        }

        /// <summary>
        /// Information about device to be paired.
        /// </summary>
        [DataContract]
        public class PairBluetoothDevicesInfo 
        {
            /// <summary>
            /// Gets the pair results
            /// </summary>
            [DataMember(Name = "PairResult")]
            public PairResult PairResult { get; private set; }
        }

        /// <summary>
        /// Information about device pairing.
        /// </summary>
        public class PairResult
        {
            /// <summary>
            /// Gets the result about the device pairing
            /// </summary>
            [DataMember(Name = "Result")]
            public string Result { get; private set; }

            /// <summary>
            /// Gets the deviceId to be paired
            /// </summary>
            [DataMember(Name = "deviceId")]
            public string DeviceId { get; private set; }

            /// <summary>
            /// Gets the pin
            /// </summary>
            [DataMember(Name = "Pin")]
            public string Pin { get; private set; }
        }
        #endregion // Data contract
    }
}
