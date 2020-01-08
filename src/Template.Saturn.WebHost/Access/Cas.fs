module CAS

open Saturn
open Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open AspNetCore.Security.CAS
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Hosting

//let private addCookie state (c : AuthenticationBuilder) = if not state.CookiesAlreadyAdded then c.AddCookie() |> ignore

type ApplicationBuilder with
    [<CustomOperation("use_cas")>]
    member __.UseCasAuthentication(state: ApplicationState, casCookies : IConfiguration -> System.Action<CookieAuthenticationOptions>, casConfig : IConfiguration -> System.Action<CasOptions>) = //(options :  CasOptions) ) =
        let middleware (app : IApplicationBuilder) =
            app.UseAuthentication()

        let mutable casOptions = None
        let mutable cookies = None

        let handler (nxt : HttpFunc) (ctx : Microsoft.AspNetCore.Http.HttpContext) : HttpFuncResult =
            let ic = ctx.GetService<IConfiguration>()
            casOptions <- Some (casConfig ic)
            cookies <- Some (casCookies ic)
            nxt ctx

        let service (s : IServiceCollection) =
            let c = s.AddAuthentication(fun cfg ->
                cfg.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
                cfg.DefaultChallengeScheme <- "CAS"
                )
            match casOptions with
            | Some o    -> c.AddCAS(o) |> ignore
            | None      -> ()

            match cookies with
            | Some o    -> c.AddCookie(o) |> ignore
            | None      -> ()
            s

        { state with
            ServicesConfig = service::state.ServicesConfig
            AppConfigs = middleware::state.AppConfigs
            CookiesAlreadyAdded = true
        }

    [<CustomOperation("use_cas2")>]
    member __.UseCasAuthentication2(state: ApplicationState, (casConfig: System.Action<WebHostBuilderContext, IServiceCollection>) ) = //, casCookies : IConfiguration -> System.Action<CookieAuthenticationOptions>, casConfig : IConfiguration -> System.Action<CasOptions>) = //(options :  CasOptions) ) =
        let middleware (app : IApplicationBuilder) =
            app.UseAuthentication()

        {state with HostConfigs = (fun (app : IWebHostBuilder)-> app.ConfigureServices(casConfig))::state.HostConfigs}
