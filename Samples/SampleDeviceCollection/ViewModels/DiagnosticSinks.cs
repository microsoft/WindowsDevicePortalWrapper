//----------------------------------------------------------------------------------------------
// <copyright file="DiagnosticSinks.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Several IDiagnosticSink implementations
    /// </summary>
    public static class DiagnosticSinks
    {
        /// <summary>
        /// Discards all diagnostic output
        /// </summary>
        public class NullDiagnosticSink : IDiagnosticSink
        {
            /// <summary>
            /// Writes formatted output to the sink
            /// </summary>
            /// <param name="fmt">Format string</param>
            /// <param name="args">Format args</param>
            public void OutputDiagnosticString(string fmt, params object[] args)
            {
            }
        }

        /// <summary>
        /// Sends diagnostic output to the console
        /// </summary>
        public class ConsoleDiagnositcSink : IDiagnosticSink
        {
            /// <summary>
            /// Writes formatted output to the sink
            /// </summary>
            /// <param name="fmt">Format string</param>
            /// <param name="args">Format args</param>
            public void OutputDiagnosticString(string fmt, params object[] args)
            {
                Console.Write(string.Format(fmt, args));
            }
        }

        /// <summary>
        /// Sends diagnostic output to the debug channel (i.e. OutputDebugString)
        /// </summary>
        public class DebugDiagnosticSink : IDiagnosticSink
        {
            /// <summary>
            /// Writes formatted output to the sink
            /// </summary>
            /// <param name="fmt">Format string</param>
            /// <param name="args">Format args</param>
            public void OutputDiagnosticString(string fmt, params object[] args)
            {
                Debug.Write(string.Format(fmt, args));
            }
        }

        /// <summary>
        /// Combines several diagnostic sinks together
        /// </summary>
        public class AggregateDiagnosticSink : IDiagnosticSink
        {
            /// <summary>
            /// The diagnostic sinks that are aggregated together
            /// </summary>
            private IEnumerable<IDiagnosticSink> sinks;

            /// <summary>
            /// Initializes a new instance of the <see cref="AggregateDiagnosticSink" /> class.
            /// </summary>
            /// <param name="args">Collection of diagnostic sinks to aggregate together</param>
            public AggregateDiagnosticSink(params IDiagnosticSink[] args)
            {
                this.sinks = args;
            }

            /// <summary>
            /// Writes formatted output to the sink
            /// </summary>
            /// <param name="fmt">Format string</param>
            /// <param name="args">Format args</param>
            public void OutputDiagnosticString(string fmt, params object[] args)
            {
                foreach (var diag in this.sinks)
                {
                    diag.OutputDiagnosticString(fmt, args);
                }
            }
        }
    }

    /// <summary>
    /// Interface describes a destination for diagnostic output
    /// </summary>
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Writes formatted output to the sink
        /// </summary>
        /// <param name="fmt">Format string</param>
        /// <param name="args">Format args</param>
        void OutputDiagnosticString(string fmt, params object[] args);
    }
}
