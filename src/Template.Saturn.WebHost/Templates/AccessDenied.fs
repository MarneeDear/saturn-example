module AccessDenied

open Giraffe.GiraffeViewEngine

let accessDenied =
    [
        section [_class "container"] [
            h1 [] [rawText "ACCESS DENIED"]
            a [_href "/dashboard" ] [rawText "Go back to home page"]            
        ]
    ]

let layout =
    App.layout accessDenied