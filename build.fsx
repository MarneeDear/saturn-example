//#r @"packages/build/FAKE/tools/FakeLib.dll"
#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif


open System
open Fake.DotNet
open Fake.Core
open Fake.IO

//https://fake.build/apidocs/v5/fake-core-buildservermodule.html#Functions%20and%20values
//https://fake.build/buildserver.html

//C:\development\comit\templates\saturnapp\paket-files\build\CompositionalIT\fshelpers\src\FsHelpers\ArmHelper
//#load @"paket-files/build/CompositionalIT/fshelpers/src/FsHelpers/ArmHelper/ArmHelper.fs"
#load @"paket-files\build\UACOMTucson\fshelpers\src\FsHelpers\ArmHelper\ArmHelper.fs"
open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters
open Microsoft.Azure.Management.ResourceManager.Fluent.Core

let appPath = "./src/Template.Saturn.WebHost/" |> Fake.IO.Path.getFullName
let testsPath = "./src/Template.Saturn.Tests" |> Fake.IO.Path.getFullName
//TODO you wlll need to fill this in if using Fable and SAFE Stack
//let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Template.Saturn.Client"
let deployDir = Path.getFullName "./deploy"

let isTeamCity =
    match BuildServer.buildServer with
    | TeamCity -> true
    | _ -> true


let platformTool tool winTool =
    Trace.trace (sprintf "Tool %s and WinTool %s" tool winTool)
    let runTool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath runTool with
    | Some t -> t
    | _ ->  let errorMsg =
                tool + " was not found in path. " +
                "Please install it and make sure it's available from your path. " +
                "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            failwith errorMsg

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir


let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

