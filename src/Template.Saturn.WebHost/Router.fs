module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open Books
open Login
open Logout
open Dashboard
open CurricularAffairs
open Giraffe
open System.Security.Claims
open Microsoft.AspNetCore.Http

let login = pipeline {
    requires_authentication (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    //plug print_user_details
}

let notFound = pipeline { 
    set_status_code 401
    render_html NotFound.layout
}//Use the default 404 webpage

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (htmlView Index.layout)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
}

type UserCredentialsResponse = { user_name : string }  

let loggedInView = router {
    pipe_through protectFromForgery
    pipe_through login
    //get "/books" Books.Controller.resource
    //getf "/books/add" (fun s -> Books.Controller.resource)
    //deletef "/books/%s" (fun (x : string) -> Books.Controller.resource)
    //getf "/books/%s" (fun (x : string) -> Books.Controller.resource)
    //post "/books" Books.Controller.resource

    forward "/books" Books.Controller.resource 
    forwardf "/books/%s" (fun (x : string) -> Books.Controller.resource)    
    forward "/dashboard" (fun next ctx -> htmlView (Dashboard.layout ctx) next ctx)
    forward "/CurricularAffairs" CurricularAffairs.Controller.resource
}

let isAuthenticated (ctx:HttpContext) =
    if ctx.User.Identity.IsAuthenticated then 
        (redirectTo false "/dashboard")
    else
        (Giraffe.Auth.challenge "CAS")

let browserRouter = router {
    not_found_handler notFound
    pipe_through browser //Use the default browser pipeline
    forward "" defaultView //Use the default view
    get "/CurricularAffairs" loggedInView
    //forward "/books" loggedInView
    //forwardf "/books/%s" (fun (x : string) -> loggedInView)    

    get "/books" loggedInView
    getf "/books/add" (fun _ -> loggedInView)
    deletef "/books/%s" (fun (_ : string) -> Books.Controller.resource)
    getf "/books/%s" (fun (_ : string) -> loggedInView)
    post "/books" loggedInView
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