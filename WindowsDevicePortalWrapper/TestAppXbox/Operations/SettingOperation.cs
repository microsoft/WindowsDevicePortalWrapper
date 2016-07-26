﻿//----------------------------------------------------------------------------------------------
// <copyright file="SettingOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Setting related operations
    /// </summary>
    public class SettingOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblSettingUsageMessage = "Usage:\n" +
            "  [/setting:<setting name> [/value:<setting value>]]\n" +
            "        Gets current settings and their values. If\n" +
            "        /setting is specified, only returns that value.\n" +
            "        If /value is also specified, sets the settting to\n" +
            "        that value instead of returning the current\n" +
            "        value.\n";

        /// <summary>
        /// Main entry point for handling a Setting operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblSettingUsageMessage);
                return;
            }

            string desiredSetting = parameters.GetParameterValue("setting");
            string desiredValue = parameters.GetParameterValue("value");

            // Determine if this is for all settings or a single setting.
            if (string.IsNullOrWhiteSpace(desiredSetting))
            {
                Task<XboxSettingList> getSettingsTask = portal.GetXboxSettings();
                getSettingsTask.Wait();

                Console.WriteLine(getSettingsTask.Result);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(desiredValue))
                {
                    Task<XboxSetting> getSettingTask = portal.GetXboxSetting(desiredSetting);
                    getSettingTask.Wait();

                    Console.WriteLine(getSettingTask.Result);
                }
                else
                {
                    XboxSetting setting = new XboxSetting();
                    setting.Name = desiredSetting;
                    setting.Value = desiredValue;

                    Task<XboxSetting> setSettingTask = portal.UpdateXboxSetting(setting);
                    setSettingTask.Wait();

                    Console.WriteLine(setSettingTask.Result);
                }
            }
        }
    }
}