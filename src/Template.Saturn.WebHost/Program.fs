module Server

open System
open Giraffe
open Saturn
open Config
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Security.Claims
open Serilog
open Serilog.Sinks
open Serilog.Events
open Microsoft.WindowsAzure.Storage
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Serilog
open System.IO
open Microsoft.AspNetCore.Authentication.Cookies
open System.Threading.Tasks
open CAS

let setupConfiguration (config:IConfiguration) =
    {
        connectionString = config.["ConnectionStrings:DefaultConnection"]
        edsUrl = config.[""]
        webAuthUrl = config.[""]
        edsUserName = config.[""]
        edsPassword = config.[""]
        configSettingExample = config.[""]
        environment = config.[""]
        blobStorageConnectionString = config.[""]
        sink = config.[""]
    }

//https://github.com/chriswill/serilog-sinks-azureblobstorage

let configSerilog (builder:ILoggingBuilder) =
    builder.SetMinimumLevel(LogLevel.Information) |> ignore
    let config = builder.Services.BuildServiceProvider().GetService<IConfiguration>()
    let loggingConfig = new LoggerConfiguration()
    loggingConfig.Enrich.FromLogContext() |> ignore
    match config.["Logging:Sink"] with
    //| "File" -> loggingConfig.WriteTo.File("D:\\log.log", rollingInterval = RollingInterval.Day) |> ignore
    | "Console" -> loggingConfig.WriteTo.Console() |> ignore
    | _ -> loggingConfig.WriteTo.Console() |> ignore
    builder.AddSerilog(loggingConfig.CreateLogger()) |> ignore

let setupCookies (options:CookieAuthenticationOptions) = 
    let cookieEvents = new CookieAuthenticationEvents()
    cookieEvents.OnSigningIn <- (fun ctx -> 
            let identity = new ClaimsIdentity()
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"))
            identity.AddClaim(new Claim(ClaimTypes.Role, "pcoord"))
            ctx.Principal.AddIdentity(identity)
            Task.CompletedTask
            )
    options.Events <- cookieEvents

let endpointPipe = pipeline {
    plug head
    plug requestId
}
let app = application {
    pipe_through endpointPipe
    logging configSerilog
    error_handler (fun ex logger -> 
                                    logger.LogCritical(ex.Message)
                                    pipeline { render_html (InternalError.layout ex) }
                                    )
    use_router Router.appRouter
    url "https://localhost/"
    memory_cache
    use_static "static"
    use_gzip
    use_config setupConfiguration 
    use_iis
    use_cas "https://webauth.arizona.edu/webauth"
    use_cookies_authentication_with_config (fun options -> setupCookies options)
    //force_ssl (Nullable<int>(8085))
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code