module Login

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http


let login (ctx:HttpContext) =
    [
        section [_class "hero is-light"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    p [_class "title"] [rawText "You might put a login page here. Maybe."]
                ]
            ]
        ]       
    ]

let layout ctx =
    App.layout (login ctx) ctx