//----------------------------------------------------------------------------------------------
// <copyright file="ParameterHelper.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MockDataGenerator
{
    /// <summary>
    /// Class for parsing command line parameters
    /// </summary>
    public class ParameterHelper
    {
        /// <summary>
        /// Help Flag identifier string
        /// </summary>
        public static readonly string HelpFlag = "?";

        /// <summary>
        /// Device Identifier identifier string
        /// </summary>
        public static readonly string FullAddress = "address";

        /// <summary>
        /// WDP Username identifier string
        /// </summary>
        public static readonly string WdpUser = "user";

        /// <summary>
        /// WDP Password identifier string
        /// </summary>
        public static readonly string WdpPassword = "pwd";

        /// <summary>
        /// List for storing parsed command line parameters as key value pairs.
        /// </summary>
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        /// <summary>
        /// List for storing parsed command line flags.
        /// </summary>
        private List<string> flags = new List<string>();

        /// <summary>
        /// Helper for getting a parameter value for a key
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <returns>parameter value or null if not present</returns>
        public string GetParameterValue(string key)
        {
            if (this.parameters.ContainsKey(key))
            {
                return this.parameters[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Helper for determining if we have a value for a parameter
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <returns>boolean indicating if the parameter is present</returns>
        public bool HasParameter(string key)
        {
            return this.parameters.ContainsKey(key);
        }

        /// <summary>
        /// Helper for determining if a flag is set
        /// </summary>
        /// <param name="flag">flag name</param>
        /// <returns>boolean indicating if the flag is set</returns>
        public bool HasFlag(string flag)
        {
            return this.flags.Contains(flag);
        }

        /// <summary>
        /// Main entry for parsing a command line array
        /// </summary>
        /// <param name="args">command line args</param>
        public void ParseCommandLine(string[] args)
        {
            // Parse the command line args
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", arg));
                }

                arg = arg.Substring(1);

                int valueIndex = arg.IndexOf(':');
                string value = null;

                // If this contains a colon, seperate it into the param and value. Otherwise add it as a flag
                if (valueIndex > 0)
                {
                    value = arg.Substring(valueIndex + 1);
                    arg = arg.Substring(0, valueIndex);

                    this.parameters.Add(arg.ToLowerInvariant(), value);
                }
                else
                {
                    this.flags.Add(arg.ToLowerInvariant());
                }
            }
        }
    }
}
