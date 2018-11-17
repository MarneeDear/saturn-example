module CAS

open Saturn
open Giraffe
open Microsoft.AspNetCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open System
open Microsoft.AspNetCore.Authentication
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json.Linq
open AspNetCore.Security.CAS


let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type ApplicationBuilder with
    //Enables CAS authentication
    //Uses https://github.com/IUCrimson/AspNet.Security.CAS
    [<CustomOperation("use_cas_with_options")>]
    member __.UseCasAuthenitcation(state: ApplicationState, (casOptions : CasOptions) ) = //, (cookieOptions: CookieAuthenticationOptions) ) =
      let middleware (app : IApplicationBuilder) =
        app.UseAuthentication()

      let service (s : IServiceCollection) =
        let c = s.AddAuthentication(fun cfg ->
          cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultChallengeScheme <- "CAS")
        addCookie state c
        //c.AddCookie( fun o -> o.LoginPath <- new PathString("/login")) |> ignore
        //c.AddCookie(fun o -> o.AccessDeniedPath <- new PathString("/access-denied")) |> ignore
        //c.AddCookie( fun o -> o.Events <- new CookieAuthenticationEvents() )
        c.AddCAS(fun (opt : CasOptions) ->
          opt.CasServerUrlBase <- casOptions.CasServerUrlBase
          opt.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          //opt.CallbackPath <- new PathString("/dashboard")
          opt.Events = new CookieAuthenticationEvents 
          )
        |> ignore
        s

      { state with
          ServicesConfig = service::state.ServicesConfig
          AppConfigs = middleware::state.AppConfigs
          CookiesAlreadyAdded = true


          //EXAMPLE USING OAUTH
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

      }
