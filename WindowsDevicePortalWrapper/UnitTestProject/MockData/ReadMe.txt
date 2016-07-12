Defaults contains mocks that are device and release agnostic to allow some 'general' testing of APIs.

Each device type can have its own folder as well for device specific responses, and subfolders 
under the device folder for release specific responses. This enables back-compat testing to ensure
new changes to the wrappers still operate as expected with responses from old devices.