//#r @"packages/build/FAKE/tools/FakeLib.dll"
#r "paket: groupref build //"
#load "./.fake/build-dotnet.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif


open System
//open Fake
open Fake.DotNet
open Fake.Core
open Fake.IO

let appPath = "./src/Template.Saturn.WebHost/" |> Fake.IO.Path.getFullName
let infrastructureTestsPath = "./src/Template.Saturn.Infrastructure.Tests" |> Fake.IO.Path.getFullName
//let serverPath = Path.getFullName "./src/Server"
//let clientPath = Path.getFullName "./src/Client"
//let deployDir = Path.getFullName "./deploy"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"

Target.create "InstallDotNetCore" (fun _ ->
    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    runTool yarnTool "--version" __SOURCE_DIRECTORY__
    runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
    //runDotNet "restore" clientPath
)

Target.create "Restore" (fun _ ->
    DotNet.restore (fun p -> p) appPath |> ignore
)

open Fake.IO.FileSystemOperators

Target.create "RenameConfig" (fun _ ->
    if not (File.exists(appPath @@ "config.yaml"))
        then Fake.IO.Shell.rename (appPath @@ "config.yaml") (appPath @@ "config_design.yaml") |> ignore
)

Target.create "Build"  (fun _ ->
    //DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
    //DotNet.build (fun p -> p) "" |> ignore
    runDotNet "build" appPath
    //runTool yarnTool "webpack-cli --config src/Client/webpack.config.js -p" __SOURCE_DIRECTORY__
)

Target.create "Run" (fun _ -> 
  let server = async {
    runDotNet "watch run" appPath |> ignore
    }

  let client = async {
        runTool yarnTool "webpack-dev-server --config src/Client/webpack.config.js" __SOURCE_DIRECTORY__
    }

  let browser = async {
    Threading.Thread.Sleep 5000
    openBrowser "http://saturn.local:8085" |> ignore
  }
  let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
  let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

  let tasks =
    [ if not safeClientOnly then yield server
      yield client
      if not vsCodeSession then yield browser ]


  //TODO consider running these in order. wait for the server to start up all the way first
  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Target.create "Test" (fun _ -> 
    DotNet.test (fun p -> p) infrastructureTestsPath
)

Target.create "Clean" (fun _ ->
    () //TODO cleanup the build folder
)

Target.create "Publish" (fun _ ->
    DotNet.publish (fun p -> { p with OutputPath = Some "../../published"} ) appPath
)

open Fake.Core.TargetOperators


"Clean" 
  ==> "InstallDotNetCore"
  ==> "RenameConfig"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

"Clean"
  ==> "InstallDotNetCore"
  ==> "RenameConfig"
  ==> "Build"
  ==> "Test"

"Clean"
  ==> "InstallDotNetCore"
  ==> "RenameConfig"
  ==> "Build"
  ==> "Test"
  ==> "Publish"


Target.runOrDefaultWithArguments "Test"

