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

type RuntimeConfigSettings = YamlConfig<"config.yaml">
let config = RuntimeConfigSettings()

let debug thing = 
    fun next ctx ->
        Printf.printfn "DEBUG [%s]" thing
        next ctx

let getConfig =
    {
      connectionString = "DataSource=database.sqlite"
      edsUrl = string config.EDS.Url
      webAuthUrl = string config.WebAuth.Url
      edsUserName = config.EDS.UserName
      edsPassword = config.EDS.Password
    }

let endpointPipe = pipeline {
    plug head
    plug requestId
    plug (debug config.EDS.UserName)
    plug (debug config.EDS.Password)
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
    use_config (fun _ -> getConfig ) //TODO: Set development time configuration
    use_iis
    use_cas "https://webauth.arizona.edu/webauth"
    //force_ssl (Nullable<int>(8085))
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code