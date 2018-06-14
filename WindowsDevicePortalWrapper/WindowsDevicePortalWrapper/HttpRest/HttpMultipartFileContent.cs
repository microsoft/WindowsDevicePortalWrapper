//----------------------------------------------------------------------------------------------
// <copyright file="HttpMultipartFileContent.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

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
    /// 2. Does not quote the boundaries, due to a bug in the device portal
    /// </summary>
    internal sealed class HttpMultipartFileContent : HttpContent
    {
        /// <summary>
        /// List of items to transfer
        /// </summary>
        private List<string> items = new List<string>();

        /// <summary>
        /// Boundary string
        /// </summary>
        private string boundaryString;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipartFileContent" /> class.
        /// </summary>
        public HttpMultipartFileContent() : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipartFileContent" /> class.
        /// </summary>
        /// <param name="boundary">The boundary string for file content.</param>
        public HttpMultipartFileContent(string boundary)
        {
            this.boundaryString = boundary;
            Headers.TryAddWithoutValidation("Content-Type", string.Format("multipart/form-data; boundary={0}", this.boundaryString));
        }

        /// <summary>
        /// Adds a file to the list of items to transfer
        /// </summary>
        /// <param name="filename">The name of the file to add</param>
        public void Add(string filename)
        {
            if (filename != null)
            {
                this.items.Add(filename);
            }
        }

        /// <summary>
        /// Adds a range of files to the list of items to transfer
        /// </summary>
        /// <param name="filenames">List of files to add</param>
        public void AddRange(IEnumerable<string> filenames)
        {
            if (filenames != null)
            {
                this.items.AddRange(filenames);
            }
        }

        /// <summary>
        /// Serializes the stream.
        /// </summary>
        /// <param name="outStream">Serialized Stream</param>
        /// <param name="context">The Transport Context</param>
        /// <returns>Task tracking progress</returns>
        protected override async Task SerializeToStreamAsync(Stream outStream, TransportContext context)
        {
            var boundary = Encoding.ASCII.GetBytes($"--{boundaryString}\r\n");
            var newline = Encoding.ASCII.GetBytes("\r\n");
            foreach (var item in this.items)
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

        /// <summary>
        /// Computes required length for the transfer.
        /// </summary>
        /// <param name="length">The computed length value</param>
        /// <returns>Whether or not the length was successfully computed</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            var boundaryLength = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", this.boundaryString)).Length;
            foreach (var item in this.items)
            {
                var headerdata = GetFileHeader(new FileInfo(item));
                length += boundaryLength + headerdata.Length + new FileInfo(item).Length + 2;
            }

            length += boundaryLength + 2;
            return true;
        }

        /// <summary>
        /// Gets the file header for the transfer
        /// </summary>
        /// <param name="info">Information about the file</param>
        /// <returns>A byte array with the file header information</returns>
        private static byte[] GetFileHeader(FileInfo info)
        {
            string contentType = "application/octet-stream";
            if (info.Extension.ToLower() == ".cer")
            {
                contentType = "application/x-x509-ca-cert";
            }

            return Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", info.Name, contentType));
        }
    }
}