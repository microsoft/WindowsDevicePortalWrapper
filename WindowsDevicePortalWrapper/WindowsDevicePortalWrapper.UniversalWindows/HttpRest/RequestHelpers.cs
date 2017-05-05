//----------------------------------------------------------------------------------------------
// <copyright file="RequestHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Methods for working with Http requests.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Copies a file to the specified stream and prepends the necessary content information
        /// required to be part of a multipart form data request.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        /// <param name="stream">The stream to which the file will be copied.</param>
        /// <returns>The async task.</returns>
        private static async Task CopyFileToRequestStream(
            StorageFile file,
            Stream stream)
        {
            byte[] data;
            string fn = file.Name;
            string contentDisposition = string.Format(
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", 
                fn, 
                fn);
            string contentType = "Content-Type: application/octet-stream\r\n\r\n";

            data = Encoding.ASCII.GetBytes(contentDisposition);
            stream.Write(data, 0, data.Length);

            data = Encoding.ASCII.GetBytes(contentType);
            stream.Write(data, 0, data.Length);

            using (IRandomAccessStreamWithContentType ras = await file.OpenReadAsync())
            {
                Stream fs = ras.AsStreamForRead();
                fs.CopyTo(stream);
            }
        }
    }
}
