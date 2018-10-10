The Saturn App Template is best developed with Visual Studio Code or Visual Studio 2017 (Community works).

More about Saturn here
https://saturnframework.org/docs/
http://kcieslak.io/Reinventing-MVC-for-web-programming-with-F

This is for pure F# web applications and should be preferred over the original web app template.

It can be built on dotnet core and is optimized for that, but in that case you wont be able to use the 
FSharp.Data SqlCommandProvider and some other F# type providers.

Optional libraries

Instead of Chessie for error handling I recommend the builtin Result type or FsToolkit.ErrorHandling which provides more utilites

https://demystifyfp.gitbook.io/fstoolkit-errorhandling/

FsToolkit.ErrorHandling has been installed in Infrastructure and WebHost

Test Framework
Required
xUnit
FsUnit

Optional
TickSpec for BDD
https://github.com/fsprojects/TickSpec
