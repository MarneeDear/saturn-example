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
                    li [] [ encodedText "Id:"; encodedText (string course.id) ]
                    li [] [ encodedText "GradYear:"; encodedText (string (getGradYear course.gradYear)) ]
                    li [] [ encodedText "Course:"; encodedText course.name]
                    li [] [ a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx course.id ) ] [rawText "Delete"]]

                ]
            ]
        ]
        AuthApp.layout ([section [_class "section"] content ]) ctx