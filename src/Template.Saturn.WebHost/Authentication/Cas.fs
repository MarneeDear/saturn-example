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
          )
        |> ignore
        s

      { state with
          ServicesConfig = service::state.ServicesConfig
          AppConfigs = middleware::state.AppConfigs
          CookiesAlreadyAdded = true
      }
