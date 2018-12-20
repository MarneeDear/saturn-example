module Authorization

open Saturn
open Giraffe
open AccessDenied
open Saturn.CSRF

//TODO These types may not fit your needs. Change the code to suit your domain
type Resource =
    | CurricularAffairs
    | StudentAffairs
    | Dashboard
    | Guide

type Access =
    | View
    | Create
    | Update
    | Delete

type Roles = 
    | Admin
    | ProgramCoordinator
    | Dean

let allRoles =
    ["admin";"pccord";"dean"]

let accessDenied = pipeline {
  set_status_code 401
  //text "Access Denied!!!!"
  render_html AccessDenied.layout
}

let onlyAllowAdmin = pipeline {
  requires_role "admin"  accessDenied
}

let allowAccessByRoles roles = pipeline {
  requires_role_of roles accessDenied 
}

//TODO add logic to get authorized roles per resource
//YOU may want a database like MedReports uses
//IF you have a complex domain with many resource and access combinations, you should use a database here
//Use FSharp.Data.SqlClient http://fsprojects.github.io/FSharp.Data.SqlClient/index.html
//You could also refactor to use policy based authorizations
//https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-2.2
//YOU might also consider capability based security described by Scott Wlaschin https://fsharpforfunandprofit.com/posts/capability-based-security/
let getAuthorizedRoles resource access =
    match resource, access with
    | CurricularAffairs, View -> ["admin";"pcoord"]
    | StudentAffairs, View -> ["admin";"dean"]
    | Dashboard, View -> allRoles
    | Guide, View -> allRoles
    | _ -> []
