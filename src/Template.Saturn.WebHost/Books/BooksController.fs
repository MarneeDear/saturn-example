namespace Books

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn
open Authorization
open Microsoft.Extensions.Logging


module Controller =
  open Giraffe

  let indexAction (ctx : HttpContext) =

    let logger = ctx.GetLogger("Books.indexAction")
    logger.LogError("TEST ME IN BOOKS CONTROLLER")     
    //logger.LogInformation(logger.GetType().ToString())
    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getAll cnf.connectionString
      match result with
      | Ok result ->
        return Views.index ctx (List.ofSeq result)
      | Error ex ->
        return raise ex
    }

  let showAction (ctx: HttpContext) (id : int64) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getById cnf.connectionString id
      match result with
      | Ok (Some result) ->
        return (Views.show ctx result)
      | Ok None ->
        return NotFound.layout
      | Error ex ->
        return raise ex
      //return Controller.text ctx "YOU SEE ME"
    }

  let addAction (ctx: HttpContext) =
    task {
      return Views.add ctx None Map.empty
    }

  let editAction (ctx: HttpContext) (id : int64) =
    let logger = ctx.GetLogger("Books.editAction")
    logger.LogError("EDIT THING")     

    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getById cnf.connectionString id
      match result with
      | Ok (Some result) ->
        return Views.edit ctx result Map.empty
      | Ok None ->
        return NotFound.layout
      | Error ex ->
        return raise ex
      //return Controller.text ctx  "YOU FOUND ME"
    }

  let createAction (ctx: HttpContext) =
    let logger = ctx.GetLogger("Books.createAction")
    logger.LogError("CREATE THING")     

    task {
      let! input = Controller.getModel<Book> ctx
      let validateResult = Validation.validate input
      if validateResult.IsEmpty then

        let cnf = Controller.getConfig ctx
        let! result = Database.insert cnf.connectionString input
        match result with
        | Ok _ ->
          return! Controller.redirect ctx (Helpers.removeTrailingSlash (Links.index ctx))
        | Error ex ->
          return raise ex
      else
        return! Controller.renderHtml ctx (Views.add ctx (Some input) validateResult)
    }

  let updateAction (ctx: HttpContext) (id : int64) =
    let logger = ctx.GetLogger("Books.updateAction")
    logger.LogError("UPDATE THING")     

    task {
      let! input = Controller.getModel<Book> ctx
      let validateResult = Validation.validate input
      if validateResult.IsEmpty then
        let cnf = Controller.getConfig ctx
        let! result = Database.update cnf.connectionString input
        match result with
        | Ok _ ->
          return! Controller.redirect ctx (Helpers.removeTrailingSlash (Links.index ctx))
        | Error ex ->
          return raise ex
      else
        return! Controller.renderHtml ctx (Views.edit ctx input validateResult)
    }

  let deleteAction (ctx: HttpContext) (id : int64) =
    task {
      //let cnf = Controller.getConfig ctx
      //let! result = Database.delete cnf.connectionString id
      //match result with
      //| Ok _ ->
      //  //return! Controller.redirect ctx (Helpers.removeTrailingSlash (Links.index ctx))
      //| Error ex ->
      //  return raise ex
      return Controller.text ctx "YOU DELETED ME!"
    }

  //set_status_code 401 >=> text "Access Denied"

  let resource = controller {
    //index (requires_role "admin" accessDenied) //>=> indexAction
    //index accessDenied
    //plug [Index; Show] (requiresRole "admin" denyAccess)
    //plug [Index] (allowAccessByRoles ["admin"; "staff"; "faculty"])
    //plug [Index] (pipeline { requires_role "admin" accessDenied })
    index indexAction
    show showAction
    add addAction
    edit editAction
    create createAction
    update updateAction
    delete deleteAction
  }

