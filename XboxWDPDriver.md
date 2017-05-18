# Using XboxWdpDriver.exe for testing or production management of consoles

XboxWdpDriver.exe, built from the Samples/XboxWdpDriver project, is used to target WDP on Xbox One to assist in automated testing as well as being useful for production development scenarios.

The app has built in help for most of its operations which can be accessed with the /? flag.

Scripts or other executables could be written to interface with XboxWdpDriver.exe to make simpler command line calls, and can mimic things like the Xb*.exe command line tools that may be familiar to some Xbox developers. An example of such a script is provided in the Tools directory when building the TestAppXbox project (XbUser.cmd).

### XboxWdpDriver Parameters

| Parameter               | Purpose                                        |
|-------------------------|------------------------------------------------|
| /Ip                     | The system IP address for the Xbox One console (required if no default console is set).          |
| /User                   | WDP username (if required, will be stored after the first connection starting with Windows 8).   |
| /Pwd                    | WDP password (if required, will be stored after the first connection starting with Windows 8).   |
| /CertFile               | (optional) Path to a certificate file. This allows accepting an untrusted root certificate and allows specifying a proxy cert for a web proxy such as Fiddler |
| /Thumbprint             | (optional) Thumbprint for an SSL certificate that we are willing to accept from the console. This is another way to accept an untrusted certificate without providing the entire certificate file |
| /Op                     | The operation to run. Run XboxWdpDriver without this parameter to get a list of all available operations.    |

