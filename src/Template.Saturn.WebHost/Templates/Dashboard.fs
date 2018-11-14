module Dashboard

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http
open System.Security.Claims


let dahsboard (ctx:HttpContext) =
    let name = 
        ctx.User.Claims
        |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) 
        |> Seq.head
    [
        section [_class "hero is-primary"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    p [_class "title"] [rawText (sprintf "Welcome to the dashboard. Your user name is %s."name.Value)]
                ]
            ]
        ]       
    ]

let layout ctx =
    App.layout (dahsboard ctx) ctx