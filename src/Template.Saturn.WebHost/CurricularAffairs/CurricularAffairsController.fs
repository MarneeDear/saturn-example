namespace CurricularAffairs

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn

module Controller =

    let indexAction (ctx : HttpContext) =
        task {
            return Views.index ctx
        }

    let resource = controller {
        index indexAction
    }