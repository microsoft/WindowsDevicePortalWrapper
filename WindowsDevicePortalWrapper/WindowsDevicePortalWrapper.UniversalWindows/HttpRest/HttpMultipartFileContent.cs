using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This class mimicks <see cref="HttpMultipartContent"/>, with two main differences
    /// 1. Simplifies posting files by taking file names instead of managing streams.
    /// 2. Does not quote the boundaries, due to a bug in the device portal:
    ///    https://insider.windows.com/FeedbackHub/fb?contextid=519&feedbackid=19a5af49-38f4-409a-b464-e66f80679545&form=1
    /// </summary>
    internal sealed class HttpMultipartFileContent : IHttpContent
    {
        private List<string> items = new List<string>();
        private string boundaryString;

        public HttpMultipartFileContent() : this(Guid.NewGuid().ToString()) { }

        public HttpMultipartFileContent(string boundary)
        {
            boundaryString = boundary;
            Headers.ContentType = new HttpMediaTypeHeaderValue(string.Format("multipart/form-data; boundary={0}", boundaryString));
        }

        public void Add(string filename)
        {
            if (filename != null)
                items.Add(filename);
        }

        public void AddRange(IEnumerable<string> filenames)
        {
            if (filenames != null)
                items.AddRange(filenames);
        }

        public HttpContentHeaderCollection Headers => new HttpContentHeaderCollection();

        public IAsyncOperationWithProgress<ulong, ulong> BufferAllAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            items.Clear();
        }

        public IAsyncOperationWithProgress<IBuffer, ulong> ReadAsBufferAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<IInputStream, ulong> ReadAsInputStreamAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<string, ulong> ReadAsStringAsync()
        {
            throw new NotImplementedException();
        }

        public bool TryComputeLength(out ulong length)
        {
            length = 0;
            var boundaryLength = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString)).Length;
            foreach (var item in items)
            {
                length += (ulong)(new FileInfo(item).Length + boundaryLength);
            }
            length += (ulong)(boundaryLength + 4);
            return true;
        }

        public IAsyncOperationWithProgress<ulong, ulong> WriteToStreamAsync(IOutputStream outputStream)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<ulong, ulong>((token, progress) =>
            {
                return WriteToStreamAsyncTask(outputStream, (ulong p) => progress.Report(p));
            });
        }

        private async Task<ulong> WriteToStreamAsyncTask(IOutputStream outputStream, Action<ulong> progress)
        {
            ulong bytesWritten = 0;
            var outStream = outputStream.AsStreamForWrite();
            var data = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString));
            foreach (var item in items)
            {
                outStream.Write(data, 0, data.Length);
                bytesWritten += (ulong)data.Length;
                using (var file = File.OpenRead(item))
                {
                    await file.CopyToAsync(outStream);
                    bytesWritten += (ulong)file.Position;
                    progress(bytesWritten);
                }
            }
            // Close the installation request data.
            data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryString));
            outStream.Write(data, 0, data.Length);
            bytesWritten += (ulong)data.Length;
            return bytesWritten;
        }
    }
}
