using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLab
{
    /// <summary>
    /// Interface describes a destination for diagnostic output
    /// </summary>
    public interface IDiagnosticSink
    {
        void OutputDiagnosticString(string fmt, params object[] args);
    }

    /// <summary>
    /// Discards all diagnostic output
    /// </summary>
    public class NullDiagnosticSink : IDiagnosticSink
    {
        public void OutputDiagnosticString(string fmt, params object[] args)
        {
        }
    }

    /// <summary>
    /// Sends diagnostic output to the console
    /// </summary>
    public class ConsoleDiagnositcSink : IDiagnosticSink
    {
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
        private IEnumerable<IDiagnosticSink> sinks;

        public AggregateDiagnosticSink(params IDiagnosticSink[] args)
        {
            sinks = args;
        }


        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            foreach (var diag in sinks)
            {
                diag.OutputDiagnosticString(fmt, args);
            }
        }
    }


    /// <summary>
    /// View Model to provide diagnostic output to a view through the OutputStream string property
    /// </summary>
    public class DiagnosticOutputViewModel : INotifyPropertyChanged, IDiagnosticSink
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private const int cMaxBufferSize = 65535;
        #endregion // Private Members


        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region OutputStream
        private string mOutputStream;
        public string OutputStream
        {
            get
            {
                return mOutputStream;
            }

            private set
            {
                SetField(ref mOutputStream, value, "OutputStream");
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
            mOutputStream += string.Format(fmt, args);
            FlushOutput();
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
            int len = mOutputStream.Length;
            if (len > cMaxBufferSize)
            {
                mOutputStream = mOutputStream.Substring(len - cMaxBufferSize);
            }
            OnPropertyChanged("OutputStream");
        }
        #endregion // Private Methods

        //-------------------------------------------------------------------
        //  INotifyPropertyChanged Implementation
        //-------------------------------------------------------------------
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion // INotifyPropertyChanged implementation
    }
}
