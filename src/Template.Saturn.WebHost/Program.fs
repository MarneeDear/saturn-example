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
open FSharp.Configuration
open Serilog
open NLog.Common
open Serilog
open Microsoft.Extensions.Configuration
open Serilog.Events

//type ConfigSettings = AppSettings<"web.config">

type RuntimeConfigSettings = YamlConfig<"config.yaml">
let config = RuntimeConfigSettings()


let endpointPipe = pipeline {
    plug head
    plug requestId
    //plug (print_something config.EDS.UserName)
    //plug (print_something config.EDS.Password)
}

let getConfig =
    {
      connectionString = "DataSource=database.sqlite"
      edsUrl = string config.EDS.Url
      webAuthUrl = string config.WebAuth.Url
      edsUserName = config.EDS.UserName
      edsPassword = config.EDS.Password
    }

let loggingConfig = new LoggerConfiguration()
loggingConfig.MinimumLevel.Debug() |> ignore
loggingConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Information) |> ignore
loggingConfig.Enrich.FromLogContext() |> ignore
loggingConfig.WriteTo.Console() |> ignore
//loggingConfig.CreateLogger()

//Log.Logger <- loggingConfig.CreateLogger()

//new LoggerConfiguration().

let app = application {
    pipe_through endpointPipe
    logging (fun (builder: ILoggingBuilder) -> 
                    builder.SetMinimumLevel(LogLevel.Trace) |> ignore
                    builder.AddSerilog(loggingConfig.CreateLogger()) |> ignore
                    )
    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
    use_router Router.appRouter
    url "http://saturn.local:8085/"
    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> getConfig )
    use_iis
    use_cas (string config.WebAuth.Url) (string config.EDS.Url) config.EDS.UserName config.EDS.Password
    //force_ssl
    //consider using force_ssl even in development. find out about setting up the port to handle this
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    //Log.Information("HELLO SERILOG")
    0 // return an integer exit code