//----------------------------------------------------------------------------------------------
// <copyright file="ApplicationInstallStatus.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Filename matches the enum better than the ApplicationInstallStatusEventArgs class.")]

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Application install status event handler
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="args">install args</param>
    public delegate void ApplicationInstallStatusEventHandler(DevicePortal sender, ApplicationInstallStatusEventArgs args);

    /// <summary>
    /// Application install status
    /// </summary>
    public enum ApplicationInstallStatus
    {
        /// <summary>
        /// No install status
        /// </summary>
        None,

        /// <summary>
        /// Installation is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Installation is completed
        /// </summary>
        Completed,

        /// <summary>
        /// Installation failed
        /// </summary>
        Failed
    }

    /// <summary>
    /// Install phase
    /// </summary>
    public enum ApplicationInstallPhase
    {
        /// <summary>
        /// Idle phase
        /// </summary>
        Idle,

        /// <summary>
        /// Uninstalling the previous version
        /// </summary>
        UninstallingPreviousVersion,

        /// <summary>
        /// Copying the package file
        /// </summary>
        CopyingFile,

        /// <summary>
        /// Installing the package
        /// </summary>
        Installing
    }

    /// <summary>
    /// Application install status event args
    /// </summary>
    public class ApplicationInstallStatusEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInstallStatusEventArgs"/> class.
        /// </summary>
        /// <param name="status">Install status</param>
        /// <param name="phase">Install phase</param>
        /// <param name="message">Install message</param>
        internal ApplicationInstallStatusEventArgs(
            ApplicationInstallStatus status,
            ApplicationInstallPhase phase,
            string message = "")
        {
            this.Status = status;
            this.Phase = phase;
            this.Message = message;
        }

        /// <summary>
        /// Gets the install status
        /// </summary>
        public ApplicationInstallStatus Status { get; private set; }

        /// <summary>
        /// Gets the install phase
        /// </summary>
        public ApplicationInstallPhase Phase { get; private set; }

        /// <summary>
        /// Gets the install message
        /// </summary>
        public string Message { get; private set; }
    }
}
