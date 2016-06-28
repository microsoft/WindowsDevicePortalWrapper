# WindowsDevicePortalWrapper
A client library that wraps the Windows Device Portal REST APIs.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Source layout

The Windows Device Portal Wrapper source code has the following layout.

## \
The project root folder contains the Visual Studio 2015 solution (.sln) and all sub-folders that make up the project.

## \WindowsDevicePortalWrapper
The WindowsDevicePortalWrapper folder contains the Visual Studio 2015 c# project (.csproj), the source code required to build the DLL and a setttings file used by [StyleCop](http://stylecop.codeplex.com).

## \WindowsDevicePortalWrapper\Core
The Core sub-folder contains the implementation of methods supported by all Windows Device Portal platforms.

## \WindowsDevicePortalWrapper\Events
The Interfaces sub-folder contains events (ex: DeviiceConnnectionStatus) defined by the Windows Device Portal Wrapper and their associated event arg classes.

## \WindowsDevicePortalWrapper\Exceptions
The Interfaces sub-folder contains the exceptions (ex: DevicePortalException) defined by the Windows Device Portal Wrapper.

## \WindowsDevicePortalWrapper\HoloLens
The HoloLens sub-folder contains the implmentation of HoloLens specific Windows Device Portal methods.

## \WindowsDevicePortalWrapper\HttpRest
The HttpRest sub-folder contains methods used to send requests and receive data from the Windows Device Portal's REST API.

## \WindowsDevicePortalWrapper\Interfaces
The Interfaces sub-folder contains the definitions of interfaces (ex: IDeviceConnection) defined by the Windows Device Portal Wrapper.

## \WindowsDevicePortalWrapper\Xbox
The Xbox sub-folder contains the implmentation of Xbox One specific Windows Device Portal methods.

## \TestApp*
The TestApp* folders contain source code for simple applications showing how to use the implementation of the WindowsDevicePortalWrapper.

# Core methods

Please see the [Windows Device Portal Core API reference](https://msdn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal-api-core) for additional information.

# HoloLens specific methods

Please see the [HoloLens Device Portal API reference](https://developer.microsoft.com/en-us/windows/holographic/device_portal_api_reference) for additional information.

# Xbox One specific methods

Please see the [Xbox Device Portal API reference](https://msdn.microsoft.com/en-us/windows/uwp/xbox-apps/reference) for additional information.
