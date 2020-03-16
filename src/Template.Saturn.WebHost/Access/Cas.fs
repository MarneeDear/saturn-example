module CAS

open Saturn
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open AspNetCore.Security.CAS
open Microsoft.AspNetCore.Http

//let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type Saturn.Application.ApplicationBuilder with
    //Enables CAS authentication
    //Uses https://github.com/IUCrimson/AspNet.Security.CAS
    [<CustomOperation("use_cas")>]
    member __.UseCasAuthentication(state: ApplicationState, casServerUrlBase) =
      let middleware (app : IApplicationBuilder) = app.UseAuthentication()
      
      let resultOptions = new System.Action<CasOptions>(fun o ->
                 o.CasServerUrlBase  <- casServerUrlBase 
                 o.SignInScheme      <- CookieAuthenticationDefaults.AuthenticationScheme
                 //o.CallbackPath      <- new PathString("/dashboard")
                 o.AccessDeniedPath <- new PathString("/error")
                 
             )    
      
      let serviceSetup (s : IServiceCollection) =
          let c = s.AddAuthentication(fun cfg ->
              cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
              cfg.DefaultChallengeScheme <- "CAS"
          )
          c.AddCAS(resultOptions) |> ignore
          s

      { state with
              ServicesConfig      = serviceSetup::state.ServicesConfig
              AppConfigs          = middleware::state.AppConfigs
      }
