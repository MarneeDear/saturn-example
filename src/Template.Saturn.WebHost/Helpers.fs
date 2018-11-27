module Helpers

let cleanLink (link:string) =
  link.Remove(link.Length - 1, 1)

