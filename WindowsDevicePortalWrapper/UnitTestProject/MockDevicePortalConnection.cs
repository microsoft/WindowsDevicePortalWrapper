using System;
using System.Net;
using Microsoft.Tools.WindowsDevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    internal class MockDevicePortalConnection : IDevicePortalConnection
    {
        public Uri Connection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetworkCredential Credentials
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Family
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DevicePortal.OperatingSystemInformation OsInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public byte[] GetDeviceCertificateData()
        {
            throw new NotImplementedException();
        }

        public void SetDeviceCertificate(byte[] certificateData)
        {
            throw new NotImplementedException();
        }

        public void UpdateConnection(bool requiresHttps)
        {
            throw new NotImplementedException();
        }

        public void UpdateConnection(DevicePortal.IpConfiguration ipConfig, bool requiresHttps)
        {
            throw new NotImplementedException();
        }
    }
}