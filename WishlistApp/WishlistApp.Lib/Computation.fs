namespace WishlistApp.Lib

#nowarn "62"

open System
open System.Collections.Generic
open System.Runtime.CompilerServices


type ('a, 'b) computation =
    | In of 'a
    | Just of 'b


[<CompiledName ("FSharpComputation")>]
module Computation =
    let Out f =
        function
        | In v -> f v
        | Just v -> v

    let Try f =
        function
        | (In v) as comp ->
            match f v with
            | Some v -> Just v
            | None -> comp
        | (Just v) as comp -> comp

        
[<Extension>]
type ComputationExtension () =
    [<Extension>]
    static member AsComputation<'a, 'b> value : ('a, 'b) computation =
        In value

    [<Extension>]
    static member AsComputationResult<'a, 'b> value : ('a, 'b) computation =
        Just value

    [<Extension>]
    static member Out (comp, f : Func<_, _>) =
        Computation.Out f.Invoke comp

    [<Extension>]
    static member Try (comp, f : Func<_, _>) =
        Computation.Try f.Invoke comp

