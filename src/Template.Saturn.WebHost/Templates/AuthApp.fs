module AuthApp

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http
open Giraffe.HttpStatusCodeHandlers

//open Saturn

let layout (content: XmlNode list) (ctx:HttpContext) =
    html [] [ //[_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Hello Saturn COM Template"]
            link [_rel "stylesheet"; _href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
            link [_rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.2/css/bulma.min.css" ]
            link [_rel "stylesheet"; _href "/app.css" ]
            link [_rel "shortcut icon"; _href "/favicon.ico"]
        ]
        body [] [

            yield div [_style "background-color:#0c234b; height:50px; 
            padding-top:10px; padding-bottom:10px;"] [
                div [_class "container uahslogo"] [
                    img [_src "/uahs-banner2x.png"]
                ]
            ]
            yield nav [ _class "navbar has-shadow is-pulled-right" ] [
                div [_class "navbar-brand"] [
                    div [_class "navbar-burger burger"; attr "data-target" "navMenu"] [
                        span [] [] //PUT LOGOUT HERE????
                        span [] []
                        span [] []
                    ]
                ]
                div [_class "navbar-menu"; _id "navMenu"] [
                    div [_class "navbar-start"; _id "navstart"] [
                        a [_class "navbar-item"; _href "/dashboard"; _id "dashboard"] [rawText "Dashboard"]
                        a [_class "navbar-item"; _href "/studentaffairs"; _id "studentaffairs"] [rawText "Student Affairs"]
                        a [_class "navbar-item"; _href "/curricularaffairs"; _id "caffairs"] [rawText "Curricular Affairs"]
                        a [_class "navbar-item"; _href "/blog"; _id "blog"; _target "_blank"] [rawText "My Blog"]
                        a [_class "navbar-item"; _href "/logout"; _id "logout"] [rawText "Logout"]
                    ]
                    div [_class "navbar-end"] []
                ]
            ]
            yield div [_class "container logo"] [
                img [_src "/bannerlogo2x.png"]
            ]

            yield div [_class "container"; _style "min-height:61vh"] [
                yield! content
            ]
            yield 
                div [_style "background-color:#f7f7f7;"] [
                    div [_class "blocka has-text-centered"; _style "height:179px"] [
                        img [_src "/blockangleblue2x.png"; _class "is-fixed-bottom "]
                    ]
                    div [_style "height:50px;background-color:#0c234b;"] []
                ]
            yield script [_src "/app.js"] []
            yield script [_src "https://code.jquery.com/jquery-3.3.1.min.js"] []

        ]
    ]
