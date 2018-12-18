namespace CurricularAffairs

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn
open Giraffe
open Microsoft.Extensions.Logging


module Controller =

    let indexAction (ctx : HttpContext) =
        let logger = ctx.GetLogger("CurricularAffairs.index")
        logger.LogInformation("EXAMPLE OF HOW TO USE LOGGING")
        task {
            //EXAMPLE OF HOW TO USE CONFIG
            let cnf = Controller.getConfig ctx
            let conStr = cnf.connectionString
            let configSettingExample = cnf.configSettingExample            
            return Views.index ctx { id = 10000L; gradYear = GradYear.Y2018; name = "Nervous System"; catalogNumber = 999}
        }

    let resource = controller {
        index indexAction
    }