# Installing the project
The easiest way to get started is to clone this project to a local repo and open the solution in Visual Studio.  From there, you can compile the library (either .NET or UWP, as needed) and copy the DLL from the /bin/Release folder in that project.  Adding this DLL to an existing project will allow you to use Device Portal Wrapper. 

Alternately, to quickly test out the Device Portal Wrapper, you can open one of the SampleWdpClient apps and begin modifying from there. 

**Note**: Device Portal Wrapper requires .NET 4.5.2 from the [Windows SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk). 

# Using Device Portal Wrapper
The Device Portal Wrapper is built around a single object, the DevicePortal, which represents a Device Portal service instance running on a machine (either local or remote to the client).  The object is used to trigger Device Portal REST APIs on the target device (shutdown, app launch, etc). 

**Note**: The examples in the Getting Started guide assume that you have referenced Microsoft.Tools.WindowsDevicePortal in your project. 

## Creating a DevicePortal object
The DevicePortal object is initialized with a DevicePortalConnection object, which handles the connection between the client and the service on the target. The Device Portal Wrapper comes with a basic DefaultDevicePortalConnection implementation that can connect to IoT, HoloLens, Xbox, and Desktop instances. Once the object is created, the connection can be established using the Connect method. 
		
Example - initializing and establishing a Device Portal connection:
```C#	   
DevicePortal portal = new DevicePortal(
  new DefaultDevicePortalConnection(
    address, // a full URI (e.g. 'https://localhost:50443')
    username,
    password));
portal.Connect().Wait(); // The ConnectionStatus event will tell you if it succeeded 
```
For complete examples and stepping off points see the SampleWdpClients.  A UWP version and WPF version of the sample app are included in the solution.

## Using the DevicePortal object
Each REST API exposed by Device Portal is represented as a method off of the DevicePortal object. Most methods are a task that return an object representing the JSON return value.  

Example - finding a calculator app installed on the device: 
```C#
AppPackages apps = await portal.GetInstalledAppPackages();
PackageInfo package = apps.Packages.First(x => x.Name.Contains("calc"));
```

### Connecting over WebSocket
You can also establish websocket connections, for instance to get System Performance or Running Process information on a push basis.  Events are fired by the portal for each push of data from the server, and begin firing once the websocket connection is established. 

Example - print the process with the highest memory consumption, and stop listing processes if it's the Contoso process. 
```C#
await  portal.StartListeningForRunningProcesses();
portal.RunningProcessesMessageReceived += (portal, args) =>
  {
    DeviceProcessInfo proc =  args.Message.Processes.OrderByDescending(x=>x.TotalCommit).First();
    Console.WriteLine("Process with highest total commit:{0} - {1} ", proc.ProcessId, proc.Name);
    if (proc.Name.Contains("Contoso"))
      {
        portal.StopListeningForRunningProcesses();
      }
  };
```

## Using the Sample Apps and DefaultDevicePortalConnection
The SampleWdpClient apps are built on top of the DefaultDevicePortalConnection, which allows them to connect to most devices. At this time, the DefaultDevicePortalConnection is incompatible with Windows Phone. 
To connect using one of the sample apps, you can enter the full protocol, IP, and port for the target device.  

| Platform  | Default Uri |
| ------------- | ------------- |
| PC  | https://ipAddress:50443  |
| Xbox | https://ipAddress:11443  |
| IoT | http://ipAddress:8080 |
| HoloLens | https://ipAddress |

Once connected to the target device, you can see basic device information, collect the IP Config for the device, and power cycle it. 