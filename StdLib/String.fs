module String

let replace (oldValue : string) (newValue : string) : string -> string = function
    | null -> ""
    | s -> s.Replace(oldValue, newValue)

let split (separator : string) : string -> string list = function
    | null -> []
    | s -> s.Split separator |> Array.toList

let splitWithChar (separator: char) : string -> string list = function
    | null -> []
    | s -> s.Split separator |> Array.toList

let trim : string -> string = function
    | null -> ""
    | s -> s.Trim()
