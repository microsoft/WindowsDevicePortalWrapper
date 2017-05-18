//----------------------------------------------------------------------------------------------
// <copyright file="ScreenshotOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Screenshot related operations
    /// </summary>
    public class ScreenshotOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblScreenshotUsageMessage = "Usage:\n" +
            "  [/filepath:<filepath> [/override]]\n" +
            "        Saves a screenshot from the console to the destination file specified\n" +
            "        by /filepath. This filename should end in .png so the file can be\n" +
            "        correctly read. If this parameter is not provided the screenshot is\n" +
            "        saved in the current directory as xbox_screenshot.png. This operation\n" +
            "        will fail if the file already exists unless the /override flag is specified.\n";

        /// <summary>
        /// Main entry point for handling a Screenshot operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblScreenshotUsageMessage);
                return;
            }

            string filepath = parameters.GetParameterValue("filepath");

            if (string.IsNullOrEmpty(filepath))
            {
                filepath = "xbox_screenshot.png";
            }

            if (File.Exists(filepath) && !parameters.HasFlag("override"))
            {
                Console.WriteLine("Found an existing file: {0}. Specify /override flag to override this file.", filepath);
            }
            else
            {
                Task<Stream> screenshotTask = portal.TakeXboxScreenshotAsync();
                screenshotTask.Wait();

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    screenshotTask.Result.CopyTo(fileStream);
                }

                Console.WriteLine("Screenshot saved as {0}.", filepath);
            }
        }
    }
}