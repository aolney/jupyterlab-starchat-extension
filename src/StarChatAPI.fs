module StarChatAPI

open System
open Fable.Core
open Fable.Core.JsInterop
open Thoth.Json 
open Thoth.Fetch

let mutable endpointOption : string option = None

type CodingAssistanceRequest =
    {
        user_message : string
    }

type CodingAssistanceResponse=
    {
        bot_response : string
    }
///Get coreferences from AllenNLP
let SendMessage( input: string ) : JS.Promise<Result<CodingAssistanceResponse,FetchError>> =
    let endpoint = 
        match endpointOption with
        | Some( endpoint ) -> endpoint
        | None -> "https://starchat.olney.ai/api/getBotResponse"
    promise {
        return! Fetch.tryPost( endpoint , { user_message = input }, caseStrategy = SnakeCase)
}