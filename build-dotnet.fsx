#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake
open Fake.DotNet
open Fake.Core.TargetOperators

let appPath = "./src/Template.Saturn.WebHost/" |> Fake.IO.Path.getFullName
//FullName 

//maybe dont need to do this
//let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson() 

//let DoNothing = ignore


Core.Target.create "InstallDotNetCore" (fun _ ->
    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

//Target "InstallDotNetCore" (fun _ ->
//    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
//  //dotnetcliVersion|> ignore
//  //InstallDotNetSDK dotnetcliVersion |> ignore
//)

Core.Target.create "Restore" (fun _ ->
    //DotNetCli.Restore (fun p ->  {p with WorkingDir = appPath})
    DotNet.restore (fun p -> p) "src\Template.Saturn.WebHost" |> ignore
)

//Target "Restore" (fun _ ->
//    //DotNetCli.Restore (fun p ->  {p with WorkingDir = appPath})
//    DotNet.restore (fun p -> p) "src\Template.Saturn.WebHost" |> ignore
//)

Core.Target.create "Build"  (fun _ ->
    //DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
    DotNet.build (fun p -> p) |> ignore
)

//Target "Build" (fun _ ->
//    //DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
//    DotNet.build (fun p -> p) |> ignore
//)

Core.Target.create "Run" (fun _ -> 
  let server = async {
    DotNet.exec (fun p -> {p with WorkingDirectory = appPath}) "watch run" "" |> ignore
    }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://saturn-local:8085" |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

//Target "Run" (fun () ->
//  //let server = async {
//  //  //DotNetCli.RunCommand (fun p -> {p with WorkingDir = appPath}) "watch run"
    
//  //}
//  let server = async {
//    DotNet.exec (fun p -> p) "watch run" "" |> ignore
//    }
//  let browser = async {
//    Threading.Thread.Sleep 5000
//    Diagnostics.Process.Start "http://localhost:8085" |> ignore
//  }

//  [ server; browser]
//  |> Async.Parallel
//  |> Async.RunSynchronously
//  |> ignore
//)

Core.Target.create "Test" (fun _ -> 
    DotNet.test (fun p -> p) "src\Template.Saturn.Infrastructure.Tests"
)

//Target "Test" (fun _ -> 
//    DotNet.test (fun p -> p) "src\Template.Saturn.WebHost"
//)

Core.Target.create "Clean" (fun _ ->
    () //TODO cleanup the build folder
)
//|> DoNothing


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

//RunTargetOrDefault "Test"