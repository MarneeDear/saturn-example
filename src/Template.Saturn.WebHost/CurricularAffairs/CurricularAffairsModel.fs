namespace CurricularAffairs

type GradYear =
    | Y2018
    | Y2019
    | Y2020
    | Y2021
    | Y2022


[<CLIMutable>]
type CourseView = {
    id : int64
    gradYear : GradYear
    name : string
    catalogNumber : int
}  

