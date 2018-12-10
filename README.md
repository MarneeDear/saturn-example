The Saturn App Template is best developed with Visual Studio Code or Visual Studio 2017 (Community works).

More about Saturn here

https://saturnframework.org/docs/

http://kcieslak.io/Reinventing-MVC-for-web-programming-with-F

This is for pure F# web applications and should be preferred over the original web app template.

You can target netstandard2.0, but in that case you wont be able to use the FSharp.Data SqlCommandProvider and some other F# type providers.

Here is a list of the possible target frameworks.

https://docs.microsoft.com/en-us/dotnet/standard/frameworks

## Optional libraries

Instead of Chessie for error handling I recommend the builtin Result type or FsToolkit.ErrorHandling which provides more utilities

https://demystifyfp.gitbook.io/fstoolkit-errorhandling/

FsToolkit.ErrorHandling has been installed in Infrastructure and WebHost

## Test Framework

### Required

* xUnit

* FsUnit

## Optional Recommendations

* TickSpec for BDD
  * https://github.com/fsprojects/TickSpec
* Canopy for UI tests 
  * https://lefthandedgoat.github.io/canopy/

# How to host development on IIS
By default this template will run as self-hosted using Kestrel, but you can host on IIS too.

1) Add a site to IIS and point it to the WebHost folder
2) Configure the bindings to whatever port you want to run it on. The template uses 8085.
3) Configure the Application Pool to use No Managed Code
4) Configure the web.config file
	1) Make sure processPath points to where the executable is stored when the project is built. This is template puts it here: .\bin\Debug\net461\Template.Saturn.WebHost.exe

## You can also host on IIS as a published site

The above steps will get you there but instead of pointing to the WebHost folder, use Visual Studio or `dotnet publish` to create a published site and use that in IIS.

## Adding packages

```bash
./paket/paket.exe add PACKAGENAME --project <your project>
```
```bash
dotnet restore src/Template.Saturn.Project.fspro
```
