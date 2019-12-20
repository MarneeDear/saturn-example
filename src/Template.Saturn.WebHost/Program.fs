module Server

open System
open Giraffe
open Saturn
open Config
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.OAuth
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json.Linq
open CAS
open Microsoft.AspNetCore.Builder
open System.Security.Claims
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.Cookies
open Serilog
open Serilog.Sinks
open Microsoft.Extensions.Configuration
open Serilog.Events
open Microsoft.WindowsAzure.Storage
open Microsoft.Extensions.DependencyInjection
open COM.Core
open COM.Infrastructure
open COM.Core.Models

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let setupConfiguration (config:IConfiguration) =
    {
        connectionString = config.["ConnectionStrings:DefaultConnection"]
        edsUrl = string config.["EDS:Url"]
        webAuthUrl = string config.["WebAuth:Url"]
        edsUserName = config.["EDS:UserName"]
        edsPassword = config.["EDS:Password"]
        configSettingExample = config.["General:ConfigSettingExample"]
        environment = config.["Environment"]
        blobStorageConnectionString = config.["Azure:BlobStorageConnectionString"]
    }

    (*
    https://github.com/chriswill/serilog-sinks-azureblobstorage    
    *)
let configSerilog (builder:ILoggingBuilder) =
    builder.SetMinimumLevel(LogLevel.Trace) |> ignore
    let config = builder.Services.BuildServiceProvider().GetService<IConfiguration>()
    let loggingConfig = new LoggerConfiguration()
    loggingConfig.MinimumLevel.Information |> ignore
    loggingConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Information) |> ignore
    loggingConfig.Enrich.FromLogContext() |> ignore


    let findConfig sink =
        match sink with
        //| "RollingFile" -> loggingConfig.WriteTo.RollingFile("D:\logs\log-{Date}.txt") |> ignore//.CreateLogger();
        //| "File" -> loggingConfig.WriteTo.File("D:\\log.log", rollingInterval = RollingInterval.Day) |> ignore
        | "Console" -> loggingConfig.WriteTo.Console() |> ignore
        | "AzureBlobStorage" -> loggingConfig.WriteTo.AzureBlobStorage(config.["Azure.BlobStorageConnectionString"]) |> ignore
        //| _ -> failwith (sprintf "[ERROR] Unknown logging sink. You must configure a known logging sink. Sink configured is [%s]" config.Logging.Sink)
        | _ -> loggingConfig.WriteTo.Console() |> ignore


    [
    config.["Logging:Sinks:0"]
    //config.["Logging:Sinks:1"]
    //config.["Logging:Sinks:2"]        
    ] |> List.iter findConfig

    builder.AddSerilog(loggingConfig.CreateLogger()) |> ignore

let setupCASConfig (config:IConfiguration) =
    //let config = builder.Services.BuildServiceProvider().GetService<IConfiguration>()
    //config.
    //(config.["WebAuth.Url"], config.["EDS.Url"], config.["EDS.UserName"], config.["EDS.Password"])
    let resultOptions = new Action<CasOptions>(fun o ->
        o.CasServerUrlBase <- config.["EDS.Url"]
        o.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
    )
    //let options = new CasOptions()
    //options.CasServerUrlBase <- config.["EDS.Url"]
    //options.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
    //options
    resultOptions

let setupCookies (config:IConfiguration) =
    let edsService =
        new DirectoryServices.EdsClient(config.["WebAuth.Url"],  config.["EDS.UserName"],
            config.["EDS.Password"]) :> Interfaces.IEdsClient

    let getClaims (attributes: UserAttribute list) =
        let identity = new ClaimsIdentity()
        for attr in attributes do 
            for value in attr.Values do
                if attr.Name = "isMemberOf" then
                    identity.AddClaim(new Claim(ClaimTypes.Role, value))
                else
                    identity.AddClaim(new Claim(attr.Name, value))              
        identity.AddClaim(new Claim(ClaimTypes.Role, "admin")) //TODO: you don't want this in production. See TODO above
        identity
    
    let cookieEvents = new CookieAuthenticationEvents()
    cookieEvents.OnSigningIn <- (fun ctx -> 
          let userInfo = edsService.GetUserInfo(ctx.Principal.Identity.Name)
          ctx.Principal.AddIdentity(getClaims(userInfo.Attributes))
          Task.CompletedTask)

    let cookie = new Action<CookieAuthenticationOptions>(fun o ->
        o.Events <- cookieEvents
    )
    cookie

let app =    
    application {
        pipe_through endpointPipe
        logging configSerilog
        error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
        use_router Router.appRouter
        url "http://saturn.local:8085/"
        memory_cache
        use_static "static"
        use_gzip
        use_config (fun _ -> setupConfiguration)
        use_iis
        use_cas setupCookies setupCASConfig
        use_antiforgery
        //force_ssl
        //consider using force_ssl even in development. find out about setting up the port to handle this
    }

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code