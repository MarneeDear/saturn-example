module App

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http

let layout (content: XmlNode list) (ctx:HttpContext) =
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

            yield div [_style "background-color:#0c234b"] [
                div [_class "container uahslogo"] [
                    img [_src "/uahs-banner2x.png"]
                ]
            ]
            yield nav [ _class "navbar has-shadow" ] [
                div [_class "navbar-brand"] [
                    //a [_class "navbar-item"; _href "/"] [
                    //    img [_src "https://avatars0.githubusercontent.com/u/35305523?s=200"; _width "28"; _height "28"]
                    //]
                    div [_class "navbar-burger burger"; attr "data-target" "navMenu"] [
                        span [] []
                        span [] []
                        span [] []
                    ]
                ]
            ]
            yield div [_class "container logo"] [
                img [_src "/bannerlogo2x.png"]
            ]

            yield! content
            //yield footer [_class "footer is-fixed-bottom"] [
            //    div [_class "container"] [
            //        div [_class "content has-text-centered"] [
            //            p [] [
            //                rawText "Powered by "
            //                a [_href "https://github.com/SaturnFramework/Saturn"] [rawText "Saturn"]
            //                rawText " - F# MVC framework created by "
            //                a [_href "http://lambdafactory.io"] [rawText "λFactory"]
            //            ]
            //        ]
            //    ]
            //]
            yield //footer [_class "footer"] [
                div [_style "background-color:#f7f7f7; height:179px"] [
                    div [_class "blocka has-text-centered"] [
                        img [_src "/blockangleblue2x.png"; _class "is-fixed-bottom"]
                    ]
                //]
            ]
            yield div [_style "height:50px;background-color:#0c234b;"] []
        ]
    ]
