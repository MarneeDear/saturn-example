namespace Books

open Microsoft.AspNetCore.Http
open Giraffe.GiraffeViewEngine
open Saturn

module Views =
  let index (ctx : HttpContext) (objs : Book list) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [rawText "Listing Books"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [rawText "Id"]
              th [] [rawText "Title"]
              th [] [rawText "Author"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [rawText (string o.id)]
                td [] [rawText (string o.title)]
                td [] [rawText (string o.author)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.id )] [rawText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.id )] [rawText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.id ) ] [rawText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [rawText "New Book"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])


  let show (ctx : HttpContext) (o : Book) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [rawText "Show Book"]

        ul [] [
          li [] [ strong [] [rawText "Id: "]; rawText (string o.id) ]
          li [] [ strong [] [rawText "Title: "]; rawText (string o.title) ]
          li [] [ strong [] [rawText "Author: "]; rawText (string o.author) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.id)] [rawText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [rawText "Back"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let private form (ctx: HttpContext) (o: Book option) (validationResult : Map<string, string>) isUpdate =
    let validationMessage =
      div [_class "notification is-danger"] [
        a [_class "delete"; attr "aria-label" "delete"] []
        rawText "Oops, something went wrong! Please check the errors below."
      ]

    let field selector lbl key =
      div [_class "field"] [
        yield label [_class "label"] [rawText (string lbl)]
        yield div [_class "control has-icons-right"] [
          yield input [_class (if validationResult.ContainsKey key then "input is-danger" else "input"); _value (defaultArg (o |> Option.map selector) ""); _name key ; _type "text" ]
          if validationResult.ContainsKey key then
            yield span [_class "icon is-small is-right"] [
              i [_class "fas fa-exclamation-triangle"] []
            ]
        ]
        if validationResult.ContainsKey key then
          yield p [_class "help is-danger"] [rawText validationResult.[key]]
      ]

    let buttons =
      div [_class "field is-grouped"] [
        div [_class "control"] [
          input [_type "submit"; _class "button is-link"; _value "Submit"]
        ]
        div [_class "control"] [
          a [_class "button is-text"; _href (Links.index ctx)] [rawText "Cancel"]
        ]
      ]

    let cnt = [
      div [_class "container "] [
        form [ _action (if isUpdate then Links.withId ctx o.Value.id else Links.index ctx ); _method "post"] [
          if not validationResult.IsEmpty then
            yield validationMessage
          yield field (fun i -> (string i.id)) "Id" "id" 
          yield field (fun i -> (string i.title)) "Title" "title" 
          yield field (fun i -> (string i.author)) "Author" "author" 
          yield buttons
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (o: Book option) (validationResult : Map<string, string>)=
    form ctx o validationResult false

  let edit (ctx: HttpContext) (o: Book) (validationResult : Map<string, string>) =
    form ctx (Some o) validationResult true
