module Helpers

let removeTrailingSlash(link:string) =
  link.Remove(link.Length - 1, 1)

