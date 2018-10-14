module TemplateTests

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``1 should equal 1`` () =
    1 |> should equal 1

[<Fact>]
let ``1 should not equal 2`` () =
    1 |> should not' (equal 2)
    //raise (new System.Exception("PPOP"))