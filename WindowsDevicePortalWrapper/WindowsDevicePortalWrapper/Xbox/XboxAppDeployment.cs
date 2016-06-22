//----------------------------------------------------------------------------------------------
// <copyright file="XboxAppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System.Threading.Tasks;

    /// <content>
    /// Register Application Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// REST endpoint for registering a package from a loose folder
        /// </summary>
        private static readonly string RegisterPackageApi = "api/app/packagemanager/register";

        /// <summary>
        /// Registers a loose app on the console
        /// </summary>
        /// <param name="folderName">Relative folder path where the app can be found.</param>
        /// <returns>Task for tracking async completion.</returns>
        public async Task RegisterApplication(string folderName)
        {
            await this.Post(
                RegisterPackageApi,
                string.Format("folder={0}", Utilities.Hex64Encode(folderName)));
        }
    }
}
