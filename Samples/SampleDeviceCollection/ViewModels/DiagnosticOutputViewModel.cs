//----------------------------------------------------------------------------------------------
// <copyright file="DiagnosticOutputViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using Prism.Mvvm;

namespace SampleDeviceCollection
{
    /// <summary>
    /// View Model to provide diagnostic output to a view through the OutputStream string property
    /// </summary>
    public class DiagnosticOutputViewModel : BindableBase, IDiagnosticSink
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        /// <summary>
        /// The maximum number of characters to store before discarding diagnostic output
        /// </summary>
        private const int MaxBufferSize = 65535;
        #endregion // Private Members

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region OutputStream
        /// <summary>
        /// Stores the output that has already been written to the diagnostic sink
        /// </summary>
        private string outputStream;

        /// <summary>
        /// Gets a string that stors the output that has already been written to this diagnostic sink
        /// </summary>
        public string OutputStream
        {
            get
            {
                return this.outputStream;
            }

            private set
            {
                this.SetProperty(ref this.outputStream, value);
            }
        }
        #endregion // OutputStream
        #endregion // Properties

        //-------------------------------------------------------------------
        //  IDiagnosticSink Implementation
        //-------------------------------------------------------------------
        #region IDiagnosticSink Implementation
        /// <summary>
        /// Prints a formatted diagnostic string to the output stream
        /// </summary>
        /// <param name="fmt">Format string</param>
        /// <param name="args">Format arguments</param>
        /// <remarks>Automatically flushes the output</remarks>
        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            this.outputStream += string.Format(fmt, args);
            this.FlushOutput();
        }
        #endregion // IDiagnosticSink Implementation

        //-------------------------------------------------------------------
        //  Private Methods
        //-------------------------------------------------------------------
        #region Private Methods
        /// <summary>
        /// Flush pending output
        /// </summary>
        private void FlushOutput()
        {
            int len = this.outputStream.Length;
            if (len > MaxBufferSize)
            {
                this.outputStream = this.outputStream.Substring(len - MaxBufferSize);
            }

            this.OnPropertyChanged("OutputStream");
        }
        #endregion // Private Methods
    }
}
