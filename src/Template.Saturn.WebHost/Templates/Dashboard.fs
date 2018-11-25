module Dashboard

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http
open System.Security.Claims


let dashboard (ctx:HttpContext) =
    let name = 
        ctx.User.Claims
        |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) 
        |> Seq.head

    let email =
      ctx.User.Claims
      |> Seq.filter (fun c -> c.Type = "mail")
      |> Seq.head

    let memberOf = 
      ctx.User.Claims
      |> Seq.filter (fun c -> c.Type = ClaimTypes.Role)
      |> Seq.head
    [
        section [_class "hero is-primary"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    p [_class "title"] [rawText (sprintf "Welcome to the dashboard. Your user name is %s. Your email is %s. One of your roles is %s." name.Value email.Value memberOf.Value)]
                ]
            ]
        ]       
    ]

let layout ctx =
    AuthApp.layout (dashboard ctx) ctx