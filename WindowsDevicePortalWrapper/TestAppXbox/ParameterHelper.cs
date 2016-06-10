using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class ParameterHelper
    {
        // lists for storing all parsed command line parameters
        private Dictionary<String, String> _parameters = new Dictionary<string, string>();
        private List<String> _flags = new List<string>();

        // Some common parameters can have defines
        public static String HelpFlag = "?";
        public static String VerboseFlag = "v";
        public static String Operation = "op";
        public static String IpOrHostname = "ip";
        public static String WdpUser = "user";
        public static String WdpPassword = "pwd";

        public string GetParameterValue(string key)
        {
            if (_parameters.ContainsKey(key))
            {
                return _parameters[key];
            }
            else
            {
                return null;
            }
        }

        public bool HasParameter(string key)
        {
            return _parameters.ContainsKey(key);
        }

        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        public void ParseCommandLine(String[] args)
        {
            // Parse the command line args
            for (Int32 i = 0; i < args.Length; ++i)
            {
                String arg = args[i];
                if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                {
                    throw new Exception(String.Format("Unrecognized argument: {0}", arg));
                }

                arg = arg.Substring(1);

                Int32 valueIndex = arg.IndexOf(':');
                String value = null;

                // If this contains a colon, seperate it into the param and value. Otherwise add it as a flag
                if (valueIndex > 0)
                {
                    value = arg.Substring(valueIndex + 1);
                    arg = arg.Substring(0, valueIndex);

                    _parameters.Add(arg.ToLowerInvariant(), value);
                }
                else
                {
                    _flags.Add(arg.ToLowerInvariant());
                }
            }
        }
    }
}
