# Running Tests


There are two types of tests for excercising the wrapper library. 

First, unit tests built against mock responses should be run before every Pull Request. Use Run All via the Test Explorer in Visual Studio prior to submitting any Pull Requests. You can also configure Visual Studio to automatically run the tests after each build of the solution via Test Settings in the Test menu.
<br>

  ![runalltests](https://cloud.githubusercontent.com/assets/19478513/17338380/16900018-589c-11e6-87a6-8091d62ce399.png)

Second, manual or semi-automated tests can also be run on a per-device basis via the device specific apps. See the following for information on running these apps:

For Xbox One: [Using the XboxWdpDriver to test wrappers against a real Xbox One](https://github.com/Microsoft/WindowsDevicePortalWrapper/blob/master/XboxWDPDriver.md)

# Writing Tests

Windows Device Portal Wrapper (WDPW) tests are run against mock data so that a windows device is not required to run the tests.

1.	Create a Unit Test class in the Unit Test Project
  ![addunittest](https://cloud.githubusercontent.com/assets/1520739/17310088/225af006-57f7-11e6-834c-7735b7270d85.png)

2.	Have the test class inherit from the BaseTests class which will automatically establish your mock connection (the constructor and TestContext can be removed)

  ```c#
 /// <summary>
  /// Summary description for DemoTest
  /// </summary>
  [TestClass]
  public class DemoTest : BaseTests
  {
  }
  ```
  
  a. If you are writing a device specific version test then override the PlatformType, the FriendlyOperatingSystemVersion used to specify where mock files are stored/named, and the OperatingSystemVersion to be used when validating the OS’s actual version number.
  
  ```c#
 /// <summary>
/// Gets the Platform type these tests are targeting.
/// </summary>
protected override DevicePortalPlatforms PlatformType
{
  get { return DevicePortalPlatforms.XboxOne;}
}

/// <summary>
/// Gets the friendly OS Version these tests are targeting.
/// </summary>
protected override string FriendlyOperatingSystemVersion
{
  get { return "rs1_xbox_rel_1608";}
}

/// <summary>
/// Gets the OS Version these tests are targeting.
/// </summary>
protected override string OperatingSystemVersion
{
  get { return "14385_1002_amd64fre_rs1_xbox_rel_1608_160709_1700";}
}
  ```

3.	Define void methods with no parameters tagged with “testMethod” for each test case you want to run

  ```c#
  [TestMethod]
  public void TestMethod1()
  {
    //
    // TODO: Add test logic here
    //
  }
  ```

4. Each test should start with a call to TestHelpers.MockHttpResponder.AddMockResponse to prepare a mock response for a specified endpoint in one of three ways:

  a. Default mock
  
  Use mock data from the MockData\Defaults\ \<endpoint\>\_Default.dat file.
  ```c#
  TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.MachineNameApi, HttpMethods.Get);
  ```
  b. Version specific mock
  
  Use mock data from the MockData\ \<platform>\ \<friendly OS version\>\ \<endpoint\>\_\<platform\>\_\<friendly OS version\>.dat file
  ```c#
  TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.KnownFoldersApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);
  ```
  c. Provided response
  
  Use a response object as a mock.
  ```c#
  TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFilesApi, response, HttpMethods.Get);
  ```
5. Have the test methods use TestHelpers.Portal to call DevicePortal methods and then Assert methods to validate results

  ```c#
  Task<string> getNameTask = TestHelpers.Portal.GetDeviceName();
  getNameTask.Wait();
  Assert.IsNotNull(TestHelpers.Portal.OperatingSystemVersion);
  ```
  
6. After building the test right click on it in the test explorer and select “Debug Selected Tests” to be able to hit selected break points as it runs.

  ![debugselectedtests](https://cloud.githubusercontent.com/assets/1520739/17310093/27a33636-57f7-11e6-8cab-45620c167dcf.png)

# Collecting Data for Tests

## Using MockDataGenerator.exe to Generate Mock Data

MockDataGenerator.exe, from the MockDataGenerator project, is used to target a WDP instance to create mock data.

### MockDataGenerator Parameters

| Parameter                  | Purpose                                        |
|----------------------------|------------------------------------------------|
| /Address                   | URL for device (eg. https://10.0.0.1:11443)    |
| /User                      | WDP username                                   |
| /Pwd                       | WDP password                                   |
| /Endpoint                  | API to call (default is all endoints in program.cs)   |
| /Method                    | HTTP method to use (default is GET)            |
| /Directory                 | Directory to save mock data file(s) (default is .\MockData) |
| /requestBody               | File to use for the request body. Only applicable when /Endpoint is present. |
| /requestBodyMultiPartFile  | Normally the requestBody file is considered JSON and provided as is. Specify this parameter to include it as a multipart file instead. |

### MockDataGenerator File Name Format

All mock data is saved as files in the \<endpoint\>\_\<platform\>\_\<friendly OS version\>.dat format, such as api_os_devicefamily_XboxOne_rs1_xbox_rel_1608.dat. 

All file names are pre-pended with the HTTP method except for GET (as it is the default). Hence the file name for calling the System Performance API with GET is api_resourcemanager_systemperf_XboxOne_rs1_xbox_rel_1608.dat but calling the WebSocket is WebSocket_api_resourcemanager_systemperf_XboxOne_rs1_xbox_rel_1608.dat.

### MockDataGenerator Examples

All examples connect to 10.0.0.1:11443 with username TestUser and password SuperSecret.
* Generate all mocks specified in the Program.cs Endpoints array and store them in .\MockData

  ```shell
  MockDataGenerator.exe  /ip: 10.0.0.1:11443 /user:TestUser /pwd:SuperSecret
  ```

* Generate mock for the device family API and store it in .\MockData
  
  ```shell
  MockDataGenerator.exe  /ip: 10.0.0.1:11443 /user:TestUser /pwd:SuperSecret /endpoint:api/os/devicefamily
  ```
  or
  ```shell
  MockDataGenerator.exe  /ip: 10.0.0.1:11443 /user:TestUser /pwd:SuperSecret /endpoint:api/os/devicefamily /method:Get
  ```
  
* Generate mock for the System Performance API web socket connection and store it in .\MockData

  ```shell
  MockDataGenerator.exe  /ip: 10.0.0.1:11443/user:TestUser /pwd:SuperSecret /endpoint:api/resourcemanager/systemperf /method:WebSocket
  ```

* Generate mock for a POST call to upload a file, specifying the file to use for the request body.

    ```shell
  MockDataGenerator.exe  /ip: 10.0.0.1:11443/user:TestUser /pwd:SuperSecret /endpoint:api/filesystem/apps/file?knownfolderid=LocalAppData&packagefullname=MyTestPackage_Fullname /method:Post /requestBody:myfile.txt /requestBodyMultiPartFile
  ```
  
  ## Adding Mock Data to the Solution
  
  1. Mock data should be added to the UnitTestProject in the MockData directory. 
  
    * Default mock data should be added to the MockData\Defaults directory.
    
      ![defaultmocks](https://cloud.githubusercontent.com/assets/1520739/17312218/ff62b160-5805-11e6-92f9-0934fc50b961.png)

  
    * Device-version mock data should be added to the MockData<Device\>\ \<Friendly OS Version\> directory with the Friendly OS Version parsed from the mock data’s file name.
    
      ![platformspecificmocks](https://cloud.githubusercontent.com/assets/1520739/17312269/5248edf4-5806-11e6-833e-cb2445ffc0f1.png)

  
  2. In the properties view the mock data files should have their “Copy to Output Directory” property marked as “Copy if newer.” If this is not done then the tests will be unable to find the files.
  
    ![copytooutputdirectory](https://cloud.githubusercontent.com/assets/1520739/17312271/55911450-5806-11e6-9616-eaf7de842121.png)

