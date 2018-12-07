module Authorization

open Saturn
open Giraffe

let denyAccess = pipeline {
  set_status_code 401
  text "Access Denied!!!!"

}

let onlyAllowAdmin = pipeline {
  requires_role "admin"  denyAccess
}

let allowAccessByRoles roles = pipeline {
  requires_role_of roles denyAccess 
}


