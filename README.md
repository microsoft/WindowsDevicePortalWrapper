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

### \WindowsDevicePortalWrapper
The WindowsDevicePortalWrapper folder contains the Visual Studio 2015 c# project (.csproj), the source code required to build the DLL and a setttings file used by [StyleCop](http://stylecop.codeplex.com).

#### DevicePortal.cs
Implements public properties and establishes a connection to the device.

#### Utilities.cs
A static class providing utility functions utilized throughout the Windows Device Portal Wrapper project.

#### Settings.StyleCop

#### WindowsDevicePortalWrapper.csproj

---

### \WindowsDevicePortalWrapper\Core
The Core sub-folder contains the implementation of methods supported by all Windows Device Portal platforms.

#### AppDeployment.cs

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

### \WindowsDevicePortalWrapper\Events
The Interfaces sub-folder contains events defined by the Windows Device Portal Wrapper and their associated event arg classes.

#### ApplicationInstallStatus.cs

#### ConnectionStatus.cs

---

### \WindowsDevicePortalWrapper\Exceptions
The Interfaces sub-folder contains the exceptions defined by the Windows Device Portal Wrapper.

#### DevicePortalException.cs

---

### \WindowsDevicePortalWrapper\HoloLens
The HoloLens sub-folder contains the implmentation of HoloLens specific Windows Device Portal methods.

#### HolographicOs.cs

#### HolographicPerception.cs

#### HolographicThermal.cs

#### MixedRealityCapture.cs

#### PerceptionSimulationPlayback.cs

#### PerceptionSimulationRecording.cs

---

### \WindowsDevicePortalWrapper\HttpRest
The HttpRest sub-folder contains methods used to send requests and receive data from the Windows Device Portal's REST API.

#### RestDelete.cs

#### RestGet.cs

#### RestPost.cs

#### RestPut.cs

#### ServerCertificateValidation.cs

---

### \WindowsDevicePortalWrapper\Interfaces
The Interfaces sub-folder contains the definitions of interfaces defined by the Windows Device Portal Wrapper.

#### IDevicePortalConnection.cs

---

### \WindowsDevicePortalWrapper\Xbox
The Xbox sub-folder contains the implmentation of Xbox One specific Windows Device Portal methods.

#### SmbShare.cs

#### UserManagement.cs

#### XboxAppDeployment.cs

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
##### [Go back to the top of this file.](https://github.com/Microsoft/WindowsDevicePortalWrapper#WindowsDevicePortalWrapper)
---