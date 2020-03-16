module Server

open System
open Giraffe
open Saturn
open Config
open System.Security.Claims
open Serilog
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Serilog
open System.IO
open System.Threading.Tasks
open CAS

let setupConfiguration (config:IConfiguration) =
    {
        connectionString            = config.["ConnectionStrings:DefaultConnection"]
        edsUrl                      = config.[""]
        webAuthUrl                  = config.["WebAuth:Url"]
        edsUserName                 = config.[""]
        edsPassword                 = config.[""]
        configSettingExample        = config.[""]
        environment                 = config.[""]
        blobStorageConnectionString = config.[""]
        sink                        = config.["Logging:Sink"]
    }

//https://github.com/chriswill/serilog-sinks-azureblobstorage

let configSerilog (builder:ILoggingBuilder) =
    builder.SetMinimumLevel(LogLevel.Trace) |> ignore
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
    options.Cookie.HttpOnly <- true
    //opts.Cookie.Expiration <- Nullable (TimeSpan.FromMinutes(30.0))
    options.ExpireTimeSpan <- TimeSpan.FromMinutes(30.0)
    options.LoginPath <- new PathString("/login")
    options.AccessDeniedPath <- new PathString("/unauthorized")
    options.SlidingExpiration <- true
    

let configureServices (services : IServiceCollection) =   
    services.AddSession(fun opts ->
        opts.IdleTimeout <- TimeSpan.FromMinutes(30.0)
        //opts.Cookie.Expiration <- Nullable (TimeSpan.FromMinutes(30.0))        
    ) |> ignore
    services.AddDataProtection() |> ignore
    services

//let authOptions (opts:CookieAuthenticationOptions) =
//    opts.Cookie.HttpOnly <- true
//    //opts.Cookie.Expiration <- Nullable (TimeSpan.FromMinutes(30.0))
//    opts.ExpireTimeSpan <- TimeSpan.FromMinutes(30.0)
//    opts.LoginPath <- new PathString("/login")
//    opts.AccessDeniedPath <- new PathString("/unauthorized")
//    opts.SlidingExpiration <- true

let endpointPipe = pipeline {
    plug head
    plug requestId
}
let app = application {
    pipe_through endpointPipe
    use_cas "https://webauth.arizona.edu/webauth"
    logging configSerilog
    service_config configureServices
    error_handler (fun ex logger -> 
                                    logger.LogCritical(ex.Message)
                                    pipeline { render_html (InternalError.layout ex) }
                                    )
    use_router Router.appRouter
    url "http://saturn.local:8085/"
    memory_cache
    use_static "static"
    use_gzip
    use_config setupConfiguration 
    use_antiforgery
    use_iis
    use_cookies_authentication_with_config (fun options -> setupCookies options)
    //force_ssl (Nullable<int>(8085))
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code