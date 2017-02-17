using System;
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
            Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.Format("multipart/form-data; boundary={0}", boundaryString));
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
            var data = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString));
            foreach (var item in items)
            {
                var headerdata = GetFileHeader(new FileInfo(item));
                outStream.Write(headerdata, 0, headerdata.Length);

                outStream.Write(data, 0, data.Length);
                using (var file = File.OpenRead(item))
                {
                    await file.CopyToAsync(outStream);
                }
                await outStream.FlushAsync();
            }
            // Close the installation request data.
            data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryString));
            outStream.Write(data, 0, data.Length);
            await outStream.FlushAsync();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            var boundaryLength = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString)).Length;
            foreach (var item in items)
            {
                length += (GetFileHeader(new FileInfo(item)).Length + new FileInfo(item).Length + boundaryLength);
            }
            length += (boundaryLength + 4);
            return true;
        }
        private static byte[] GetFileHeader(FileInfo info)
        {
            string contentType = "application/octet-stream";
            if (info.Extension.ToLower() == "cer")
                contentType = "application/x-x509-ca-cert";

            return Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", info.Name, contentType));
        }

    }
}