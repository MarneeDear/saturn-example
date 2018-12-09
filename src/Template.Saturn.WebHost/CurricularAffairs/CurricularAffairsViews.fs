namespace CurricularAffairs

open Microsoft.AspNetCore.Http
open Giraffe.GiraffeViewEngine
open Saturn

module Views =

    let index (ctx : HttpContext) = 
        let content = [
            div [_class "container"] [
                h2 [_class "title"] [rawText "Curricular Affairs"]
            ]
        ]
        AuthApp.layout ([section [_class "section"] content ]) ctx