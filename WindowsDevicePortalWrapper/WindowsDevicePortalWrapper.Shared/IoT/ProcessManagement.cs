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

    public partial class DevicePortal
    {
        public static readonly string RunCommandApi = "api/iot/processmanagement/runcommand";
        public static readonly string RunCommandWithoutOutputApi = "api/iot/processmanagement/runcommandwithoutput";

        /// <summary>
        /// Runs the command.
        /// </summary>
        public async Task RunCommand(string command,string runAsDefaultAccount)
        {
            await this.Post(
                RunCommandApi, string.Format("command={0}&runasdefaultaccount={1}", Utilities.Hex64Encode(command), Utilities.Hex64Encode(runAsDefaultAccount)));
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <returns>String containing the output after the command is executed.</returns>
        public async Task<RunCommandOutputInfo> RunCommandWithoutOutput(string commandWithoutOutput, string runAsDefaultAccount, string timeout)
        {
            return await this.Post<RunCommandOutputInfo>(
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
            /// Returns the output for the command executed.
            /// </summary>
            [DataMember(Name = "output")]
            public string output { get; set; }
        }
        #endregion // Data contract

    }
}
