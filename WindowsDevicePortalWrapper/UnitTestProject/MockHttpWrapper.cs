using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    internal class MockHttpWrapper : HttpWrapper
    {
        public override Task<HttpResponseMessage> DeleteAsync(HttpClient client, Uri uri)
        {
            throw new NotImplementedException();
        }

        public override Task<HttpResponseMessage> GetAsync(HttpClient client, Uri uri)
        {
            throw new NotImplementedException();
        }

        public override Task<HttpResponseMessage> PostAsync(HttpClient client, Uri uri, HttpContent content)
        {
            throw new NotImplementedException();
        }

        public override Task<HttpResponseMessage> PutAsync(HttpClient client, Uri uri, HttpContent content)
        {
            throw new NotImplementedException();
        }
    }
}