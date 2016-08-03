ECHO OFF

REM This is an example of how a batch script can wrap the XboxWdpDriver to resemble traditional Xbox remote management tools.
REM It's designed so things like "XbUser list" can match exactly if a default console is set and WDP credentials are persisted.
REM However, it doesn't match XbUser.exe usage fully, but rather is an example of how you could do that. For making a full shim,
REM I'd recommend using a small EXE to help with the command line parsing and mapping from Xb*.exe's expected command line
REM usage to the XboxWDPDriver.exe expected usage. If you want to see more of this kind of shims, either as examples or in
REM fully functional and usable components, please open an Issue via the GitHub project and let us know what you want to see.

..\XboxWdpDriver.exe /op:xbluser /subop:%*