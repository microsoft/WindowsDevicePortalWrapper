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

        public HttpContentHeaderCollection Headers { get; } = new HttpContentHeaderCollection();

        IAsyncOperationWithProgress<ulong, ulong> IHttpContent.BufferAllAsync()
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            items.Clear();
        }

        IAsyncOperationWithProgress<IBuffer, ulong> IHttpContent.ReadAsBufferAsync()
        {
            throw new NotImplementedException();
        }

        IAsyncOperationWithProgress<IInputStream, ulong> IHttpContent.ReadAsInputStreamAsync()
        {
            throw new NotImplementedException();
        }

        IAsyncOperationWithProgress<string, ulong> IHttpContent.ReadAsStringAsync()
        {
            throw new NotImplementedException();
        }

        bool IHttpContent.TryComputeLength(out ulong length)
        {
            length = 0;
            var boundaryLength = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString)).Length;
            foreach (var item in items)
            {
                var headerdata = GetFileHeader(new FileInfo(item));
                length += (ulong)(boundaryLength + headerdata.Length + new FileInfo(item).Length + 2);
            }
            length += (ulong)(boundaryLength + 2);
            return true;
        }

        IAsyncOperationWithProgress<ulong, ulong> IHttpContent.WriteToStreamAsync(IOutputStream outputStream)
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
            var boundary = Encoding.ASCII.GetBytes($"--{boundaryString}\r\n");
            var newline = Encoding.ASCII.GetBytes("\r\n");
            foreach (var item in items)
            {
                outStream.Write(boundary, 0, boundary.Length);
                bytesWritten += (ulong)boundary.Length;
                var headerdata = GetFileHeader(new FileInfo(item));
                outStream.Write(headerdata, 0, headerdata.Length);
                bytesWritten += (ulong)headerdata.Length;
                using (var file = File.OpenRead(item))
                {
                    await file.CopyToAsync(outStream);
                    bytesWritten += (ulong)file.Position;
                }
                outStream.Write(newline, 0, newline.Length);
                bytesWritten += (ulong)newline.Length;
                await outStream.FlushAsync();
                progress(bytesWritten);
            }
            // Close the installation request data.
            boundary = Encoding.ASCII.GetBytes($"--{boundaryString}--\r\n");
            outStream.Write(boundary, 0, boundary.Length);
            await outStream.FlushAsync();
            bytesWritten += (ulong)boundary.Length;
            return bytesWritten;
        }
        private static byte[] GetFileHeader(FileInfo info)
        {
            string contentType = "application/octet-stream";
            if (info.Extension.ToLower() == ".cer")
                contentType = "application/x-x509-ca-cert";

            return Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", info.Name, contentType));
        }
    }
}
