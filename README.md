# WindowsDevicePortalWrapper
A client library that wraps the Windows Device Portal REST APIs.

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

# HoloLens specific methods

# Xbox One specific methods
