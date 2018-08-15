module App.Types

open System
open Fable.Core

[<Pojo>]
type Person = {
    name: string
    birthday: DateTime
}

type Age = {
    years: int
    months: int
    days: int
} with
    static member empty =
        { years = 0; months = 0; days = 0 }

type Msg =
  | AddPerson
  | DeletePerson of int
  | UpdateName of (int*string)
  | UpdateBirthday of (int*DateTime)

type Model = {
    persons: Map<int, Person>
    averageBirthday: DateTime
}
