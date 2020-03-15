module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
//open Books
open Login
open Logout
open Dashboard
open Giraffe
open System.Security.Claims
open Microsoft.AspNetCore.Http


let login = pipeline {
    requires_authentication (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    //plug print_user_details
}

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
    pipe_through login
    pipe_through protectFromForgery
    forward "/CurricularAffairs" CurricularAffairs.Controller.resource
    //forwardf "/CurricularAffairs/%s" (fun (_ : int) -> CurricularAffairs.Controller.resource)
    forward "/dashboard" (fun next ctx -> htmlView (Dashboard.layout ctx) next ctx)
}

let getIntExample (id : string) = text (sprintf "YOU GOT ME %s" id)

let isAuthenticated (ctx:HttpContext) =
    if ctx.User.Identity.IsAuthenticated then 
        (redirectTo false "/dashboard")
    else
        (Giraffe.Auth.challenge "CAS")

let browserRouter = router {
    not_found_handler (htmlView NotFound.layout) //Use the default 404 webpage
    //pipe_through browser //Use the default browser pipeline
    forward "" defaultView //Use the default view
    get "/CurricularAffairs" loggedInView
    //deletef "/CurricularAffairs/%s" (fun (_ : string) -> loggedInView)
    //THIS IS 404ing
    deletef "/CurricularAffairs/%s" (fun (id : string) -> CurricularAffairs.Controller.resource)
    //deletef "/CurricularAffairs/%i" getIntExample
    get "/login" (fun next ctx -> htmlView (Login.layout ctx) next ctx)
    get "/logout" (signOut "Cookies" >=> (fun next ctx -> htmlView (Logout.layout ctx) next ctx)) 
    get "/dashboard" loggedInView 
    get "/webauth" (fun next ctx -> (isAuthenticated ctx) next ctx) 
    get "/blog" (redirectTo false "https://steemit.com/@marnee")

    //THESE BOOKS ROUTES ALL WORK TOGETHER
    //get "/books" loggedInView
    //get "/books/add" loggedInView
    //getf "/books/%d" (fun (_ : int64) -> loggedInView)
    //getf "/books/%d/edit" (fun (s:int64) -> loggedInView)
    //post "/books" loggedInView
    //postf "/books/%d" (fun (_:int64) -> loggedInView)
    //END WORKING ROUTES


    //DONT WORK SO GOOD 
    //404 but why?
    //deletef "/delete/%d" getIntExample //loggedInView)
    //delete "/delete" (text "DELETE ME??@")

    //UNKNOWN
    //patch "/books" loggedInView
    //put "/books" loggedInView   

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