module App.View

open Elmish
open Fable.Import
open Fable.DateFunctions
open Fable.Core.JsInterop
open App.State
open App.Types
open System

importAll "../sass/main.sass"

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.AST.Fable

module Mui = Fable.Helpers.MaterialUI
module MP = Fable.MaterialUI.Props

let age (date : DateTime) =
    let now = DateTime.Now
    let yearDiff = now.Year - date.Year
    let ageY =
        if new DateTime(now.Year, date.Month, date.Day) >= now then yearDiff - 1 else yearDiff
    let mutable ageM = 0
    let mutable ageD = 0
    while new DateTime(date.Year + ageY, date.Month + ageM + 1, date.Day) <= now do
        ageM <- ageM + 1
    while new DateTime(date.Year + ageY, date.Month + ageM, date.Day + ageD + 1) <= now do
        ageD <- ageD + 1
    { years = ageY; months = ageM; days = ageD }

let ageToString (age: Age) =
    match (age.years, age.months, age.days) with
    | 0,0,0 -> ""
    | 0,0,d when d > 0 -> sprintf "%d days" d
    | 0,m,0 when m > 0 -> sprintf "%d months" m
    | y,0,0 when y > 0 -> sprintf "%d years" y
    | 0,m,d when m > 0 && d > 0 -> sprintf "%d months %d days" m d
    | y,0,d when y > 0 && d > 0 -> sprintf "%d years %d days" y d
    | y,m,0 when y > 0 && m > 0 -> sprintf "%d years %d months" y m
    | y,m,d -> sprintf "%d years %d months %d days" y m d

let gridItem (size: MP.GridSizeNum) (props: IHTMLProp list) content =
    Mui.grid ([ MP.Item true
                MP.GridProp.Xl (U3.Case3 size) ] @ props)
        content

let nameTextField id name dispatch =
    Mui.textField
        [ Id ("name-" + string id)
          DefaultValue name
          Label "Name"
          MP.FullWidth true
          OnChange (fun e -> UpdateName (id, !!e.target?value) |> dispatch ) ]
        []

let birthTextField id (date : DateTime) dispatch =
    Mui.textField
        [ Id ("date-" + string id)
        //   Label "Birthdate"
          Type "date"
          DefaultValue (date.ToString("yyyy-MM-dd"))
          MP.FullWidth true
          OnChange (fun e -> UpdateBirthday (id, DateTime.Parse !!e.target?value) |> dispatch) ]
        []

let ageLabel (date : DateTime) =
    date |> age |> ageToString |> str

let deleteButton id dispatch =
    Mui.button [ MP.ButtonProp.Variant MP.ButtonVariant.Outlined
                 MP.Color MP.ComponentColor.Secondary
                 OnClick (fun _ -> DeletePerson id |> dispatch)]
               [ str "Delete" ]

let textBoxes (persons : Map<int,Person>) dispatch =
    div [ ClassName "textBoxes"]
        (persons |> Map.map (fun k v ->
            Mui.grid [ MP.GridProp.Container true
                       MP.GridProp.Spacing MP.GridSpacing.``24``
                       MP.GridProp.AlignContent MP.GridAlignContent.SpaceAround ] [
                gridItem MP.GridSizeNum.``4`` []
                    [ nameTextField k v.name dispatch ]
                gridItem MP.GridSizeNum.``3`` [ ClassName "flex-bottom" ]
                    [ birthTextField k v.birthday dispatch ]
                gridItem MP.GridSizeNum.``3`` [ ClassName "flex-bottom" ]
                    [ ageLabel v.birthday ]
                gridItem MP.GridSizeNum.``2`` [ ClassName "flex-bottom" ]
                    [ (if persons.Count > 2 then deleteButton k dispatch else null) ]
            ]) |> Map.toList |> List.map snd)

let sumAges (persons: Map<int, Person>) =
    persons
    |> Map.map (fun _ v -> v.birthday |> age)
    |> Map.toList
    |> List.map snd
    |> List.fold (fun s i ->
        { years = i.years + s.years
          months = i.months + s.months
          days = i.days + s.days }) Age.empty
let results (model: Model) =
    Mui.grid [ ClassName "results"
               MP.GridProp.Container true ] [
        gridItem MP.GridSizeNum.``2`` [ ClassName "has-text-weight-semibold" ]
            [ str "Average birth date:" ]
        gridItem MP.GridSizeNum.``10`` []
            [ str (model.averageBirthday.ToShortDateString()) ]
        gridItem MP.GridSizeNum.``2`` [ ClassName "has-text-weight-semibold" ]
            [ str "Average age:" ]
        gridItem MP.GridSizeNum.``10`` []
            [ ageLabel model.averageBirthday ]
        gridItem MP.GridSizeNum.``2`` [ ClassName "has-text-weight-semibold" ]
            [ str "Total age:" ]
        gridItem MP.GridSizeNum.``10`` []
            [ str (model.persons |> sumAges |> ageToString) ] ]


let addButton dispatch =
    Mui.button [ ClassName "is-pulled-right"
                 MP.ButtonProp.Variant MP.ButtonVariant.Contained
                 MP.Color MP.ComponentColor.Primary
                 OnClick (fun _ -> AddPerson |> dispatch )]
        [ Mui.icon [] [ str "add" ]
          str "Add person" ]
let root model dispatch =
    div [ ClassName "wrapper" ] [
        Mui.paper [ ClassName "main" ]
            [ h1 [ ClassName "title is-pulled-left" ] [ str "Average birthday calculator" ]
              addButton dispatch
              div [ ClassName "is-clearfix" ] []
              p [] [ str "Please input the birth dates of people you want to find the average birth date"]
              textBoxes model.persons dispatch
              results model ] ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update root
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "app"
|> Program.run
