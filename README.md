# Saturn App

The Saturn App Template is best developed with Visual Studio Code or Visual Studio 2017 (Community works).

# Table of Contents

* [Background](#Background)
* [Getting Started](#Getting-Started)
    * [Prerequisites](##Prerequisites)
* [Test and Build](#Test-and-Build)
    * [Common Gotchas](##Common-Gotchas)
* [Test Frameworks](#Test-Frameworks)
    * [Required](##Required)
    * [Optional Libraries](##Optional-Libraries)
    * [Optional Recommendations](##Optional-Recommendations)
* [Deployment](#Deployment)
    * [Host on IIS](##How-to-host-development-on-IIS)
    * [Publish Site](##You-can-also-host-on-IIS-as-a-published-site)
* [Adding packages](#Adding-Packages)

# Background

More about Saturn here:

* [Saturn Framework Docs](https://saturnframework.org/docs/)
* [Reinventing MVC pattern for web programming with F#](http://kcieslak.io/Reinventing-MVC-for-web-programming-with-F)

This is for pure F# web applications and should be preferred over the original web app template.

You can target netstandard2.0, but in that case you wont be able to use the FSharp.Data SqlCommandProvider and some other F# type providers.

Here is a list of the possible target frameworks:

* [.NET Standard Frameworks](https://docs.microsoft.com/en-us/dotnet/standard/frameworks)

# Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment notes on how to host development on IIS.

## Prerequisites

Please review the MedSRP Development Workflow and Processes in Confluence.
1. Access to the `saturnapp` repository
2. Access to the TeamCity credentials for nuget packages
3. Access to Environment Variables
    * See `config.yaml` in `src/Template.Saturn.WebHost` directory
4. Download the .NET SDK
    * See `global.json` in the root `saturnapp` for target framework
5. Use Chocolately to install `nodejs` and `yarn`

# Test and Build
To get a better idea of what is happening behind these commands, see the `build-dotnet.fsx`.
```bash
fake build --target test
```
Once tests have passed, you are ready to run the `saturnapp`

```bash
fake build --target run
```

## Common Gotchas
1. Add mapping of localhost IP address to hostname in the `hosts` file on your machine

2. Update your `config.yaml` file with proper environment variable values

3. Run the `paket` command configure the TeamCity credentials securely on your system
4. See `paket.dependencies`
```bash
.paket/paket.exe config add-credentials <TeamCity-URL>
```

# Test Frameworks

## Required

* [xUnit](https://xunit.github.io/)
* [FsUnit](https://fsprojects.github.io/FsUnit/)

## Optional Libraries

Instead of Chessie for error handling I recommend the builtin Result type or FsToolkit.ErrorHandling which provides more utilities

* [FsToolkit.ErrorHanding](https://demystifyfp.gitbook.io/fstoolkit-errorhandling/)

FsToolkit.ErrorHandling has been installed in Infrastructure and WebHost

## Optional Recommendations

* [TickSpec](https://github.com/fsprojects/TickSpec) for BDD
* [Canopy](https://lefthandedgoat.github.io/canopy/) for UI tests 

# Deployment

## How to host development on IIS
By default this template will run as self-hosted using Kestrel, but you can host on IIS too.

1) Add a site to IIS and point it to the WebHost folder
2) Configure the bindings to whatever port you want to run it on. The template uses 8085.
3) Configure the Application Pool to use No Managed Code
4) Configure the web.config file
	1) Make sure processPath points to where the executable is stored when the project is built. This is template puts it here: .\bin\Debug\net461\Template.Saturn.WebHost.exe

## You can also host on IIS as a published site

The above steps will get you there but instead of pointing to the WebHost folder, use Visual Studio or `dotnet publish` to create a published site and use that in IIS.

# Adding Packages

```bash
./paket/paket.exe add PACKAGENAME --project <your project>
```
```bash
dotnet restore src/Template.Saturn.Project.fsproj
```
