dotnet-coveralls
=============
[![Build status](https://ci.appveyor.com/api/projects/status/up29r5k4ca0900q1/branch/master?svg=true)](https://ci.appveyor.com/project/abe545/dotnet-coveralls/branch/master) [![Coverage Status](https://coveralls.io/repos/github/abe545/dotnet-coveralls/badge.svg?branch=master)](https://coveralls.io/github/abe545/dotnet-coveralls?branch=master)

Hard fork of [coveralls.net](https://github.com/csMACnz/coveralls.net) to create a simple `dotnet coveralls` publishing cli

I am currently working on modularizing the code, and adding more tests. The only publisher that is really tested at all is Open Cover, however the original implementor claims that it works with the following coverage tools (feel free to submit a pr to add tests that prove they work!):

* Dynamic Code Coverage (CodeCoverage.exe)
* Visual Studio Code Coverage xml results
* Monocov coverage results (these apparently get output to a directory, which is the input for this coverage parser)
* Chutzpah json results
* lcov results

Installation
=============
It is published as a nuget package, but you should install it as a cli tool. It is generally only useful for .net core projects (and it is compiled against .net core 2.0, so you'll need that sdk to run this tool, and not sure if it will work for 1.x projects)
`<DotNetCliToolReference Include="dotnet-coveralls" Version="<latest-version-here>" />`

Usage
=============
After installation, full usage is available from the command line `dotnet coveralls --help`

You'll need to pass one or more coverage files to the command line, via the individual parser's switch. You can pass each file type switch multiple times (or mix and match your coverage types), and it will merge the coverage report for you. Here's an example of how this repo passes data to coveralls for its tests:

```
dotnet coveralls --open-cover C:\projects\dotnet-coveralls\test\dotnet-coveralls.Tests\coverage.xml --use-relative-paths --base-path C:\projects\dotnet-coveralls
```

I tried to support the documentation on [coveralls.io](https://docs.coveralls.io/supported-ci-services). Basically, all the `CI_*` environment variables can be used to pass the majority of the information that coveralls needs to generate reports. 

Appveyor users will have all this info provided for you, with the exception of the repo token.
