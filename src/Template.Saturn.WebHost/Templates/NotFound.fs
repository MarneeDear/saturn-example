module NotFound

open Giraffe.GiraffeViewEngine

let notFound =
    [
        section [_class "container"] [
            h1 [] [rawText "ERROR #404"]
            a [_href "/dashboard" ] [rawText "Go back to home page"]            
        ]
    ]

let layout =
    App.layout notFound