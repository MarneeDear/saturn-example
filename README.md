# Saturn Example

The Saturn App Template is best developed with Visual Studio Code or Visual Studio 2019 (Community works).

## More about Saturn

https://saturnframework.org/docs/

http://kcieslak.io/Reinventing-MVC-for-web-programming-with-F

This is for pure F# web applications and should be preferred over the original web app template.

Here is a list of the possible target frameworks.
https://docs.microsoft.com/en-us/dotnet/standard/frameworks

## Features

* Example CAS setup
* Examples of an access restricted application using two routes
  * Logged In View
  * Default View
* Examples cookies setup where you can plug in EDS requests to get membership and other user details

## How to get me running

Make sure you have .NET Core 3.1 SDK installed

1. Clone me
2. Add a hostname
	1. `saturn.local` `127.0.0.1`
3. Add an environment variable. This will tell ASP.NET Core that you want to use Development settings
	1. key: ASPNETCORE_ENVIRONMENT
	2. value: Development
4. Copy appsettings.json and rename it to appsettings.Development.json
5. __DO NOT PUT SENSITIVE SECRET SETTINGS IN__ appsettings.json. __ONLY PUT SETTINGS IN__ appsettings.Development.json. __NEVER COMMIT appsettings.Development.json__
	1. You do not need to change the settings to get the app to run. Only do this when you use the temaple to build a real application.
6. Install paket
	1. `dotnet tool install paket`
	2. `dotnet tool restore`
7. Install fake
	1. `dotnet tool install fake-cli`
	2. `dotnet tool restore`
8. Run the build and launch the app
	1. In `cmd` go to the root of the application. 
	2. Do `dotnet fake build target Run`
	2. The app should launch in a browser


## Test Framework

### Required

See the paket.dependencies file for all testing libraries

* Excpecto

## Optional Recommendations

* TickSpec for BDD
https://github.com/fsprojects/TickSpec
* Canopy for UI tests https://lefthandedgoat.github.io/canopy/

# How to host development on IIS
By default this template will run as self-hosted using Kestrel, but you can host on IIS too.

1) Add a site to IIS and point it to the WebHost folder
2) Configure the bindings to whatever port you want to run it on. The template uses 8085.
3) Configure the Application Pool to use No Managed Code
4) Configure the web.config file
	1) Make sure processPath points to where the executable is stored when the project is built. This is template puts it here: .\bin\Debug\net461\Template.Saturn.WebHost.exe

## You can also host on IIS as a published site

The above steps will get you there but instead of pointing to the WebHost folder, use Visual Studio or `dotnet publish` to create a published site and use that in IIS. You may need to copy over the web.config file manually. I don't know why.

## Deploying to Azure

TODO

## I might be running on Azure

https://saturn-example.azurewebsites.net/

You can login with your Net ID.