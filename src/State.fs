module App.State

open Elmish
open App.Types
open Fable.Import
open System

let init _ =
    let now = DateTime.Today
    { persons =
        Map.ofList [
            (1, { name = ""; birthday = now})
            (2, { name = ""; birthday = now})]
      averageBirthday = now }, Cmd.Empty

let update msg model =
    match msg with
    | AddPerson ->
        let lastIndex = model.persons |> Map.toList |> List.map fst |> List.max
        let newMap = Map.add (lastIndex + 1) { name = ""; birthday = DateTime.Now } model.persons
        { model with persons = newMap }, []
    | DeletePerson id ->
        let newMap = Map.filter (fun k _ -> k <> id) model.persons
        { model with persons = newMap }, Cmd.Empty
    | UpdateName (id,name) ->
        let newName k v =
            if k = id then { v with name = name } else v
        { model with persons = Map.map newName model.persons }, []
    | UpdateBirthday (id,date) ->
        let newMap =
            model.persons
            |> Map.map (fun k v -> if k = id then { v with birthday = date } else v)
        let sum =
            newMap
            |> Map.map (fun _ v -> v.birthday.Ticks)
            |> Map.toList
            |> List.sumBy snd

        let av = sum / (int64 model.persons.Count)

        let date = new DateTime(av)

        { model with persons = newMap; averageBirthday = date }, []

