# Contributing to the Windows Device Portal Wrapper

The Windows Device Portal Wrapper welcomes contributions from the community.

# Process

1. [Make a proposal](https://github.com/Microsoft/WindowsDevicePortalWrapper/issues) (either new, or for one of the elements in our backlog)
2. Implement the proposal and its tests.
3. Run StyleCop and ensure compliance.
4. Rebase commits to tell a compelling story.
5. Start a pull request & address comments.
6. Merge.

# Proposal

For things like fixing typos and small bug fixes, you can skip this step.

If your change is more than a simple fix, please don't just create a big pull request. 
Instead, start by [opening an issue](https://github.com/Microsoft/WindowsDevicePortalWrapper/issues) describing the problem you want to solve and how you plan to approach the problem. This will let us have a brief discussion about the problem and, hopefully, identify some potential pitfalls before too much time is spent.

Note:  If you wish to work on something that already exists on our backlog, you can use that work item as your proposal.  

# Implementation

1. Fork the repository. Click on the "Fork" button on the top right of the page and follow the flow.
2. If your work needs more time, the consider branching off of master else just code in your fork.
3. Ensure your changes check for the appropriate device families (ex: Windows Desktop and IoT only).
4. Implement one or more [tests](https://github.com/Microsoft/WindowsDevicePortalWrapper/blob/master/Testing.md) to ensure the change works on the target platform(s).
5. Make small and frequent commits that include [tests](https://github.com/Microsoft/WindowsDevicePortalWrapper/blob/master/Testing.md) against mock data or manual tests which can be run against real devices.
6. Make sure that all existing [tests](https://github.com/Microsoft/WindowsDevicePortalWrapper/blob/master/Testing.md) continue to pass.

# Run StyleCop

The Windows Device Portal Wrapper uses the [StyleCop](http://stylecop.codeplex.com) code analysis tool to ensure code consistency and readability. This step is required for the WindowsDevicePortalWrapper folder and is optional (though highly recommended) for test applications.

1. Download and install the latest version of StyleCop.
2. Run StyleCop analysis on the project (In Visual Studio 2015, select Tools > Run StyleCop).
3. Update the source code to address detected issues.
4. Repeat steps 2 and 3 until analysis detects no issues.

If there is a StyleCop issue that you believe does not need to be enforced, please add the suppression entry either to your code or the Settings.StyleCop file in the appropriate folder. This will highlight the rule change and allow the community to comment.

# Rebase commits

The commits in your pull request should tell a story about how the code got from point A to point B. 
Good stories are edited, so you'll want to rebase your commits so that they tell a good story.

Each commit should build and pass all of the tests. 
If you want to add new tests for functionality that's not yet written, ensure the tests are added disabled.

Don't forget to run git diff --check to catch those annoying whitespace changes.
 
Please follow the established [Git convention for commit messages](https://www.git-scm.com/book/en/v2/Distributed-Git-Contributing-to-a-Project#Commit-Guidelines). 
The first line is a summary in the imperative, about 50 characters or less, and should not end with a period. 
An optional, longer description must be preceded by an empty line and should be wrapped at around 72 characters. 
This helps with various outputs from Git or other tools.

You can update message of local commits you haven't pushed yet using git commit --amend or git rebase --interactivewith reword command.

# Pull request

Start a GitHub pull request to merge your topic branch into the [main repository's master branch](https://github.com/Microsoft/WindowsDevicePortalWrapper/tree/master). 
(If you are a Microsoft employee and are not a member of the [Microsoft organization on GitHub](https://github.com/Microsoft) yet, please contact the DDE team via e-mail for instructions before starting your pull request. There's some process stuff you'll need to do ahead of time.)
If you haven't contributed to a Microsoft project before, you may be asked to sign a [contribution license agreement](https://cla.microsoft.com/). 
A comment in the PR will let you know if you do.

The project maintainers will review your changes. We aim to review all changes within three business days.
Address any review comments, force push to your topic branch, and post a comment letting us know that there's new stuff to review.

# Merge

If the pull request review goes well, a project maintainer will merge your changes. Thank you for helping improve the Windows Device Portal Wrapper!

# NuGet release and versioning

**For maintainers**

When creating a new NuGet and GitHub release, the following steps should be taken:
1. Bump the version number as appropriate in master (after 1.0, WDP Wrapper will correctly use semver)
2. Merge from Master to Release, with a PR appropriately named ("v1.2.3 release")
3. Squash and merge commits, leaving major feature entries and fixes in the description. 
4. Compile release builds of the .NET and UWP libraries, sign them, and upload to NuGet 
  a. We now have a CI server for this that builds, signs, and packages the NuGet package [here](https://microsoft.visualstudio.com/DefaultCollection/OS/_build/index?path=%5C&definitionId=14239).  
5. Cut a new release on GitHub using the same version number ("v1.2.3") and attach the signed libraries to the release. 
6. Update code documentation. 

# Updating code documentation

**For maintainers, since it's a total hack**

The Windows Device Portal Wrapper uses [Doxygen](http://www.stack.nl/~dimitri/doxygen/download.html) to automatically generate code documentation directly from the source code. Any changes to existing or new classes or methods should also update the documentation.

1. Download and install [Doxygen](http://www.stack.nl/~dimitri/doxygen/download.html) (our docs are generated using version 1.8.11).
2. Open a CMD prompt and navigate to your git repository's root directory.
3. Switch to the gh-pages branch
4. Run '\<Doxygen Install Location\>\\doxygen.exe DocConfig.txt'. This will update the files under the html folder relative to the root directory.
5. Delete everything but the html folder, then bring the contents of html/ to root.
6. Commit, and PR to the gh-pages branch. 
7. Ponder a better way to do this.

Alternate:

1. First time, do steps 1-3 above, only make the gh-pages branch a second clone of the repository to keep things cleaner and easier for the helper script and for the future.
2. Run the helper script from a command prompt starting at the root of this second clone: 'updateDocs.cmd \<path to your master repository\>'
3. Verify the docs look right on the [main site](https://microsoft.github.io/WindowsDevicePortalWrapper/)
