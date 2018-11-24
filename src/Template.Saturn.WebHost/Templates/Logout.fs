module Logout

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http


let logout (ctx:HttpContext) =
    [
        section [_class "hero is-danger"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    p [_class "title"] [rawText "You might put a logout of webauth message here. "]
                ]
            ]
        ]       
    ]

let layout ctx =
    App.layout (logout ctx)