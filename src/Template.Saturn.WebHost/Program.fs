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

//type ConfigSettings = AppSettings<"web.config">
type ConfigSettings = YamlConfig<"config.yaml">
let config = ConfigSettings()


let endpointPipe = pipeline {
    plug head
    plug requestId
}

let getConfig =
    {
      connectionString = "DataSource=database.sqlite"
      edsUrl = config.EDS.Url
      webAuthUrl = config.WebAuth.Url
      edsUserName = config.EDS.UserName
      edsPassword = config.EDS.Password

    }

let app = application {
    pipe_through endpointPipe
    logging (fun (builder: ILoggingBuilder) -> builder.SetMinimumLevel(LogLevel.Trace) |> ignore)
    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
    use_router Router.appRouter
    url "http://saturn.local:8085/"
    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> getConfig )
    use_iis
    use_cas (config.WebAuth.Url) (config.EDS.Url) config.EDS.UserName config.EDS.Password
    //consider using force_ssl even in developement. find out about setting up certs
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code