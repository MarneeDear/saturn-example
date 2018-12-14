#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake
open Fake.DotNet
open Fake.Core.TargetOperators
open Fake.IO

let appPath = "./src/Template.Saturn.WebHost/" |> Fake.IO.Path.getFullName
let infrastructureTestsPath = "./src/Template.Saturn.Infrastructure.Tests" |> Fake.IO.Path.getFullName

Core.Target.create "InstallDotNetCore" (fun _ ->
    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

Core.Target.create "Restore" (fun _ ->
    DotNet.restore (fun p -> p) appPath |> ignore
)

Core.Target.create "RenameConfig" (fun _ ->
    FileHelper.Rename (appPath @@ "config.yaml") (appPath @@ "config_design.yaml") |> ignore
)

Core.Target.create "Build"  (fun _ ->
    //DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
    DotNet.build (fun p -> p) "" |> ignore
)

Core.Target.create "Run" (fun _ -> 
  let server = async {
    DotNet.exec (fun p -> {p with WorkingDirectory = appPath}) "watch run" "" |> ignore
    }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://saturn.local:8085" |> ignore
  }

  //TODO consider running these in order. wait for the server to start up all the way first
  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Core.Target.create "Test" (fun _ -> 
    DotNet.test (fun p -> p) infrastructureTestsPath
)

Core.Target.create "Clean" (fun _ ->
    () //TODO cleanup the build folder
)

Core.Target.create "Publish" (fun _ ->
    DotNet.publish (fun p -> { p with OutputPath = Some "../../published"} ) appPath
)


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


Core.Target.runOrDefault "Test"

