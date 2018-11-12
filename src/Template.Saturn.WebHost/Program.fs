module Server

open System
open Giraffe
open Saturn
open Config
//open Microsoft.AspNetCore.Authentication.OAuth
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.OAuth
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json.Linq
open CAS
open Microsoft.AspNetCore.Builder
open System.Security.Claims

let endpointPipe = pipeline {
    plug head
    plug requestId
}


let newCasOptions = new CasOptions()
newCasOptions.CasServerUrlBase <- "https://webauth.arizona.edu/webauth"

    

//let login = pipeline {
//    requires_authentication (Giraffe.Auth.challenge "CAS")
//}

//let logged_in_view = router {
//    pipe_through login

//    get "/google" (fun next ctx -> task {
//        let name = ctx.User.Claims |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) |> Seq.head
//        return! json { user_name = name.Value } next ctx
//    })
//}

//let setCustomOAuthOptions (opts:OAuthOptions) = 
//    opts.AuthorizationEndpoint <- "https://webauth.arizona.edu/webauth"
//    opts.CallbackPath <- new PathString("/")
//    opts.Events <- new OAuthEvents()
//    opts.ClientId <- "WebAuth"
//    opts.ClientSecret <- "TEST"
//    opts.Events.OnCreatingTicket <- 
//        fun ctx -> 
//            let tsk = task {
//              let req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint)
//              req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
//              req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", ctx.AccessToken)
//              let! (response : HttpResponseMessage) = ctx.Backchannel.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted)
//              response.EnsureSuccessStatusCode () |> ignore
//              let! cnt = response.Content.ReadAsStringAsync()
//              let user = JObject.Parse cnt
//              ctx.RunClaimActions user
//            }
//            Task.Factory.StartNew(fun () -> tsk.Result)

    //https://github.com/SaturnFramework/Saturn/blob/c32044a41a540aa82fe2d5ecac15e683716eec07/src/Saturn.Extensions.Authorization/OAuth.fs#L25
    
    (*
    
    those are always painful for me. I usually end up having to define them in pure F#, and then when setting the property on the options object wrap my function in `System.Func<_,_,_.....>`, or `System.Action<_,_,...>` with more `_` for each parameter it takes
    eg `let app = app.Use(Func<_,_,_> applyForwardingHeaders)`
    *)

let app = application {
    pipe_through endpointPipe

    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
    use_router Router.appRouter
    url "http://saturn.local:8085/"
    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> {connectionString = "DataSource=database.sqlite"} ) //TODO: Set development time configuration
    use_iis
    //use_custom_oauth "Web Auth" (fun opts -> setWebAuthOptions opts )
    use_cas_with_options newCasOptions
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code