module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open Books
open Login
open Logout
open Dashboard
open Giraffe
open System.Security.Claims
open Microsoft.AspNetCore.Http


//let logout = pipeline {
//    plug 
//}

let login = pipeline {
    requires_authentication (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    //(Giraffe.Auth.challenge "CAS")
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


let loggedInView = router {
    pipe_through login
    forward "/books" Books.Controller.resource 
    forward "/dashboard" (fun next ctx -> htmlView (Dashboard.layout ctx) next ctx)
}

let isAuthenticated (ctx:HttpContext) =
    if ctx.User.Identity.IsAuthenticated then 
        (redirectTo false "/dashboard")
    else
        (Giraffe.Auth.challenge "CAS")

let browserRouter = router {
    not_found_handler (htmlView NotFound.layout) //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline
    forward "" defaultView //Use the default view
    get "/books" loggedInView
    get "/login" (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    get "/logout" (signOut "Cookies" >=> (fun next ctx -> htmlView (Logout.layout ctx) next ctx)) 
    get "/dashboard" loggedInView 
    get "/webauth" (fun next ctx -> (isAuthenticated ctx) next ctx) 
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