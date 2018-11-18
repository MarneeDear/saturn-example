module CAS

open Saturn
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open AspNetCore.Security.CAS


let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type ApplicationBuilder with
    //Enables CAS authentication
    //Uses https://github.com/IUCrimson/AspNet.Security.CAS
    [<CustomOperation("use_cas_with_options")>]
    member __.UseCasAuthenitcation(state: ApplicationState, casServerUrlBase) =
      let middleware (app : IApplicationBuilder) =
        app.UseAuthentication()               

      let service (s : IServiceCollection) =
        let c = s.AddAuthentication(fun cfg ->
          cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultChallengeScheme <- "CAS"
          )
        addCookie state c
        c.AddCAS(fun o -> 
            o.CasServerUrlBase <- casServerUrlBase
            o.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            )
        |> ignore
        s

      { state with
          ServicesConfig = service::state.ServicesConfig
          AppConfigs = middleware::state.AppConfigs
          CookiesAlreadyAdded = true


      }
