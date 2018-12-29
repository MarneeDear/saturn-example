module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Thoth.Json

open Shared


open Fulma

open Fable.FontAwesome
open Fable.FontAwesome.Free

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { Counter: Counter option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| Increment
| Decrement
| InitialCountLoaded of Result<Counter, exn>

let initialCounter = fetchAs<Counter> "/api/init" (Decode.Auto.generateDecoder())

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { Counter = None }
    let loadCountCmd =
        Cmd.ofPromise
            initialCounter
            []
            (Ok >> InitialCountLoaded)
            (Error >> InitialCountLoaded)
    initialModel, loadCountCmd



// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.Counter, msg with
    | Some counter, Increment ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value + 1 } }
        nextModel, Cmd.none
    | Some counter, Decrement ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value - 1 } }
        nextModel, Cmd.none
    | _, InitialCountLoaded (Ok initialCount)->
        let nextModel = { Counter = Some initialCount }
        nextModel, Cmd.none

    | _ -> currentModel, Cmd.none


let safeComponents =
    let components =
        span [ ]
           [
             a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io/elmish/" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://mangelmaxime.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://dansup.github.io/bulma-templates/" ] [ str "Bulma\u00A0Templates" ]
           ]

    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]

let show = function
| { Counter = Some counter } -> string counter.Value
| { Counter = None   } -> "Loading..."

let navBrand =
    Navbar.navbar [ Navbar.Color IsWhite ]
        [ Container.container [ ]
            [ Navbar.Brand.div [ ]
                [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ]
                      [ str "SAFE Admin" ] ]
              Navbar.menu [ ]
                  [ Navbar.Start.div [ ]
                      [ Navbar.Item.a [ ]
                            [ str "Home" ]
                        Navbar.Item.a [ ]
                            [ str "Orders" ]
                        Navbar.Item.a [ ]
                            [ str "Payments" ]
                        Navbar.Item.a [ ]
                            [ str "Exceptions" ] ] ] ] ]

let menu =
    Menu.menu [ ]
        [ Menu.label [ ]
              [ str "General" ]
          Menu.list [ ]
              [ Menu.Item.a [ ]
                    [ str "Dashboard" ]
                Menu.Item.a [ ]
                    [ str "Customers" ] ]
          Menu.label [ ]
              [ str "Administration" ]
          Menu.list [ ]
              [ Menu.Item.a [ ]
                  [ str "Team Settings" ]
                li [ ]
                    [ a [ ]
                        [ str "Manage Your Team" ]
                      Menu.list [ ]
                          [ Menu.Item.a [ ]
                                [ str "Members" ]
                            Menu.Item.a [ ]
                                [ str "Plugins" ]
                            Menu.Item.a [ ]
                                [ str "Add a member" ] ] ]
                Menu.Item.a [ ]
                    [ str "Invitations" ]
                Menu.Item.a [ ]
                    [ str "Cloud Storage Environment Settings" ]
                Menu.Item.a [ ]
                    [ str "Authentication" ] ]
          Menu.label [ ]
              [ str "Transactions" ]
          Menu.list [ ]
              [ Menu.Item.a [ ]
                    [ str "Payments" ]
                Menu.Item.a [ ]
                    [ str "Transfers" ]
                Menu.Item.a [ ]
                    [ str "Balance" ] ] ]

