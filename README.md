# WindowsDevicePortalWrapper
A client library that wraps the Windows Device Portal REST APIs.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

---
---

## Source layout

The Windows Device Portal Wrapper source code has the following layout.

### \
The project root folder contains the Visual Studio 2015 solution (.sln) and all sub-folders that make up the project.

#### WindowsDevicePortalWrapper.sln

#### License.txt

#### CONTRIBUTING.md

#### README.md

---

### \WindowsDevicePortalWrapper.Shared
The WindowsDevicePortalWrapper.Shared folder is a code only project that contains platform independent code for accessing the Windows Device Portal. Large methods use conditional compilation blocks for platform specific code. Small methods requiring platform specific code are duplicated into source files residing in the folders containing that platform's .csproj file.

#### DevicePortal.cs
The root file for the Microsoft.Tools.WindowsDevicePortal.DevicePortal class.

Events:
(All events are part of the DevicePortal class)
* ConnectionStatus

Properties:
(All properties are part of the DevicePortal class)
* Address
* ConnectionHttpStatusCode
* DeviceFamily
* OperatingSystemVersion
* Platform
* PlatformName

Methods:
(All methods are part of the DevicePortal class)
* DevicePortal()
* Connect()
* SendConnectionStatus()

#### Utilities.cs
A static class providing utility functions utilized throughout the Windows Device Portal Wrapper project.

Methods:
(All methods are part of the Utilities class)
* BuildEndpoint()
* Hex64Encode()

---

### \WindowsDevicePortalWrapper.Shared\Core
The Core sub-folder contains the implementation of methods supported by all Windows Device Portal platforms.

#### AppDeployment.cs

Events:
(All events are part of the DevicePortal class)
* AppInstallStatus

Methods:
(All methods are part of the DevicePortal class)
* CopyInstallationFileToStream()
* CreateAppInstallEndpointAndBoundaryString()
* GetInstalledAppPackages()
* InstallApplication()
* UninstallApplication()
* SendAppInstallStats()

Defines the data contract used by the application deployment methods.

#### AppFileExplorer.cs

#### DeviceManager.cs

#### Dns-Sd.cs

#### DumpCollection.cs

#### Etw.cs

#### Networking.cs

#### OsInformation.cs

#### PerformanceData.cs

#### Power.cs

#### RemoteControl.cs

#### TaskManager.cs

#### WiFiManagement.cs

#### WindowsErrorReporting.cs

#### WindowsPerformanceRecorder.cs

---

### \WindowsDevicePortalWrapper.Shared\Events
The Interfaces sub-folder contains events defined by the Windows Device Portal Wrapper and their associated event arg classes.

#### ApplicationInstallStatus.cs

#### ConnectionStatus.cs

#### WebSocketMessageReceivedEventArgs.cs

---

### \WindowsDevicePortalWrapper.Shared\Exceptions
The Interfaces sub-folder contains the exceptions defined by the Windows Device Portal Wrapper.

#### DevicePortalException.cs

---

### \WindowsDevicePortalWrapper.Shared\HoloLens
The HoloLens sub-folder contains the implmentation of HoloLens specific Windows Device Portal methods.

#### HolographicOs.cs

#### HolographicPerception.cs

#### HolographicThermal.cs

#### MixedRealityCapture.cs

#### PerceptionSimulationPlayback.cs

#### PerceptionSimulationRecording.cs

---

### \WindowsDevicePortalWrapper.Shared\HttpRest
The HttpRest sub-folder contains methods used to send requests and receive data from the Windows Device Portal's REST API.

#### CsrfToken.cs
Provides CSRF token management support.

Methods:
(All methods are part of the DevicePortal class)
* ApplyCsrfToken()
Sets the CSRF token header on the HTTP request.
* RetrieveCsrfToken
Gets the CSRF token header value from the HTTP response and stores it for future use.

#### RestDelete.cs

Methods:
(All methods are part of the DevicePortal class)
* Delete()

#### RestGet.cs

