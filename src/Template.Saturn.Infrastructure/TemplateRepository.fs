namespace Template.Infrastructure

open System

module TemplateRepository =

    type RepoUnion =
        | Thing1
        | Thing2
        | Thing3

    type Repo =
        {
            Field1 : string
            Field2 : int64
            Field3 : RepoUnion
        }

