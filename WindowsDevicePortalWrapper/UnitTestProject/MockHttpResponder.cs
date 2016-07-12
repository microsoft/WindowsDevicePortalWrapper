//----------------------------------------------------------------------------------------------
// <copyright file="MockHttpResponder.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Mock implementation of HttpWrapper.
    /// </summary>
    public class MockHttpResponder
    {
        /// <summary>
        /// Dictionary of mock responses from endpoints to the stored response message
        /// </summary>
        private Dictionary<string, HttpResponseMessage> mockResponses = new Dictionary<string, HttpResponseMessage>();

        /// <summary>
        /// Clears all mock responses. Used between tests to ensure mocks don't interfere with other tests.
        /// </summary>
        public void ResetMockResponses()
        {
            this.mockResponses.Clear();
        }

        /// <summary>
        /// Adds a given response as a mock for the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to be mocked.</param>
        /// <param name="response">The response to return.</param>
        public void AddMockResponse(string endpoint, HttpResponseMessage response)
        {
            this.mockResponses.Add(endpoint, response);
        }

        /// <summary>
        /// Adds a response for this endpoint, loading from the file matching the deviceType and OsVersion
        /// </summary>
        /// <param name="endpoint">Endpoint we are mocking.</param>
        /// <param name="platform">Device platform we are testing.</param>
        /// <param name="operatingSystemVersion">The OS Version we are testing.</param>
        public void AddMockResponse(string endpoint, DevicePortalPlatforms platform, string operatingSystemVersion)
        {
            // If no OS is provided, use the default.
            if (operatingSystemVersion == null)
            {
                this.AddMockResponse(endpoint);
                return;
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string filepath = Path.Combine("MockData", platform.ToString(), operatingSystemVersion, endpoint.Replace('/', '_') + "_" + platform.ToString() + "_" + operatingSystemVersion + ".dat");
            response.Content = this.LoadContentFromFile(filepath);

            this.mockResponses.Add(endpoint, response);
        }

        /// <summary>
        /// Adds a default response for this endpoint, device agnostic.
        /// </summary>
        /// <param name="endpoint">Endpoint we are mocking.</param>
        public void AddMockResponse(string endpoint)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            // Add the Content from the default file if one exists.
            string filepath = Path.Combine("MockData", "Defaults", endpoint.Replace('/', '-') + "_Default.dat");

            response.Content = this.LoadContentFromFile(filepath);

            this.mockResponses.Add(endpoint, response);
        }

        /// <summary>
        /// Abstract method Mock Implementation
        /// </summary>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            Task<HttpResponseMessage> task = new Task<HttpResponseMessage>(this.HttpStoredResponse, uri);
            task.Start();

            return await task;
        }

        /// <summary>
        /// Abstract method Mock Implementation
        /// </summary>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content)
        {
            Task<HttpResponseMessage> task = new Task<HttpResponseMessage>(this.HttpStoredResponse, uri);
            task.Start();

            return await task;
        }

        /// <summary>
        /// Abstract method Mock Implementation
        /// </summary>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content)
        {
            Task<HttpResponseMessage> task = new Task<HttpResponseMessage>(this.HttpStoredResponse, uri);
            task.Start();

            return await task;
        }

        /// <summary>
        /// Abstract method Mock Implementation
        /// </summary>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(Uri uri)
        {
            Task<HttpResponseMessage> task = new Task<HttpResponseMessage>(this.HttpStoredResponse, uri);
            task.Start();

            return await task;
        }

        /// <summary>
        /// Http Stored Response.
        /// </summary>
        /// <param name="state">The URI we are looking for a canned response for.</param>
        /// <returns>An HttpResponseMessage previously stored for this URI.</returns>
        private HttpResponseMessage HttpStoredResponse(object state)
        {
            Uri uri = state as Uri;

            Assert.IsNotNull(uri);

            foreach (KeyValuePair<string, HttpResponseMessage> storedResponse in this.mockResponses)
            {
                if (uri.AbsolutePath.Contains(storedResponse.Key))
                {
                    return storedResponse.Value;
                }
            }

            Assert.Fail(string.Format("Failed to find a stored response for {0}", uri.AbsolutePath));

            return null;
        }

        /// <summary>
        /// Load a response from a file into an HttpContent object.
        /// </summary>
        /// <param name="filepath">filepath to be loaded.</param>
        /// <returns>Byte array of the mock data as read from the file.</returns>
        private HttpContent LoadContentFromFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                using (var fileStream = File.OpenRead(filepath))
                {
                    MemoryStream stream = new MemoryStream();
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.CopyTo(stream);

                    return new ByteArrayContent(stream.ToArray());
                }
            }

            return null;
        }
    }
}