Methods:
(All methods are part of the DevicePortal class)
* Get<T>()

#### RestPost.cs

Methods:
(All methods are part of the DevicePortal class)
* Post()

#### RestPut.cs

Methods:
(All methods are part of the DevicePortal class)
* Put()
* Put<T>()

#### WebSocket.cs

---

### \WindowsDevicePortalWrapper.Shared\Interfaces
The Interfaces sub-folder contains the definitions of interfaces defined by the Windows Device Portal Wrapper.

#### IDevicePortalConnection.cs
Defines the IDevicePortalConnection interface, for which an implementation must be provided by tools which consume the Windows Device Portal Wrapper.

---

### \WindowsDevicePortalWrapper.Shared\Xbox
The Xbox sub-folder contains the implmentation of Xbox One specific Windows Device Portal methods.

#### SmbShare.cs

#### UserManagement.cs

#### XboxAppDeployment.cs

---

### \WindowsDevicePortalWrapper
The WindowsDevicePortalWrapper folder contains the references and files required to build a the Windows Device Portal Wrapper project targeting version 4.5.2 of the .net framework.

#### CertificateHandling.cs

Methods:
(All methods are part of the DevicePortal class)
* GetDeviceCertificate()
* ServerCertificateValidation()
* ServerCertificateNonValidation()

---

### \WindowsDevicePortalWrapper\Core
The WindowsDevicePortalWrapper\Core folder contains the .net 4.5.2 specific Core Device Portal API functionality.

#### AppDeployment.cs

Methods:
(All methods are part of the DevicePortal class)
* GetInstallStatus()

---

### \WindowsDevicePortalWrapper\HttpRest
The WindowsDevicePortalWrapper\Core folder contains the .net 4.5.2 specific HTTP REST functionality.

#### RestDelete.cs

Methods:
(All methods are part of the DevicePortal class)
* Delete(Uri)

#### RestGet.cs

Methods:
(All methods are part of the DevicePortal class)
* Get(Uri)

#### RestPost.cs

Methods:
(All methods are part of the DevicePortal class)
* Post(Uri)

#### RestPut.cs

Methods:
(All methods are part of the DevicePortal class)
* Put(Uri)

---

### \WindowsDevicePortalWrapper.UniversalWindows
The WindowsDevicePortalWrapper.UnversalWindows folder contains the references and files required to build a the Windows Device Portal Wrapper project targeting the Windows 10 Universal Windows Platform.

#### CertificateHandling.cs

Methods:
(All methods are part of the DevicePortal class)
* GetDeviceCertificate()
* SetDeviceCertificate()

---

### \WindowsDevicePortalWrapper.UniversalWindows\Core
The WindowsDevicePortalWrapper\Core folder contains the Universal Windows Platform specific Core Device Portal API functionality.

#### AppDeployment.cs

Methods:
(All methods are part of the DevicePortal class)
* GetInstallStatus()

---

### \WindowsDevicePortalWrapper.UniversalWindows\HttpRest
The WindowsDevicePortalWrapper\Core folder contains the Universal Windows Platform specific HTTP REST functionality.

#### RestDelete.cs

Methods:
(All methods are part of the DevicePortal class)
* Delete(Uri)

#### RestGet.cs

Methods:
(All methods are part of the DevicePortal class)
* Get(Uri)

#### RestPost.cs

Methods:
(All methods are part of the DevicePortal class)
* Post(Uri)

#### RestPut.cs

Methods:
(All methods are part of the DevicePortal class)
* Put(Uri)

---

### Core methods

Please see the [Windows Device Portal Core API reference](https://msdn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal-api-core) for additional information.

---

### HoloLens specific methods

Please see the [HoloLens Device Portal API reference](https://developer.microsoft.com/en-us/windows/holographic/device_portal_api_reference) for additional information.

---

### Xbox One specific methods

Please see the [Xbox Device Portal API reference](https://msdn.microsoft.com/en-us/windows/uwp/xbox-apps/reference) for additional information.

---
