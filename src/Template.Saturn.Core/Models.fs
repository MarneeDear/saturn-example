namespace Template.Saturn.Core.Models

open System

module TemplateCoreModels =

    type TemplateCoreUnion =
        | Thing1
        | Thing2
        | Thing3

    type TemplateCore =
        {
            Field1 : string
            Field2 : int64
            Field3 : TemplateCoreUnion
        }