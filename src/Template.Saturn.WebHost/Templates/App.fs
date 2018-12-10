module App

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http
open Giraffe.HttpStatusCodeHandlers

//open Saturn

let layout (content: XmlNode list) =
    html [] [ //[_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Hello Saturn COM Template"]
            link [_rel "stylesheet"; _href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
            link [_rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.6.1/css/bulma.min.css" ]
            link [_rel "stylesheet"; _href "/app.css" ]
            link [_rel "shortcut icon"; _href "/favicon.ico"]
        ]
        body [] [

            yield div [_style "background-color:#0c234b; height:50px; padding-top:10px; padding-bottom:10px;"] [
                div [_class "container uahslogo"] [
                    img [_src "/uahs-banner2x.png"]
                ]
            ]
            //LOGIN HERE
            yield nav [ _class "navbar is-pulled-right"; _style "padding:10px 10px 10px 10px;" ] [
                div [_class "container"] [
                    a [_class "button is-red"; _href "/webauth"] [rawText "Login"]
                ]
            ]
            yield div [_class "container logo"] [
                img [_src "/bannerlogo2x.png"]
            ]

            yield div [_class "container"; _style "min-height:61vh"] [
                yield! content
            ]
            yield hr []
            yield 
                div [_style "background-color:#f7f7f7;"] [
                    div [_class "blocka has-text-centered"; _style "height:179px"] [
                        img [_src "/blockangleblue2x.png"; _class "is-fixed-bottom "]
                    ]
                    div [_style "height:50px;background-color:#0c234b;"] []
                ]
            yield script [_src "/app.js"] []

        ]
    ]
