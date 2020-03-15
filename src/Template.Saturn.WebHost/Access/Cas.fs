module CAS

open Saturn
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open AspNetCore.Security.CAS
open System.Security.Claims
open System.Threading.Tasks

//let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type Saturn.Application.ApplicationBuilder with
    //Enables CAS authentication
    //Uses https://github.com/IUCrimson/AspNet.Security.CAS
    [<CustomOperation("use_cas")>]
    member __.UseCasAuthentication(state: ApplicationState, casServerUrlBase) =
      let middleware (app : IApplicationBuilder) =
        app.UseAuthentication() 
      
      //TODO: remove and replace with your roles, policies, claims, or resource assignment handlers
      //https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-2.1
      //let getClaims (attributes: UserAttribute list) =
      //  let identity = new ClaimsIdentity()
      //  for attr in attributes do 
      //    for value in attr.Values do
      //      if attr.Name = "isMemberOf" then
      //        identity.AddClaim(new Claim(ClaimTypes.Role, value))
      //      else
      //        identity.AddClaim(new Claim(attr.Name, value))              
      //  identity.AddClaim(new Claim(ClaimTypes.Role, "admin")) //TODO: you don't want this in production. See TODO above
      //  identity

      //let cookieEvents = new CookieAuthenticationEvents()
      //cookieEvents.OnSigningIn <- (fun ctx -> 
      //      //let userInfo = edsService.GetUserInfo(ctx.Principal.Identity.Name)
      //      //ctx.Principal.AddIdentity(getClaims(userInfo.Attributes))
      //      //Task.CompletedTask
      //      let identity = new ClaimsIdentity()
      //      identity.AddClaim(new Claim(ClaimTypes.Role, "admin"))
      //      identity.AddClaim(new Claim(ClaimTypes.Role, "pcoord"))
      //      ctx.Principal.AddIdentity(identity)
      //      Task.CompletedTask
      //      )
      let service (s : IServiceCollection) =
        let c = s.AddAuthentication(fun cfg ->
          cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          cfg.DefaultChallengeScheme <- "CAS"
          )
        //c.AddCookie(fun o -> o.Events <- cookieEvents) |> ignore
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
          //CookiesAlreadyAdded = true
      }
