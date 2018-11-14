module Login

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http


let login (ctx:HttpContext) =
    [
        section [_class "hero"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    p [_class "title"] [rawText "You need to be logged in to view this page. Click the button over there."]
                ]
            ]
        ]       
    ]

let layout ctx =
    App.layout (login ctx) ctx