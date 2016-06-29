//----------------------------------------------------------------------------------------------
// <copyright file="InstallOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace TestApp
{
    /// <summary>
    /// Helper for Install related operations
    /// </summary>
    internal class InstallOperation : IDisposable
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblInstallUsageMessage = "Usage:\n" +
            "  /appx:<path to Appx> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate>]\n" +
            "        Installs the given AppX package, along with any given dependencies.\n";

        /// <summary>
        /// Event used to indicate that the application install process is complete.
        /// </summary>
        private ManualResetEvent mreAppInstall = new ManualResetEvent(false);

        /// <summary>
        /// Install results for getting status, phase, and message.
        /// </summary>
        private ApplicationInstallStatusEventArgs installResults;

        /// <summary>
        /// Whether verbose logging should be used.
        /// </summary>
        private bool verbose;

        /// <summary>
        /// Main entry point for handling an install operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblInstallUsageMessage);
                return;
            }

            InstallOperation operation = new InstallOperation();
            portal.AppInstallStatus += operation.AppInstallStatusHandler;

            if (parameters.HasFlag(ParameterHelper.VerboseFlag))
            {
                operation.verbose = true;
            }

            List<string> dependencies = new List<string>();

            // Build up the list of dependencies.
            if (parameters.HasParameter("depend"))
            {
                dependencies.AddRange(parameters.GetParameterValue("depend").Split(';'));
            }

            string certificate = parameters.GetParameterValue("cer");

            string appxFile = parameters.GetParameterValue("appx");

            if (!string.IsNullOrEmpty(appxFile))
            {
                operation.mreAppInstall.Reset();
                Task installTask = portal.InstallApplication(null, appxFile, dependencies, certificate);
                operation.mreAppInstall.WaitOne();

                if (operation.installResults.Status == ApplicationInstallStatus.Completed)
                {
                    Console.WriteLine("Install complete.");
                }
                else
                {
                    Console.WriteLine("Install failed in phase {0}. {1}", operation.installResults.Phase, operation.installResults.Message);
                }
            }
            else
            {
                Console.WriteLine("Must provide an appx package.");
                Console.WriteLine();
                Console.WriteLine(XblInstallUsageMessage);
                return;
            }
        }

        /// <summary>
        /// Cleans up the object's data.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up the object's data.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.mreAppInstall?.Dispose();
                this.mreAppInstall = null;
            }
        }

        /// <summary>
        /// Handler for the ApplicationInstallStatus event.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void AppInstallStatusHandler(
            object sender,
            ApplicationInstallStatusEventArgs args)
        {
            if (this.verbose)
            {
                Console.WriteLine(args.Message);
            }

            if (args.Status != ApplicationInstallStatus.InProgress)
            {
                this.installResults = args;
                this.mreAppInstall.Set();
            }
        }
    }
}