Target.create "InstallDotNetCore" (fun _ ->
    DotNet.install (fun p -> {p with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

Target.create "Restore" (fun _ ->
    DotNet.restore (fun p -> p) appPath |> ignore
)

open Fake.IO.FileSystemOperators

Target.create "UpdateConfiguration" (fun _ ->
    //Common.setAssemblyInfo productName version  TODO make this work with FAKE 5
    //TODO redo all of this
    match BuildServer.buildServer with
    | TeamCity ->
                    Trace.traceEnvironmentVariables |> ignore
                    //TODO apply teamcity env variables to config file
    | _ ->
                Trace.traceEnvironmentVariables |> ignore

)

Target.create "Build"  (fun _ ->
    runDotNet "build" appPath
    runDotNet "build" testsPath
)

Target.create "Run" (fun _ -> 
  let server = async {
    runDotNet "watch run" appPath |> ignore
    }


  let browser = async {
    Threading.Thread.Sleep 8000
    openBrowser "https://localhost" |> ignore
  }
  [server; browser;]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Target.create "Bundle" (fun _ ->
    runDotNet (sprintf "publish \"%s\%s\" -c release -o \"%s\"" appPath "Template.Saturn.WebHost.fsproj" deployDir) __SOURCE_DIRECTORY__
    Shell.copyDir (Path.combine deployDir "public") (Path.combine clientPath "public") FileFilter.allFiles
)

type ArmOutput =
    { WebAppName : ParameterValue<string>
      WebAppPassword : ParameterValue<string> }

let mutable deploymentOutputs : ArmOutput option = None

Trace.trace (sprintf "The build server is %s" (if isTeamCity then "TeamCity" else "Local"))

//let teamCityDeploy
//https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application
Target.create "ArmTemplate" (fun _ ->
    let environment = Environment.environVarOrDefault "environment" (Guid.NewGuid().ToString().ToLower().Split '-' |> Array.head)
    let armTemplate = @"arm-template.json" //TODO consider making this a parameter so can deploy to differnt environments
    let resourceGroupName =
        match Environment.environVar "resourceGroupName" with
        | name when String.IsNullOrEmpty(name) -> "experimental-deploy"
        | name -> name

    Trace.tracefn "RESOURCE GROUP IS %s" resourceGroupName

    let tenantId = try Environment.environVar "tenantId" |> Guid.Parse with _ -> failwith "Invalid Tenant ID. This should be your Azure Directory ID."    

    let developerDeploy () =

        //let authCtx =
        // You can safely replace these with your own subscription and client IDs hard-coded into this script.
        let subscriptionId = try Environment.environVar "subscriptionId" |> Guid.Parse with _ -> failwith "Invalid Subscription ID. This should be your Azure Subscription ID."
        let clientId = try Environment.environVar "clientId" |> Guid.Parse with _ -> failwith "Invalid Client ID. This should be the Client ID of a Native application registered in Azure with permission to create resources in your subscription."

        Trace.tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId
        subscriptionId
        |> authenticate Trace.trace { ClientId = clientId; TenantId = Some tenantId } //or authenticate and pass secret? but find out what happens when you use a tenantid
        //|> authenticateDevice {ClientId = clientId; ClientSecret = clientSecret; TenantId = tenantId}
        |> Async.RunSynchronously

    let unattendedDeploy () = 
        let tenantId = try Environment.environVar "tenantId" |> Guid.Parse with _ -> failwith "Invalid Tenant ID. This should be your Azure Directory ID."

        let clientSecret =
            match Environment.environVar "clientSecret" with
            | secret when String.IsNullOrEmpty(secret) -> failwith "Invalid Client ID. This should be your App Registration Secret Key."
            | secret -> secret

        //let authCtx =
        // You can safely replace these with your own subscription and client IDs hard-coded into this script.
        let subscriptionId = try Environment.environVar "subscriptionId" |> Guid.Parse with _ -> failwith "Invalid Subscription ID. This should be your Azure Subscription ID."
        let clientId = try Environment.environVar "clientId" |> Guid.Parse with _ -> failwith "Invalid Client ID. This should be the Client ID of a Native application registered in Azure with permission to create resources in your subscription."

        Trace.tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId
        subscriptionId
        |> authenticateDevice {ClientId = clientId; ClientSecret = clientSecret; TenantId = tenantId}

    let deployment =
        let location = Environment.environVarOrDefault "location" Region.USSouthCentral.Name
        let pricingTier = Environment.environVarOrDefault "pricingTier" "F1"
        { DeploymentName = "SATURN-template-deploy"
          ResourceGroup = New(resourceGroupName, Region.Create location)
          ArmTemplate = IO.File.ReadAllText armTemplate
          Parameters =
              Simple
                  [ "environment", ArmString environment
                    "location", ArmString location
                    "pricingTier", ArmString pricingTier ]
          DeploymentMode = Incremental }
    Trace.trace (sprintf "The build server is %s" (if isTeamCity then "TeamCity" else "Local"))
    deployment
    |> deployWithProgress (if isTeamCity then unattendedDeploy() else developerDeploy())
    |> Seq.iter(function
        | DeploymentInProgress (state, operations) -> Trace.tracefn "State is %s, completed %d operations." state operations
        | DeploymentError (statusCode, message) -> Trace.traceError <| sprintf "DEPLOYMENT ERROR: %s - '%s'" statusCode message
        | DeploymentCompleted d -> deploymentOutputs <- d)
)

open Fake.IO.Globbing.Operators
open System.Net

// https://github.com/SAFE-Stack/SAFE-template/issues/120
// https://stackoverflow.com/a/6994391/3232646
type TimeoutWebClient() =
    inherit WebClient()
    override this.GetWebRequest uri =
        let request = base.GetWebRequest uri
        request.Timeout <- 30 * 60 * 1000
        request

//Used to be AppService
Target.create "Deploy" (fun _ ->
    //create the deploy folder if it does not exist
    if not (Shell.testDir deployDir) then
        Shell.mkdir deployDir

    let zipFile = "deploy.zip"
    IO.File.Delete zipFile
    Zip.zip deployDir zipFile !!(deployDir + @"\**\**")

    let appName = deploymentOutputs.Value.WebAppName.value
    let appPassword = deploymentOutputs.Value.WebAppPassword.value

    let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
    let client = new TimeoutWebClient(Credentials = NetworkCredential("$" + appName, appPassword))
    Trace.tracefn "Uploading %s to %s" zipFile destinationUri
    client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore
    )


Target.create "Test" (fun _ -> 
    //DotNet.test (fun p -> p) testsPath Use this for xunit
    runDotNet "run" testsPath |> ignore //Use this for expecto

)

Target.create "Clean" (fun _ ->
    () //TODO cleanup the deploy folder
)

Target.create "Publish" (fun _ ->
    DotNet.publish (fun p -> { p with OutputPath = Some "./published"} ) appPath
)

//Target.create "DeployToAzure" (fun _ ->
//    Target.runOrDefaultWithArguments "AppService"
//)

open Fake.Core.TargetOperators


"Clean" 
  //==> "InstallDotNetCore"
  //==> "InstallClient"
  //==> "CopyConfig"
  ==> "UpdateConfiguration"
  ==> "Restore"
  ==> "Build"

"Clean"
    //==> "InstallClient"
    //==> "CopyConfig"
    ==> "UpdateConfiguration"
    ==> "Restore"
    ==> "Build"
    //==> "Bundle"
    ==> "Test"
    ==> "ArmTemplate"
    ==> "Deploy"

"Clean"
  //==> "InstallClient"
  ==> "Restore"
  ==> "Run"

"Clean"
  //==> "InstallDotNetCore"
  //==> "InstallClient"
  //==> "CopyConfig"
  ==> "UpdateConfiguration"
  ==> "Restore"
  ==> "Build"
  ==> "Test"

"Clean"
  //==> "InstallDotNetCore"
  //==> "CopyConfig"
  ==> "UpdateConfiguration"
  ==> "Restore"
  ==> "Build"
  ==> "Test"
  ==> "Publish"

//"Clean"
//  //==> "CopyConfig"
//  ==> "UpdateConfiguration"


Target.runOrDefaultWithArguments "Test"

