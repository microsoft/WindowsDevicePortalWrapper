//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Main entry point for the test command line class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// String listing the available operations.
        /// </summary>
        private static readonly string AvailableOperationsText = "Supported operations are the following:\n" +
                                "   app\n" +
                                "   config\n" +
                                "   connect\n" +
                                "   fiddler\n" +
                                "   file\n" +
                                "   info\n" +
                                "   install\n" +
                                "   processes\n" +
                                "   reboot\n" +
                                "   sandbox\n" +
                                "   screenshot\n" +
                                "   systemPerf\n" +
                                "   xbluser";

        /// <summary>
        /// Usage string
        /// </summary>
        private static readonly string GeneralUsageMessage = "Usage: /x:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        /// <summary>
        /// The registry key that Xbox uses for storing default console information.
        /// </summary>
        private static readonly string DefaultConsoleRegkey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Durango\\WDP\\Consoles";

        /// <summary>
        /// The XTF registry key that Xbox uses for storing default console information.
        /// </summary>
        private static readonly string DefaultXtfConsoleRegkey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Durango\\Xtf\\Consoles";

        /// <summary>
        /// Operation types. These should be arranged alphabetically (other than None)
        /// for ease of use.
        /// </summary>
        private enum OperationType
        {
            /// <summary>
            /// No operation (just connects to the console).
            /// </summary>
            None,

            /// <summary>
            /// Perform an App operation (List, Suspend, Resume, Launch, Terminate,
            /// Uninstall)
            /// </summary>
            AppOperation,

            /// <summary>
            /// Get or set Xbox Settings.
            /// </summary>
            ConfigOperation,

            /// <summary>
            /// Sets the default xbox console to be this one.
            /// Uses the same registry setting as XbConnect tool.
            /// </summary>
            ConnectOperation,

            /// <summary>
            /// Manages enabling and disabling a Fiddler proxy for the console.
            /// </summary>
            FiddlerOperation,

            /// <summary>
            /// Does remote file operations.
            /// </summary>
            FileOperation,

            /// <summary>
            /// Info operation.
            /// </summary>
            InfoOperation,

            /// <summary>
            /// Install Appx Package or loose folder operation.
            /// </summary>
            InstallOperation,

            /// <summary>
            /// List processes operation.
            /// </summary>
            ListProcessesOperation,

            /// <summary>
            /// Reboot console operation.
            /// </summary>
            RebootOperation,

            /// <summary>
            /// Gets or sets the Xbox Live sandbox for the console.
            /// </summary>
            SandboxOperation,

            /// <summary>
            /// Takes a screenshot from the current Xbox One console.
            /// </summary>
            ScreenshotOperation,

            /// <summary>
            /// Get the system performance operation.
            /// </summary>
            SystemPerfOperation,

            /// <summary>
            /// User operation.
            /// </summary>
            XblUserOperation,
        }

        /// <summary>
        /// Gets the thumbprint that we use to manually accept server certificates even
        /// if they failed initial validation.
        /// </summary>
        public string AcceptedThumbprint { get; private set; }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            ParameterHelper parameters = new ParameterHelper();
            Program app = new Program();

            string targetConsole = string.Empty;

            try
            {
                parameters.ParseCommandLine(args);

                OperationType operation = OperationType.None;

                if (parameters.HasParameter(ParameterHelper.Operation))
                {
                    operation = OperationStringToEnum(parameters.GetParameterValue("op"));
                }

                // Allow /ip: to still function, even though we've moved to /x: in the documentation.
                if (parameters.HasParameter(ParameterHelper.IpOrHostnameOld) && !parameters.HasParameter(ParameterHelper.IpOrHostname))
                {
                    targetConsole = parameters.GetParameterValue(ParameterHelper.IpOrHostnameOld);
                }
                else if (parameters.HasParameter(ParameterHelper.IpOrHostname))
                {
                    targetConsole = parameters.GetParameterValue(ParameterHelper.IpOrHostname);
                }

                if (string.IsNullOrEmpty(targetConsole))
                {
                    object regValue;
                    regValue = Microsoft.Win32.Registry.GetValue(DefaultConsoleRegkey, null, null);

                    if (regValue == null)
                    {
                        regValue = Microsoft.Win32.Registry.GetValue(DefaultXtfConsoleRegkey, null, null);
                    }

                    if (regValue is string)
                    {
                        targetConsole = regValue as string;
                    }
                    else
                    {
                        throw new Exception("No default console is currently set. Must provide an ip address or hostname to connect to: /x:<ip or hostname>.");
                    }
                }

                string finalConnectionAddress = string.Format("https://{0}:11443", targetConsole);
                string userName = parameters.GetParameterValue(ParameterHelper.WdpUser);
                string password = parameters.GetParameterValue(ParameterHelper.WdpPassword);

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    try
                    {
                        // No creds were provided on the command line.
                        CredManager.RetrieveStoredCreds(targetConsole, ref userName, ref password);
                    }
                    catch (TypeLoadException)
                    {
                        // Windows 7 doesn't support credential storage so we'll get a TypeLoadException
                        throw new Exception("Credential storage is not supported on your PC. It requires Windows 8+ to run. Please provide the user and password parameters.");
                    }
                }
                else
                {
                    try
                    {
                        // Creds were provided on the command line.
                        CredManager.UpdateStoredCreds(targetConsole, userName, password);
                    }
                    catch (TypeLoadException)
                    {
                        // Do nothing. We can't store these on Win7
                    }
                }

                X509Certificate2 cert = null;

                IDevicePortalConnection connection = new DefaultDevicePortalConnection(finalConnectionAddress, userName, password);

                DevicePortal portal = new DevicePortal(connection);

                if (parameters.HasParameter(ParameterHelper.Cert))
                {
                    string certFile = parameters.GetParameterValue(ParameterHelper.Cert);

                    try
                    {
                        cert = new X509Certificate2(certFile);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Failed to read manual cert file {0}, {1}", certFile, e.Message), e);
                    }
                }

                // Add additional handling for untrusted certs.
                portal.UnvalidatedCert += app.DoCertValidation;

                // If a thumbprint is provided, use it for this connection. Otherwise check the registry.
                if (parameters.HasParameter("thumbprint"))
                {
                    app.AcceptedThumbprint = parameters.GetParameterValue("thumbprint");
                }
                else
                {
                    object regValue;
                    regValue = Microsoft.Win32.Registry.GetValue(DefaultConsoleRegkey, targetConsole, null);

                    if (regValue is string)
                    {
                        app.AcceptedThumbprint = regValue as string;
                    }
                }

                Task connectTask = portal.ConnectAsync(updateConnection: false, manualCertificate: cert);
                connectTask.Wait();

                if (portal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                {
                    if (portal.ConnectionHttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (connection.Credentials == null)
                        {
                            Console.WriteLine("The WDP connection was rejected due to missing credentials.\n\nPlease provide the /user:<username> and /pwd:<pwd> parameters on your first call to WDP.");
                        }
                        else
                        {
                            Console.WriteLine("The WDP connection was rejected due to bad credentials.\n\nPlease check the /user:<username> and /pwd:<pwd> parameters.");
                        }
                    }
                    else if (!string.IsNullOrEmpty(portal.ConnectionFailedDescription))
                    {
                        Console.WriteLine(string.Format("Failed to connect to WDP (HTTP {0}) : {1}", (int)portal.ConnectionHttpStatusCode, portal.ConnectionFailedDescription));
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to WDP for unknown reason.");
                    }
                }
                else
                {
                    // If the operation is more than a couple lines, it should
                    // live in its own file. These are arranged alphabetically
                    // for ease of use.
                    switch (operation)
                    {
                        case OperationType.AppOperation:
                            AppOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ConfigOperation:
                            ConfigOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ConnectOperation:
                            // User provided a new ip or hostname to set as the default.
                            if (parameters.HasParameter(ParameterHelper.IpOrHostname) || parameters.HasParameter(ParameterHelper.IpOrHostnameOld))
                            {
                                Microsoft.Win32.Registry.SetValue(DefaultConsoleRegkey, null, targetConsole);
                                Console.WriteLine("Default console set to {0}", targetConsole);
                            }
                            else
                            {
                                Console.WriteLine("Connected to Default console: {0}", targetConsole);
                            }

                            if (parameters.HasParameter("thumbprint"))
                            {
                                string thumbprint = parameters.GetParameterValue("thumbprint");
                                Microsoft.Win32.Registry.SetValue(DefaultConsoleRegkey, targetConsole, thumbprint);
                                Console.WriteLine("Thumbprint {0} saved for console with address {1}.", thumbprint, targetConsole);
                            }

                            break;

                        case OperationType.FiddlerOperation:
                            FiddlerOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.FileOperation:
                            FileOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.InfoOperation:
                            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
                            Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

                            Task<string> getNameTask = portal.GetDeviceNameAsync();
                            getNameTask.Wait();
                            Console.WriteLine("Device name: " + getNameTask.Result);
                            break;

                        case OperationType.InstallOperation:
                            if (!parameters.HasParameter(ParameterHelper.IpOrHostname))
                            {
                                // Ensure we have an IP since SMB might need it for path generation.
                                parameters.AddParameter(ParameterHelper.IpOrHostname, targetConsole);
                            }

                            InstallOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ListProcessesOperation:
                            ListProcessesOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.RebootOperation:
                            Task rebootTask = portal.RebootAsync();
                            rebootTask.Wait();
                            Console.WriteLine("Rebooting device.");
                            break;

                        case OperationType.SandboxOperation:
                            SandboxOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ScreenshotOperation:
                            ScreenshotOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.SystemPerfOperation:
                            SystemPerfOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.XblUserOperation:
                            UserOperation.HandleOperation(portal, parameters);
                            break;

                        default:
                            Console.WriteLine("Successfully connected to console but no operation was specified. \n" +
                                "Use the '/op:<operation type>' parameter to run a specified operation.");
                            Console.WriteLine();
                            Console.WriteLine(AvailableOperationsText);
                            break;
                    }
                }
            }
            catch (AggregateException e)
            {
                if (e.InnerException is DevicePortalException)
                {
                    DevicePortalException innerException = e.InnerException as DevicePortalException;

                    Console.WriteLine(string.Format("Exception encountered: {0}, hr = 0x{1:X} : {2}", innerException.StatusCode, innerException.HResult, innerException.Reason));
                }
                else
                {
                    Console.WriteLine(string.Format("Unexpected exception encountered: {0}", e.Message));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
            }

            // If a debugger is attached, don't close but instead loop here until
            // closed.
            while (Debugger.IsAttached)
            {
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Helper for converting from operation string to enum
        /// </summary>
        /// <param name="operation">string representation of the operation type.</param>
        /// <returns>enum representation of the operation type.</returns>
        private static OperationType OperationStringToEnum(string operation)
        {
            if (operation.Equals("app", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.AppOperation;
            }
            else if (operation.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ConfigOperation;
            }
            else if (operation.Equals("connect", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ConnectOperation;
            }
            else if (operation.Equals("fiddler", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.FiddlerOperation;
            }
            else if (operation.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.FileOperation;
            }
            else if (operation.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InfoOperation;
            }
            else if (operation.Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InstallOperation;
            }
            else if (operation.Equals("processes", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ListProcessesOperation;
            }
            else if (operation.Equals("reboot", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.RebootOperation;
            }
            else if (operation.Equals("sandbox", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.SandboxOperation;
            }
            else if (operation.Equals("screenshot", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ScreenshotOperation;
            }
            else if (operation.Equals("systemPerf", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.SystemPerfOperation;
            }
            else if (operation.Equals("xbluser", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.XblUserOperation;
            }

            throw new Exception("Unknown Operation Type. " + AvailableOperationsText);
        }

        /// <summary>
        /// Validate the server certificate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="certificate">The server's certificate</param>
        /// <param name="chain">The cert chain</param>
        /// <param name="sslPolicyErrors">Policy Errors</param>
        /// <returns>whether the cert passes validation</returns>
        private bool DoCertValidation(DevicePortal sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 cert = new X509Certificate2(certificate);

            if (!string.IsNullOrEmpty(this.AcceptedThumbprint))
            {
                if (cert.Thumbprint.Equals(this.AcceptedThumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            Console.WriteLine("The certificate provided by the device failed validation.");
            Console.WriteLine("Issuer:\t\t{0}", cert.Issuer);
            Console.WriteLine("Thumbprint:\t{0}", cert.Thumbprint);
            Console.WriteLine();

            Console.WriteLine("If you trust this endpoint, you can manually specify this certificate should be accepted by adding the following to any call:\n\t/thumbprint:{0}", cert.Thumbprint);
            Console.WriteLine("Or you can permanently add this as a trusted certificate for this device by calling the following:\n\tXboxWdpDriver.exe /op:connect /thumbprint:{0}", cert.Thumbprint);
            Console.WriteLine();

            return false;
        }
    }
}
