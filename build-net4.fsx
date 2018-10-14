// Include Fake lib
#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake.Testing
#r @"packages/build/COM.Build/lib/net461/Com.Build.dll"

open Fake
open Fake.AssemblyInfoFile
open Com.Build

// Properties
let version = "0.1"
let productName = "College of Medicine Saturn Template <REPLACE ME>"
let solutionFile = "SaturnAppTemplate.sln"
let webprojdir = currentDirectory + @"/src/Template.Saturn.WebHost"
let infrastructuredir = currentDirectory + @"/src/Template.Saturn.Infrastructure"
let testdir = currentDirectory + @"/src"
let nlogfile = webprojdir + @"/NLog.config"

let fileVersion = (sprintf "%s.%s" version Common.buildNumber)

Target "RestorePackages" (fun _ ->
    Common.restorePackages solutionFile
)

let updateSecrets () =
    match buildServer with 
    | TeamCity -> ()
        //Youll need this in the test app configs in order to build against a database if using one              
        // updateConnectionString "SqlProvider" (environVar "SqlProvider.ConnectionString") (infrastructuredir @@ "app.config")  
        // //Youll need this in the test app configs in order to build against a database if using one              
        // updateConnectionString "SqlProvider" (environVar "SqlProvider.ConnectionString") (testdir @@ "/Template.Infrastructure.Tests/app.config")                
        // updateAppSetting "Eds.Username" (environVar "Eds.Username") (webprojdir @@ "web.config")
        // updateAppSetting "Eds.Password" (environVar "Eds.Password") (webprojdir @@ "web.config")
        // updateConnectionString "DefaultConnection" (environVar "Database.ConnectionString") (webprojdir @@ "web.config")        
        // //Used for Hangfire
        // //updateConnectionString "Hangfire" (environVar "Hangfire.ConnectionString") (webprojdir @@ "web.config")
        // //Where logs are stored on the destination server if using the filesystem
        // updateConfigSetting nlogfile "/*[local-name() = 'nlog']/*[local-name() = 'variable']" "value" (environVar "NLog.LogDirectory")
    | _ -> () // don't do anything if running locally

Target "UpdateConfiguration" (fun _ ->
    Common.setAssemblyInfo productName version  
    
    updateSecrets()
)

Target "BuildTestEnvironmentPackage" (fun _ ->    
    Common.setAssemblyInfo productName version  

    let overloads = 
        [             
            "DeployOnBuild", "True"
            "DeployTarget", "Package"
            "PublishProfile", "TestEnvironment.pubxml" 
         ]       
    Common.rebuildDebugWith overloads solutionFile       
)

Target "BuildProductionEnvironmentPackage" (fun _ ->
    Common.setAssemblyInfo productName version  
          
    let overloads = 
        [             
            "DeployOnBuild", "True"
            "DeployTarget", "Package"
            "PublishProfile", "ProductionEnvironment.pubxml"        
         ]
    Common.rebuildReleaseWith overloads solutionFile
)

Target "ExtractTestArtifacts" (fun _ ->
    !! (Common.testDir @@ "*.xml")
    |> CopyFiles (Common.artifactsDir @@ "tests")

    !! (Common.buildDir @@ "_PublishedWebsites" @@ "Template.WebHost_Package" @@ "**/*")
    |> CopyFiles (Common.artifactsDir @@ "WebDeploy" @@ "TestEnvironment")

    TeamCityHelper.PublishArtifact Common.artifactsDir
)

Target "ExtractProductionArtifacts" (fun _ ->
    !! (Common.testDir @@ "*.xml")
    |> CopyFiles (Common.artifactsDir @@ "tests")

    !! (Common.buildDir @@ "_PublishedWebsites" @@ "Template.WebHost_Package" @@ "**/*")
    |> CopyFiles (Common.artifactsDir @@ "WebDeploy" @@ "ProductionEnvironment")

    TeamCityHelper.PublishArtifact Common.artifactsDir
)
// Required to ensure targets from Common module are loaded
Common.init

Target "Common" DoNothing
Target "All" DoNothing
Target "TestEnvironment" DoNothing
Target "ProductionEnvironment" DoNothing
Target "Derp" DoNothing

// Dependencies
"Clean"
//   ==> "RestorePackages"
  ==> "UpdateConfiguration"
//  ==> "BuildTests"
//  ==> "Test"
  ==> "Common"

"TestEnvironment" <== ["Common"; "BuildTests"; "Test"; "BuildTestEnvironmentPackage"; "ExtractTestArtifacts"]

"ProductionEnvironment" <== ["Common"; "BuildProductionEnvironmentPackage"; "ExtractProductionArtifacts"]

// "All" <== ["TestEnvironment"; "ProductionEnvironment"]
"All" <== ["TestEnvironment"]

"Derp" <== ["BuildTests"; "Test"]

    
// Start build
RunTargetOrDefault "All"