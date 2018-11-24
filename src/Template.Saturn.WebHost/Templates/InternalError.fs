module InternalError

open System
open Giraffe.GiraffeViewEngine

let error (ex: Exception) =
    [
        section [_class "container"] [
            h1 [] [rawText "ERROR #500"]
            h3 [] [rawText ex.Message]
            h4 [] [rawText ex.Source]
            p [] [rawText ex.StackTrace]
            a [_href "/dashboard" ] [rawText "Go back to home page"]

        ]
    ]

let layout ex =
    App.layout (error ex)