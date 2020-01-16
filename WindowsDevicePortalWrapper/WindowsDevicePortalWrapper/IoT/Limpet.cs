//----------------------------------------------------------------------------------------------
// <copyright file="Limpet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// TPM Limpet APIs for IoT.
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// TPM settings API.
        /// </summary>
        public static readonly string TpmSettingsApi = "api/iot/tpm/settings";

        /// <summary>
        /// Advanced Configuration and Power Interface(ACPI) information API.
        /// </summary>
        public static readonly string TpmAcpiTablesApi = "api/iot/tpm/acpitables";

        /// <summary>
        /// TPM Azure Token API.
        /// </summary>
        public static readonly string TpmAzureTokenApi = "api/iot/tpm/azuretoken";

        /// <summary>
        /// Gets Tpm Settings information.
        /// </summary>
        /// <returns>String containing theTpm Settings information.</returns>
        public async Task<TpmSettingsInfo> GetTpmSettingsInfoAsync()
        {
            return await this.GetAsync<TpmSettingsInfo>(TpmSettingsApi);
        }

        /// <summary>
        /// Sets TPM ACPI Tables information.
        /// </summary>
        /// <param name="acpiTableIndex">ACPI Table Index.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetTpmAcpiTablesInfoAsync(string acpiTableIndex)
        {
            await this.PostAsync(TpmAcpiTablesApi, string.Format("AcpiTableIndex={0}", Utilities.Hex64Encode(acpiTableIndex)));
        }

        /// <summary>
        /// Gets TPM ACPI Tables information.
        /// </summary>
        /// <returns>List of string containing the TPM ACPI Tables information.</returns>
        public async Task<TpmAcpiTablesInfo> GetTpmAcpiTablesInfoAsync()
        {
            return await this.GetAsync<TpmAcpiTablesInfo>(TpmAcpiTablesApi);
        }

        /// <summary>
        /// Gets TPM Logical Device Settings information.
        /// </summary>
        /// <param name="logicalDeviceId">The device id</param>
        /// <returns>String containing the TPM Logical Device Settings information.</returns>
        public async Task<TpmLogicalDeviceSettingsInfo> GetTpmLogicalDeviceSettingsInfoAsync(int logicalDeviceId)
        {
            return await this.GetAsync<TpmLogicalDeviceSettingsInfo>(string.Format("{0}/{1}", TpmSettingsApi, logicalDeviceId));
        }

        /// <summary>
        /// Sets TPM Logical Device Settings information.
        /// </summary>
        /// <param name="logicalDeviceId">Logical Device Id.</param>
        /// <param name="azureUri">Azure Uri.</param>
        /// <param name="azureKey">Azure Key.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetTpmLogicalDeviceSettingsInfoAsync(int logicalDeviceId, string azureUri, string azureKey)
        {
            await this.PostAsync(string.Format("{0}/{1}", TpmSettingsApi, logicalDeviceId), string.Format("AzureUri={0}&AzureKey={1}", Utilities.Hex64Encode(azureUri), Utilities.Hex64Encode(azureKey)));
        }

        /// <summary>
        /// Resets TPM Logical Device Settings information.
        /// </summary>
        /// <param name="logicalDeviceId">Logical Device Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task ResetTpmLogicalDeviceSettingsInfoAsync(int logicalDeviceId)
        {
            await this.DeleteAsync(string.Format("{0}/{1}", TpmSettingsApi, logicalDeviceId));
        }
        
        /// <summary>
        /// Gets TPM Azure Token information.
        /// </summary>
        /// <param name="logicalDeviceId">The device id</param>
        /// <param name="validity">Validity of the token</param>
        /// <returns>String containing the TPM Azure Token information.</returns>
        public async Task<TpmAzureTokenInfo> GetTpmAzureTokenInfoAsync(int logicalDeviceId, string validity)
        {
            return await this.GetAsync<TpmAzureTokenInfo>(string.Format("{0}/{1}", TpmAzureTokenApi, logicalDeviceId), string.Format("validity={0}", Utilities.Hex64Encode(validity)));
        }

        #region Data contract

        /// <summary>
        /// TPM Status information.
        /// </summary>
        [DataContract]
        public class TpmSettingsInfo
        {
            /// <summary>
            /// Gets TPM Status. 
            /// </summary>
            [DataMember(Name = "TPMStatus")]
            public string TPMStatus { get; private set; }

            /// <summary>
            /// Gets TPM Family. 
            /// </summary>
            [DataMember(Name = "TPMFamily")]
            public string TPMFamily { get; private set; }

            /// <summary>
            /// Gets TPM Firmware. 
            /// </summary>
            [DataMember(Name = "TPMFirmware")]
            public string TPMFirmware { get; private set; }

            /// <summary>
            /// Gets TPM Revision. 
            /// </summary>
            [DataMember(Name = "TPMRevision")]
            public string TPMRevision { get; private set; }

            /// <summary>
            /// Gets TPM Status. 
            /// </summary>
            [DataMember(Name = "TPMType")]
            public string TPMTypes { get; private set; }

            /// <summary>
            /// Gets TPM Vendor. 
            /// </summary>
            [DataMember(Name = "TPMVendor")]
            public string TPMVendor { get; private set; }

            /// <summary>
            /// Gets TPM Manufacturer. 
            /// </summary>
            [DataMember(Name = "TPMManufacturer")]
            public string TPMManufacturer { get; private set; }
        }

        /// <summary>
        /// TPM ACPI Tables information.
        /// </summary>
        [DataContract]
        public class TpmAcpiTablesInfo
        {
            /// <summary>
            /// Gets TPM ACPI Tables. 
            /// </summary>
            [DataMember(Name = "AcpiTables")]
            public List<string> AcpiTables { get; private set; }        
        }

        /// <summary>
        /// TPM Logical Device Settings information.
        /// </summary>
        [DataContract]
        public class TpmLogicalDeviceSettingsInfo
        {
            /// <summary>
            /// Gets Azure Uri. 
            /// </summary>
            [DataMember(Name = "AzureUri")]
            public string AzureUri { get; private set; }

            /// <summary>
            /// Gets Device Id. 
            /// </summary>
            [DataMember(Name = "DeviceId")]
            public string DeviceId { get; private set; }
        }

        /// <summary>
        /// TPM Azure Token information.
        /// </summary>
        [DataContract]
        public class TpmAzureTokenInfo
        {
            /// <summary>
            /// Gets Azure Token. 
            /// </summary>
            [DataMember(Name = "AzureToken")]
            public string AzureToken { get; private set; }            
        }
        #endregion // Data contract
    }
}
