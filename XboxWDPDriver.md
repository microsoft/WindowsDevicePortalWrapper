# Using XboxWDPDriver.exe for testing or production management

XboxWDPDriver.exe, from the TestAppXbox project, is used to target WDP on Xbox One to assist in automated testing as well as being useful for production development scenarios.

The app has built in help for most of its operations which can be accessed with the /? flag.

Scripts or other executables could be written to interface with XboxWDPDriver.exe to make simpler command line calls, and can mimic things like the Xb*.exe command line tools that may be familiar to some Xbox developers. An example of such a script is provided in the Tools directory when building the TestAppXbox project (XbUser.cmd).

### XboxWDPDriver Parameters

| Parameter               | Purpose                                        |
|-------------------------|------------------------------------------------|
| /Ip                     | The system IP address for the Xbox One console (required if no default console is set).          |
| /User                   | WDP username (if required, will be stored after the first connection starting with Windows 8).   |
| /Pwd                    | WDP password (if required, will be stored after the first connection starting with Windows 8).   |
| /Op                     | The operation to run. Run XboxWDPDriver without this parameter to get a list of all available operations.    |

Supported operations are the following:

  * connect
  * info
  * xbluser
  * install
  * reboot
  * processes
  * systemPerf
  * config
  * file
  * screenshot

### The connect operation

The ip parameter is required if no default console is configured. You can set a default console or list the current default console by using the 'connect' operation.

Examples:
```shell
XboxWDPDriver.exe /ip:10.0.0.1 /op:connect
```
or
```shell
XboxWDPDriver.exe /op:connect
```

### The info operation

Lists some basic information about the operating system and device name of this Xbox One console.

Example:
```shell
XboxWDPDriver.exe /op:info
```

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
XboxWDPDriver.exe /op:xbluser /subop:list
```

```shell
XboxWDPDriver.exe /op:xbluser /subop:signin /msa:testaccount@testdomain.com /msapwd:SuperSecret
```

```shell
XboxWDPDriver.exe /op:xbluser /subop:signout /id:16
```

```shell
XboxWDPDriver.exe /op:xbluser /subop:addsponsored
```

```shell
XboxWDPDriver.exe /op:xbluser /subop:autosigin /id:16 /state:on
```

```shell
XboxWDPDriver.exe /op:xbluser /subop:delete /id:16
```

### The install operation

Installs a UWP application from an appx or loose folder.

Usage:
```shell
  /appx:<path to Appx> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate>]
        Installs the given AppX package, along with any given dependencies.
  /folder:<path to loose folder> [/depend:<path to dependency1>;<path to dependency2> /cer:<path to certificate> /transfer:<SMB or HTTP, SMB is the default> /destfoldername:<folder name, defaults to the same as the loose folder>]
        Installs the appx from a loose folder, along with any given dependencies.
  /register:<subpath on DevelopmentFiles\LooseFolder to app to register>
        Registers a loose folder that is already present on the device.
```

Examples:
```shell
XboxWDPDriver.exe /op:install /appx:myappx.appx
```

```shell
XboxWDPDriver.exe /op:install /folder:myapploosefolder
```

```shell
XboxWDPDriver.exe /op:install /folder:myapploosefolder /transfer:HTTP
```

### The reboot operation

Reboots the target Xbox One console.

Example:
```shell
XboxWDPDriver.exe /op:reboot
```

### The processes operation

Lists all processes on the target Xbox One console.

Example:
```shell
XboxWDPDriver.exe /op:processes
```

### The systemPerf operation

Gives a summary of current system performance on the target Xbox One console (memory usage, etc).

Example:
```shell
XboxWDPDriver.exe /op:systemPerf
```

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
XboxWDPDriver.exe /op:config
```

```shell
XboxWDPDriver.exe /op:config /setting:TVResolution
```

```shell
XboxWDPDriver.exe /op:config /setting:TVResolution /value:1080p
```

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
XboxWDPDriver.exe /op:file /supop:knownfolders
```

```shell
XboxWDPDriver.exe /op:file /supop:dir /knownfolderid:DevelopmentFiles /subpath:VSRemoteTools
```

```shell
XboxWDPDriver.exe /op:file /supop:download /knownfolderid:DevelopmentFiles /subpath:VSRemoteTools/x64 /filename:dbgshim.dll /destination:c:\temp
```

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
XboxWDPDriver.exe /op:screenshot
```

```shell
XboxWDPDriver.exe /op:screenshot /filepath:c:\temp\screenshot.png
```

```shell
XboxWDPDriver.exe /op:screenshot /filepath:c:\temp\screenshot.png /override
```
