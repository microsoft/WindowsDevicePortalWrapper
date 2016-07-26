//----------------------------------------------------------------------------------------------
// <copyright file="InstallOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Install related operations
    /// </summary>
    public class InstallOperation : IDisposable
    {
        /// <summary>
        /// Error value for logon failure, returned if we need an SMB password to copy files to the device.
        /// </summary>
        private const int ErrorLogonFailureHresult = -2147023570;

        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblInstallUsageMessage = "Usage:\n" +
            "  /appx:<path to Appx> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate>]\n" +
            "        Installs the given AppX package, along with any given dependencies.\n" +
            "  /folder:<path to loose folder> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate>]\n" +
            "        Installs the appx from a loose folder, along with any given dependencies.\n";

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
            string folderPath = parameters.GetParameterValue("folder");

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
            else if (!string.IsNullOrEmpty(folderPath))
            {
                try
                {
                    // Install all dependencies one at a time (loose folder doesn't handle dependencies well).
                    foreach (string dependency in dependencies)
                    {
                        operation.mreAppInstall.Reset();
                        Task installTask = portal.InstallApplication(null, dependency, new List<string>());
                        operation.mreAppInstall.WaitOne();
                    }

                    if (!Directory.Exists(folderPath))
                    {
                        Console.WriteLine("Failed to find provided loose folder.");
                        Console.WriteLine();
                        Console.WriteLine(XblInstallUsageMessage);
                        return;
                    }

                    // Remove any trailing slash
                    if (folderPath.EndsWith("\\"))
                    {
                        folderPath = folderPath.Remove(folderPath.Length - 1);
                    }

                    // Get just the folder name
                    string folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
                    string shareName = Path.Combine("\\\\", parameters.GetParameterValue(ParameterHelper.IpOrHostname), "DevelopmentFiles");

                    try
                    {
                        operation.CopyDirectory(folderPath, Path.Combine(shareName, "LooseApps", folderName));
                    }
                    catch (IOException e)
                    {
                        if (e.HResult == ErrorLogonFailureHresult)
                        {
                            Task<SmbInfo> smbTask = portal.GetSmbShareInfo();
                            smbTask.Wait();

                            // Set the username/password for accessing the share.
                            NetworkShare.DisconnectFromShare(shareName, true);
                            int connected = NetworkShare.ConnectToShare(shareName, smbTask.Result.Username, smbTask.Result.Password);

                            if (connected != 0)
                            {
                                Console.WriteLine(string.Format("Failed to connect to the network share: {0}", connected));
                                return;
                            }

                            operation.CopyDirectory(folderPath, Path.Combine(shareName, "LooseApps", folderName));

                            NetworkShare.DisconnectFromShare(shareName, false);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Unexpected exception encountered: {0}", e.Message));
                            return;
                        }
                    }

                    Task registerTask = portal.RegisterApplication(folderName);
                    registerTask.Wait();

                    Console.WriteLine("Install complete.");
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is DevicePortalException)
                    {
                        DevicePortalException innerException = e.InnerException as DevicePortalException;

                        Console.WriteLine(string.Format("Exception encountered: 0x{0:X} : {1}", innerException.HResult, innerException.Reason));
                    }
                    else if (e.InnerException is OperationCanceledException)
                    {
                        Console.WriteLine("The operation was cancelled.");
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Unexpected exception encountered: {0}", e.Message));
                    }

                    return;
                }
            }
            else
            {
                Console.WriteLine("Must provide an appx package or a loose folder.");
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
        /// Recursively copies a source directory to a destination directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="destinationDirectory">The destination directory.</param>
        private void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDirectory))
            {
                // Get just the folder name
                string subDirName = subDir.Substring(subDir.LastIndexOf('\\') + 1);
                string destSubDir = Path.Combine(destinationDirectory, subDirName);
                if (!Directory.Exists(destSubDir))
                {
                    Directory.CreateDirectory(destSubDir);
                }

                this.CopyDirectory(subDir, destSubDir);
            }

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                // Get just the file name
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(destinationDirectory, fileName), true);
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