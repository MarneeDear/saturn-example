module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open Books
open Login
open Giraffe
open System.Security.Claims

let login = pipeline {
    requires_authentication (Giraffe.Auth.challenge "CAS")
    //plug print_user_details
}

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (fun next ctx -> htmlView (Index.layout ctx) next ctx)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")

}

type UserCredentialsResponse = { user_name : string }  


let loginRouter = router {
    pipe_through login
    get "" (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    //get "" (fun next ctx -> task {
    //    let name = ctx.User.Claims |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) |> Seq.head
    //    return! json { user_name = name.Value } next ctx
    //})
}


let browserRouter = router {
    not_found_handler (htmlView NotFound.layout) //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline

    forward "" defaultView //Use the default view
    forward "/books" Books.Controller.resource 
    forward "/login" loginRouter
}


//Other scopes may use different pipelines and error handlers

// let api = pipeline {
//     plug acceptJson
//     set_header "x-pipeline-type" "Api"
// }

// let apiRouter = router {
//     error_handler (text "Api 404")
//     pipe_through api
//
//     forward "/someApi" someScopeOrController
// }

let appRouter = router {
    // forward "/api" apiRouter
    forward "" browserRouter
}