let breadcrump =
    Breadcrumb.breadcrumb [ ]
        [ Breadcrumb.item [ ]
              [ a [ ] [ str "Bulma" ] ]
          Breadcrumb.item [ ]
              [ a [ ] [ str "Templates" ] ]
          Breadcrumb.item [ ]
              [ a [ ] [ str "Examples" ] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
              [ a [ ] [ str "Admin" ] ] ]

let hero =
    Hero.hero [ Hero.Color IsInfo
                Hero.CustomClass "welcome" ]
        [ Hero.body [ ]
            [ Container.container [ ]
                [ Heading.h1 [ ]
                      [ str "Hello, Admin." ]
                  safeComponents ] ] ]

let info =
    section [ Class "info-tiles" ]
        [ Tile.ancestor [ Tile.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Tile.parent [ ]
                  [ Tile.child [ ]
                      [ Box.box' [ ]
                          [ Heading.p [ ]
                                [ str "439k" ]
                            Heading.p [ Heading.IsSubtitle ]
                                [ str "Users" ] ] ] ]
              Tile.parent [ ]
                  [ Tile.child [ ]
                      [ Box.box' [ ]
                          [ Heading.p [ ]
                                [ str "59k" ]
                            Heading.p [ Heading.IsSubtitle ]
                                [ str "Products" ] ] ] ]
              Tile.parent [ ]
                  [ Tile.child [ ]
                      [ Box.box' [ ]
                          [ Heading.p [ ]
                                [ str "3.4k" ]
                            Heading.p [ Heading.IsSubtitle ]
                                [ str "Open Orders" ] ] ] ]
              Tile.parent [ ]
                  [ Tile.child [ ]
                      [ Box.box' [ ]
                          [ Heading.p [ ]
                                [ str "19" ]
                            Heading.p [ Heading.IsSubtitle ]
                                [ str "Exceptions" ] ] ] ] ] ]

let counter (model : Model) (dispatch : Msg -> unit) =
    Field.div [ Field.IsGrouped ]
        [ Control.p [ Control.IsExpanded ]
            [ Input.text
                [ Input.Disabled true
                  Input.Value (show model) ] ]
          Control.p [ ]
            [ Button.a
                [ Button.Color IsInfo
                  Button.OnClick (fun _ -> dispatch Increment) ]
                [ str "+" ] ]
          Control.p [ ]
            [ Button.a
                [ Button.Color IsInfo
                  Button.OnClick (fun _ -> dispatch Decrement) ]
                [ str "-" ] ] ]

let columns (model : Model) (dispatch : Msg -> unit) =
    Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is6) ]
            [ Card.card [ CustomClass "events-card" ]
                [ Card.header [ ]
                    [ Card.Header.title [ ]
                        [ str "Events" ]
                      Card.Header.icon [ ]
                          [ Icon.icon [ ]
                              [ Fa.i [ Fa.Solid.AngleDown ] [] ] ] ]
                  div [ Class "card-table" ]
                      [ Content.content [ ]
                          [ Table.table
                              [ Table.IsFullWidth
                                Table.IsStriped ]
                              [ tbody [ ]
                                  [ for _ in 1..10 ->
                                      tr [ ]
                                          [ td [ Style [ Width "5%" ] ]
                                              [ Icon.icon
                                                  [ ]
                                                  [ Fa.i [ Fa.Solid.Bell ] [] ] ]
                                            td [ ]
                                                [ str "Lorem ipsum dolor aire" ]
                                            td [ ]
                                                [ Button.a
                                                    [ Button.Size IsSmall
                                                      Button.Color IsPrimary ]
                                                    [ str "Action" ] ] ] ] ] ] ]
                  Card.footer [ ]
                      [ Card.Footer.div [ ]
                          [ str "View All" ] ] ] ]
          Column.column [ Column.Width (Screen.All, Column.Is6) ]
              [ Card.card [ ]
                  [ Card.header [ ]
                      [ Card.Header.title [ ]
                          [ str "Inventory Search" ]
                        Card.Header.icon [ ]
                            [ Icon.icon [ ]
                                [ Fa.i [Fa.Solid.AngleDown] [] ] ] ]
                    Card.content [ ]
                        [ Content.content [ ]
                            [ Control.div
                                [ Control.HasIconLeft
                                  Control.HasIconRight ]
                                [ Input.text
                                      [ Input.Size IsLarge ]
                                  Icon.icon
                                      [ Icon.Size IsMedium
                                        Icon.IsLeft ]
                                      [ Fa.i [Fa.Solid.Search] [] ]
                                  Icon.icon
                                      [ Icon.Size IsMedium
                                        Icon.IsRight ]
                                      [ Fa.i [Fa.Solid.Check] [] ] ] ] ] ]
                Card.card [ ]
                    [ Card.header [ ]
                        [ Card.Header.title [ ]
                              [ str "Counter" ]
                          Card.Header.icon [ ]
                              [ Icon.icon [ ]
                                  [ Fa.i [Fa.Solid.AngleDown] [] ] ] ]
                      Card.content [ ]
                        [ Content.content   [ ]
                            [ counter model dispatch ] ] ]   ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ]
        [ navBrand
          Container.container [ ]
              [ Columns.columns [ ]
                  [ Column.column [ Column.Width (Screen.All, Column.Is3) ]
                      [ menu ]
                    Column.column [ Column.Width (Screen.All, Column.Is9) ]
                      [ breadcrump
                        hero
                        info
                        columns model dispatch ] ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
