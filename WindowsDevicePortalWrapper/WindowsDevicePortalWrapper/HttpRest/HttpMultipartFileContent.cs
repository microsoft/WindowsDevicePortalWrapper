using System;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This class mimicks <see cref="HttpMultipartContent"/>, with two main differences
    /// 1. Simplifies posting files by taking file names instead of managing streams.
    /// 2. Does not quote the boundaries, due to a bug in the device portal:
    ///    https://insider.windows.com/FeedbackHub/fb?contextid=519&feedbackid=19a5af49-38f4-409a-b464-e66f80679545&form=1
    /// </summary>
    internal sealed class HttpMultipartFileContent : HttpContent
    {
        private List<string> items = new List<string>();
        private string boundaryString;

        public HttpMultipartFileContent() : this(Guid.NewGuid().ToString()) { }

        public HttpMultipartFileContent(string boundary)
        {
            boundaryString = boundary;
            Headers.TryAddWithoutValidation("Content-Type", string.Format("multipart/form-data; boundary={0}", boundaryString));
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

        protected override async Task SerializeToStreamAsync(Stream outStream, TransportContext context)
        {
            var boundary = Encoding.ASCII.GetBytes($"--{boundaryString}\r\n");
            var newline = Encoding.ASCII.GetBytes("\r\n");
            foreach (var item in items)
            {
                outStream.Write(boundary, 0, boundary.Length);
                var headerdata = GetFileHeader(new FileInfo(item));
                outStream.Write(headerdata, 0, headerdata.Length);

                using (var file = File.OpenRead(item))
                {
                    await file.CopyToAsync(outStream);
                }
                outStream.Write(newline, 0, newline.Length);
                await outStream.FlushAsync();
            }
            // Close the installation request data.
            boundary = Encoding.ASCII.GetBytes($"--{boundaryString}--\r\n");
            outStream.Write(boundary, 0, boundary.Length);
            await outStream.FlushAsync();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            var boundaryLength = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString)).Length;
            foreach (var item in items)
            {
                var headerdata = GetFileHeader(new FileInfo(item));
                length += boundaryLength + headerdata.Length + new FileInfo(item).Length + 2;
            }
            length += (boundaryLength + 2);
            return true;
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