Supported operations (in alphabetical order) are the following:

  * [app](#app)
  * [config](#config)
  * [connect](#connect)
  * [fiddler](#fiddler)
  * [file](#file)
  * [info](#info)
  * [install](#install)
  * [processes](#processes)
  * [reboot](#reboot)
  * [sandbox](#sandbox)
  * [screenshot](#screenshot)
  * [systemPerf](#systemperf)
  * [xbluser](#xbluser)


<a name="app"/>
### The app operation

Allows getting the list of applications on the console and performing some basic lifetime management (launch, terminate, etc). Suspend and resume aren't currently supported but will be in the future.

Usage:
```shell
  /subop:list
        Lists all installed packages on the console.
  /subop:launch /pfn:<packageFullName> /aumid:<appId>
        Starts the requested application.
  /subop:terminate /pfn:<packageFullName>
        Stops the requested application.
  /subop:uninstall /pfn:<packageFullName>
        Removes or unregisters the given application from the console.
```

Examples:
```shell
XboxWdpDriver.exe /op:app /subop:list
```

```shell
XboxWdpDriver.exe /op:app /subop:launch /pfn:Microsoft.Xbox.DevHome_100.1607.22000.0_x64__8wekyb3d8bbwe /aumid:Microsoft.Xbox.DevHome_8wekyb3d8bbwe!App
```

```shell
XboxWdpDriver.exe /op:app /subop:terminate /pfn:Microsoft.Xbox.DevHome_100.1607.22000.0_x64__8wekyb3d8bbwe
```

```shell
XboxWdpDriver.exe /op:app /subop:uninstall /pfn:d15692ce-8b27-4bd3-9ceb-81652e9fea54_1.0.0.0_x64__55mw97kmv3wha
```

<a name="config"/>
### The config operation

Allows retrieving and setting some common system settings.

Usage:
```shell
  [/setting:<setting name> [/value:<setting value>]]
        Gets current settings and their values. If
        /setting is specified, only returns that value.
        If /value is also specified, sets the settting to
        that value instead of returning the current
        value.
```

Examples:
```shell
XboxWdpDriver.exe /op:config
```

```shell
XboxWdpDriver.exe /op:config /setting:TVResolution
```

```shell
XboxWdpDriver.exe /op:config /setting:TVResolution /value:1080p
```

<a name="connect"/>
### The connect operation

The ip parameter is required if no default console is configured. You can set a default console or list the current default console by using the 'connect' operation. Specifying the /thumbprint parameter to connect will cause the thumbprint to be persisted allowing future connections to trust the device without specifying the SLL thumbprint.

Examples:
```shell
XboxWdpDriver.exe /ip:10.0.0.1 /op:connect
```
or
```shell
XboxWdpDriver.exe /op:connect
```

Persisting the SSL thumbprint:
```shell
XboxWdpDriver.exe /op:connect /ip:10.0.0.1 /thumbprint:0000111122223333444455556666777788889999
```

<a name="fiddler"/>
### The Fiddler operation

Allows enabling and disabling of a Fiddler proxy for monitoring HTTP traffic on the console.

Usage:
```shell
  /state:<on or off> [/reboot] [/proxyaddress:<proxy address> /proxyport:<proxy port> /certpath:<path to cert file>]
        Whether to enable or disable Fiddler. Enabling and disabling Fiddler
        requires a reboot. You can specify the /reboot flag to do the reboot
        automatically. If Fiddler is being enabled, proxyaddress and proxyport
        are both required. If Fiddler has not been configured on this console
        previously, then the cert file is also required.
```

Examples:
```shell
XboxWdpDriver.exe /op:fiddler
```

```shell
XboxWdpDriver.exe /op:fiddler /state:on /proxyaddress:10.0.0.1 /proxyport:8888 /certpath:FiddlerRoot.cer
```

```shell
XboxWdpDriver.exe /op:fiddler /state:off /reboot
```

<a name="file"/>
### The file operation

Allows file operations on some known folders on the console (application specific storage via LocalAppData and development files via DevelopmentFiles).

LocalAppData operations require the package full name be provided.

Usage:
```shell
  /subop:knownfolders
        Lists all available known folder ids on the console
  /subop:dir /knownfolderid:<knownfolderid> [/subpath:<subpath>] [/packagefullname:<packageFullName>]
        Lists the directory contents at the given knownfoldid and optionally subpath.
  /subop:download /knownfolderid:<knownfolderid> /filename:<name of the file to download> /destination:<filepath for storing the file> [/subpath:<subpath>] [/packagefullname:<packageFullName>]
        Downloads the requested file to the desired destination.
  /subop:upload /knownfolderid:<knownfolderid> /filepath:<filepath of the file to upload> [/subpath:<subpath>] [/packagefullname:<packageFullName>]
        Uploads a file to the requested folder.
  /subop:rename /knownfolderid:<knownfolderid> /filename:<name of the file to rename> /newfilename:<new filename> [/subpath:<subpath>] [/packagefullname:<packageFullName>]
        Renames a given file.
  /subop:delete /knownfolderid:<knownfolderid> /filename:<name of the file to delete> [/subpath:<subpath>] [/packagefullname:<packageFullName>]
        Deletes the given file.
```

Examples:
```shell
XboxWdpDriver.exe /op:file /supop:knownfolders
```

```shell
XboxWdpDriver.exe /op:file /supop:dir /knownfolderid:DevelopmentFiles /subpath:VSRemoteTools
```

```shell
XboxWdpDriver.exe /op:file /supop:download /knownfolderid:DevelopmentFiles /subpath:VSRemoteTools/x64 /filename:dbgshim.dll /destination:c:\temp
```

<a name="info"/>
### The info operation

Lists some basic information about the operating system and device name of this Xbox One console.

Example:
```shell
XboxWdpDriver.exe /op:info
```

<a name="install"/>
### The install operation

Installs a UWP application from an appx or loose folder.

Usage:
```shell
  /appx:<path to Appx> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate>]
        Installs the given AppX package, along with any given dependencies.
  /folder:<path to loose folder> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate> /transfer:<SMB or HTTP, SMB is the default> /destfoldername:<folder name, defaults to the same as the loose folder>]
        Installs the appx from a loose folder, along with any given dependencies.
  /register:<subpath on DevelopmentFiles\LooseApps to app to register>
        Registers a loose folder that is already present on the device.
```

Examples:
```shell
XboxWdpDriver.exe /op:install /appx:myappx.appx
```

```shell
XboxWdpDriver.exe /op:install /folder:myapploosefolder
```

```shell
XboxWdpDriver.exe /op:install /folder:myapploosefolder /transfer:HTTP
```

```shell
XboxWdpDriver.exe /op:install /register:myapploosefolder
```

<a name="processes"/>
### The processes operation

Lists all processes on the target Xbox One console.

Example:
```shell
XboxWdpDriver.exe /op:processes
```

<a name="reboot"/>
### The reboot operation

Reboots the target Xbox One console.

Example:
```shell
XboxWdpDriver.exe /op:reboot
```

<a name="sandbox"/>
### The sandbox operation

Gets or sets the Xbox Live sandbox for the current Xbox One console.

Usage:
```shell
  [/value:<desired value> [/reboot]]
        Gets or sets the current Xbox Live sandbox value. Changing the current
        sandbox requires a reboot, which can be done automatically be specifying
        the /reboot flag.
```

Example:
```shell
XboxWdpDriver.exe /op:sandbox
```

Example:
```shell
XboxWdpDriver.exe /op:sandbox /value:MySandboxId /reboot
```

<a name="screenshot"/>
### The Screenshot operation

Allows taking of screenshots of the remote console.

Usage:
```shell
  [/filepath:<filepath> [/override]]
        Saves a screenshot from the console to the destination file specified
        by /filepath. This filename should end in .png so the file can be
        correctly read. If this parameter is not provided the screenshot is
        saved in the current directory as xbox_screenshot.png. This operation
        will fail if the file already exists unless the /override flag is specified.
```

Examples:
```shell
XboxWdpDriver.exe /op:screenshot
```

```shell
XboxWdpDriver.exe /op:screenshot /filepath:c:\temp\screenshot.png
```

```shell
XboxWdpDriver.exe /op:screenshot /filepath:c:\temp\screenshot.png /override
```

<a name="systemperf"/>
### The systemPerf operation

Gives a summary of current system performance on the target Xbox One console (memory usage, etc).

Example:
```shell
XboxWdpDriver.exe /op:systemPerf
```

<a name="xbluser"/>
### The xbluser operation

Controls listing and managing users on the console.

Usage:
```shell
  /subop:list
        Lists all Xbox Live Users on the console
  /subop:signin <user identifier (/msa:<msa> or /id:<id>)> [/msapwd:<password>]
        Signs in the given user, adding them to the console if necessary
  /subop:signout <user identifier (/msa:<msa> or /id:<id>)>
        Signs the given user out of the console
  /subop:addsponsored
        Adds a sponsored user to the console
  /subop:autosignin <user identifier (/msa:<msa> or /id:<id>)> <state (/on or /off)>
        Turns autosignin on or off for a given user
  /subop:delete <user identifier (/msa:<msa> or /id:<id>)>
        Deletes the given user from the console
```
Examples:
```shell
XboxWdpDriver.exe /op:xbluser /subop:list
```

```shell
XboxWdpDriver.exe /op:xbluser /subop:signin /msa:testaccount@testdomain.com /msapwd:SuperSecret
```

```shell
XboxWdpDriver.exe /op:xbluser /subop:signout /id:16
```

```shell
XboxWdpDriver.exe /op:xbluser /subop:addsponsored
```

```shell
XboxWdpDriver.exe /op:xbluser /subop:autosigin /id:16 /state:on
```

```shell
XboxWdpDriver.exe /op:xbluser /subop:delete /id:16
```

