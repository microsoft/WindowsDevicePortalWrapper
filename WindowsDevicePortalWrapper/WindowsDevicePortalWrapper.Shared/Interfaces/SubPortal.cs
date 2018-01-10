using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public class SubPortal
    {
        internal DevicePortal _portal;

        internal DevicePortal.DevicePortalPlatforms Platform
        {
            get => _portal.Platform;
        }

        internal string DeviceFamily
        {
            get => _portal.DeviceFamily;
        }

        internal IDevicePortalConnection deviceConnection => _portal.deviceConnection;
        
        
        internal async Task DeleteAsync(
            string apiPath,
            string payload = null)
        {
            await _portal.DeleteAsync(apiPath, payload);
        }
        
        internal async Task<T> DeleteAsync<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            return await _portal.DeleteAsync<T>(apiPath, payload);
        }
        
        internal async Task PutAsync<K>(
            string apiPath,
            K bodyData,
            string payload = null) where K : class
        {
            await _portal.PutAsync(apiPath, bodyData, payload);
        }
        
        internal async Task<T> PutAsync<T, K>(
            string apiPath,
            K bodyData = null,
            string payload = null) where T : new()
            where K : class
        {
            return await _portal.PutAsync<T, K>(apiPath, bodyData, payload);
        }

        internal async Task PostAsync(
            string apiPath,
            string payload = null)
        {
            await _portal.PostAsync(apiPath, payload);
        }
        
        internal async Task<Stream> PostAsync(
            Uri uri,
            Stream requestStream = null,
            string requestStreamContentType = null)
        {
            return await _portal.PostAsync(uri, requestStream, requestStreamContentType);
        }

        internal async Task PostAsync(
            string apiPath,
            List<string> files,
            string payload = null)
        {
            await _portal.PostAsync(apiPath, files, payload);
        }

        internal async Task<T> PostAsync<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            return await _portal.PostAsync<T>(apiPath, payload);
        }

        internal async Task<Stream> GetAsync(
            Uri uri)
        {
            return await _portal.GetAsync(uri);
        }
        
        internal async Task<T> GetAsync<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            return await _portal.GetAsync<T>(apiPath, payload);
        }
        
    }
}