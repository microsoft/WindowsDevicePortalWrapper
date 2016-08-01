# Running Tests
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

4. Each test should start with a call to TestHelpers.MockHttpResponder.AddMockResponse to prepare a mock response for a specified enpoint in one of three ways:

  a. Default mock
  
  Use mock data from the MockData\Defaults\<endpoint>_Default.dat file.
  ```c#
  TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.KnownFoldersApi, response, HttpMethods.Get);
  ```
  b. Version specific mock
  
  Use mock data from the MockData\<platform>\<friendly OS version>\<endpoint>_<platform>_<friendly OS version>.dat file
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
