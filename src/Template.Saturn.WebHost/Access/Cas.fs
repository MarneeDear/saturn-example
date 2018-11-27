module CAS

open Saturn
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open AspNetCore.Security.CAS
open System.Security.Claims
open System.Threading.Tasks
open COM.Core
open COM.Infrastructure
open COM.Core.Models


let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type ApplicationBuilder with
    //Enables CAS authentication
    //Uses https://github.com/IUCrimson/AspNet.Security.CAS
    [<CustomOperation("use_cas")>]
    member __.UseCasAuthentication(state: ApplicationState, casServerUrlBase, edsServerUrl, edsUserName, edsPassword) =
      let middleware (app : IApplicationBuilder) =
        app.UseAuthentication() 
        
      let edsService =
        new DirectoryServices.EdsClient(edsServerUrl, edsUserName,
            edsPassword) :> Interfaces.IEdsClient

      let getClaims (attributes: UserAttribute list) =
        let identity = new ClaimsIdentity()
        for attr in attributes do 
          for value in attr.Values do
            if attr.Name = "isMemberOf" then
              identity.AddClaim(new Claim(ClaimTypes.Role, value))
            else
              identity.AddClaim(new Claim(attr.Name, value))
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"))
        identity

      let cookieEvents = new CookieAuthenticationEvents()
      cookieEvents.OnSigningIn <- (fun ctx -> 
            let userInfo = edsService.GetUserInfo(ctx.Principal.Identity.Name)
            ctx.Principal.AddIdentity(getClaims(userInfo.Attributes))
            Task.CompletedTask)

      let service (s : IServiceCollection) =
        let c = s.AddAuthentication(fun cfg ->
          cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultChallengeScheme <- "CAS"
          )
        c.AddCookie(fun o -> o.Events <- cookieEvents) |> ignore
        //addCookie state c
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
