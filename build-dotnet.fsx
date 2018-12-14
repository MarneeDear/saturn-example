#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake
open Fake.DotNet
open Fake.Core.TargetOperators

let appPath = "./src/Template.Saturn.WebHost/" |> Fake.IO.Path.getFullName

Core.Target.create "InstallDotNetCore" (fun _ ->
    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

Core.Target.create "Restore" (fun _ ->
    DotNet.restore (fun p -> p) "src\Template.Saturn.WebHost" |> ignore
)

Core.Target.create "Build"  (fun _ ->
    //DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
    DotNet.build (fun p -> p) "src\Template.Saturn.WebHost" |> ignore
)

Core.Target.create "Run" (fun _ -> 
  let server = async {
    DotNet.exec (fun p -> {p with WorkingDirectory = appPath}) "watch run" "" |> ignore
    }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://saturn.local:8085" |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Core.Target.create "Test" (fun _ -> 
    DotNet.test (fun p -> p) "src\Template.Saturn.Infrastructure.Tests"
)

Core.Target.create "Clean" (fun _ ->
    () //TODO cleanup the build folder
)

"Clean"
  ==> "InstallDotNetCore"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

"Clean"
  ==> "InstallDotNetCore"
  ==> "Build"
  ==> "Test"

Core.Target.runOrDefault "Test"

