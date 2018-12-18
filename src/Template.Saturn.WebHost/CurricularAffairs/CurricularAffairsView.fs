namespace CurricularAffairs

open Microsoft.AspNetCore.Http
open Giraffe.GiraffeViewEngine
open Saturn

module Views =
    //EXAMPLE USING A VIEW MODEL IN THE VIEW
    let getGradYear gradYear =
        match gradYear with
        | Y2018 -> 2018
        | Y2019 -> 2019
        | Y2020 -> 2020
        | Y2021 -> 2021
        | Y2022 -> 2022

    let index (ctx : HttpContext) (course : CourseView)  = 
        let content = [
            div [_class "container"] [
                h2 [_class "title"] [rawText "Curricular Affairs"]
            ]
            div [_class "container"] [
                ul [] [
                    li [] [ rawText "Id:"; rawText (string course.id) ]
                    li [] [ rawText "GradYear:"; rawText (string (getGradYear course.gradYear)) ]
                    li [] [ rawText "Course:"; rawText course.name]
                ]
            ]
        ]
        AuthApp.layout ([section [_class "section"] content ]) ctx