//----------------------------------------------------------------------------------------------
// <copyright file="ProcessManagement.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for IoT Process Management.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// IOT Run Command API.
        /// </summary>
        public static readonly string RunCommandApi = "api/iot/processmanagement/runcommand";

        /// <summary>
        /// IOT Run Command Without Output API.
        /// </summary>
        public static readonly string RunCommandWithoutOutputApi = "api/iot/processmanagement/runcommandwithoutput";

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="runAsDefaultAccount">Run As Default Account.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task RunCommandAsync(string command, string runAsDefaultAccount)
        {
            await this.PostAsync(
                RunCommandApi, string.Format("command={0}&runasdefaultaccount={1}", Utilities.Hex64Encode(command), Utilities.Hex64Encode(runAsDefaultAccount)));
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="commandWithoutOutput">Command Without Output.</param>
        /// <param name="runAsDefaultAccount">Run As Default Account.</param>
        /// <param name="timeout">The timeout value.</param>
        /// <returns>String containing the output after the command is executed.</returns>
        public async Task<RunCommandOutputInfo> RunCommandWithoutOutputAsync(string commandWithoutOutput, string runAsDefaultAccount, string timeout)
        {
            return await this.PostAsync<RunCommandOutputInfo>(
                RunCommandWithoutOutputApi, string.Format("command={0}&runasdefaultaccount={1}&timeout={2}", Utilities.Hex64Encode(commandWithoutOutput), Utilities.Hex64Encode(runAsDefaultAccount), Utilities.Hex64Encode(timeout)));
        }

        #region Data contract

        /// <summary>
        /// Run command output.
        /// </summary>
        [DataContract]
        public class RunCommandOutputInfo
        {
            /// <summary>
            /// Gets the output for the command executed.
            /// </summary>
            [DataMember(Name = "output")]
            public string Output { get; private set; }
        }
        #endregion // Data contract
    }
}
